using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BundleSink.Models;
using BundleSink.Models.Entry;
using BundleSink.Server.Serializers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BundleSink.TagHelpers
{
    [HtmlTargetElement("script-sink")]
    [HtmlTargetElement("sink")]
    public class ScriptSinkTagHelper : TagHelper {
        private readonly BundleSinkSettings _settings;
        private readonly IOptionsSnapshot<WebpackEntriesManifest> _webpackManifest;
        private readonly EntriesViewData _viewData;
        private readonly IFileVersionProvider _fileVersionProvider;
        private readonly IWebHostEnvironment _webHostEnvironment;

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        private string _sinkName;

        private Serializer _serializer;

        public ScriptSinkTagHelper(
            BundleSinkSettings settings,
            IOptionsSnapshot<WebpackEntriesManifest> webpackManifest,
            EntriesViewData viewData,
            IFileVersionProvider fileVersionProvider,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _settings = settings;
            _webpackManifest = webpackManifest;
            _viewData = viewData;
            _fileVersionProvider = fileVersionProvider;
            _webHostEnvironment = webHostEnvironment;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            _sinkName = RequestedEntryModel.DEFAULT_SINK_NAME;

            _serializer = new Serializer(
                _settings,
                _webpackManifest,
                _viewData,
                _fileVersionProvider,
                ViewContext.HttpContext.Request.PathBase,
                _sinkName
            );

            if (context.AllAttributes.TryGetAttribute("name", out var name)) {
                _sinkName = name.Value.ToString();
            }

            var finalOutput = "";

            if (_settings.PrintComments) {
                finalOutput += $"<!-- Sink \"{_sinkName}\" -->\n";
            }

            foreach (var requestEntry in _viewData.RequestedEntries.Where(x => x.Sink == _sinkName))
            {
                finalOutput += GetEntryOutput(requestEntry);
            }
            
            output.PostElement.AppendHtml(finalOutput);
        }

        private string GetEntryOutput(IRequestedEntryModel requestedEntry, string dependencyOf = null) {
            var finalOutput = "";

            if (_viewData.TryMarkEntryAsSerialized(requestedEntry)) {

                // Prevent importing if no script requires this one
                if (requestedEntry.RequiredBy.Any()) {
                    var dependants = requestedEntry.RequiredBy
                        .Where(dependant => _viewData.TryGetRequestedEntryByName(dependant, out var _));
                    if (!dependants.Any()) {
                        if (_settings.PrintComments) {
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
                        if (_webpackManifest.Value.ContainsKey(requirement)) {
                            var requirementEntryModel = RequestedEntryModel.BuildWebpackEntry(requirement);
                            requirementEntryModel.Sink = _sinkName;
                            requirementEntry = requirementEntryModel;
                        }
                    }

                    if (requirementEntry != null)
                    {
                        finalOutput += GetEntryOutput(requirementEntry, requestedEntry.Name);
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