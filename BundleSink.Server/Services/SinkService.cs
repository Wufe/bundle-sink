using System.Linq;
using BundleSink.Models;
using BundleSink.Models.Entry;
using BundleSink.Server.Serializers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace BundleSink.Services
{
    public class SinkService {
        private readonly BundleSinkSettings _settings;
        private readonly IOptionsSnapshot<WebpackEntriesManifest> _webpackManifest;
        private readonly EntriesViewData _viewData;
        private readonly IFileVersionProvider _fileVersionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private Serializer _serializer;

        public SinkService(
            BundleSinkSettings settings,
            IOptionsSnapshot<WebpackEntriesManifest> webpackManifest,
            EntriesViewData viewData,
            IFileVersionProvider fileVersionProvider,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _settings = settings;
            _webpackManifest = webpackManifest;
            _viewData = viewData;
            _fileVersionProvider = fileVersionProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public string SerializeSink(string sinkName) {
            _serializer = new Serializer(
                _settings,
                _webpackManifest,
                _viewData,
                _fileVersionProvider,
                _httpContextAccessor.HttpContext.Request.PathBase,
                sinkName
            );

            var finalOutput = "";

            if (_settings.PrintComments)
            {
                finalOutput += $"<!-- Sink \"{sinkName}\" -->\n";
            }

            foreach (var requestEntry in _viewData.RequestedEntries.Where(x => x.Sink == sinkName))
            {
                finalOutput += GetEntryOutput(requestEntry, sinkName);
            }

            return finalOutput;
        }

        private string GetEntryOutput(IRequestedEntryModel requestedEntry, string sinkName, string dependencyOf = null)
        {
            var finalOutput = "";

            if (_viewData.TryMarkEntryAsSerialized(requestedEntry))
            {

                // Prevent importing if no script requires this one
                if (requestedEntry.RequiredBy.Any())
                {
                    var dependants = requestedEntry.RequiredBy
                        .Where(dependant => _viewData.TryGetRequestedEntryByName(dependant, out var _));
                    if (!dependants.Any())
                    {
                        if (_settings.PrintComments)
                        {
                            finalOutput += $"<!-- Preventing output of \"{requestedEntry.GetIdentifier()}\" because there are no dependants. -->\n";
                        }
                        return finalOutput;
                    }
                }

                // Resolve requirements
                foreach (var requirement in requestedEntry.Requires)
                {
                    IRequestedEntryModel requirementEntry;

                    // Check if the dependency has been requested
                    if (!_viewData.TryGetRequestedEntryByName(requirement, out requirementEntry))
                    {
                        // If not, try build a new request for entry from webpack
                        if (_webpackManifest.Value.ContainsKey(requirement))
                        {
                            var requirementEntryModel = RequestedEntryModel.BuildWebpackEntry(requirement);
                            requirementEntryModel.Sink = sinkName;
                            requirementEntry = requirementEntryModel;
                        }
                    }

                    if (requirementEntry != null)
                    {
                        finalOutput += GetEntryOutput(requirementEntry, sinkName, requestedEntry.Name);
                    }
                    else
                    {
                        if (_settings.PrintComments)
                        {
                            finalOutput += $"<!-- Cannot resolve \"{requirement}\" requested by \"{requestedEntry.Name}\". -->\n";
                        }
                    }
                }

                finalOutput += _serializer.SerializeEntry(requestedEntry, dependencyOf);
            }


            return finalOutput;
        }
    }
}