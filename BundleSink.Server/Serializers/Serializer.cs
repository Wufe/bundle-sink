using System;
using System.Linq;
using BundleSink.Models;
using BundleSink.Models.Entry;
using BundleSink.Services;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace BundleSink.Server.Serializers
{
    public partial class Serializer {
        private readonly BundleSinkSettings _settings;
        private readonly EntriesManifest _webpackManifest;
        private readonly EntriesViewData _viewData;
        private readonly IFileVersionProvider _fileVersionProvider;
        private readonly string _pathBase;
        private readonly string _sinkName;


        public Serializer(
            BundleSinkSettings settings,
            EntriesManifest webpackManifest,
            EntriesViewData viewData,
            IFileVersionProvider fileVersionProvider,
            string pathBase,
            string sinkName
        )
        {
            _settings = settings;
            _webpackManifest = webpackManifest;
            _viewData = viewData;
            _fileVersionProvider = fileVersionProvider;
            _pathBase = pathBase;
            _sinkName = sinkName;
        }

        public string SerializeEntry(IRequestedEntryModel requestedEntry, string dependencyOf = null) {
            if (requestedEntry.IsWebpackEntry(out var webpackEntry))
            {
                return Serialize(webpackEntry, dependencyOf);
            }
            else if (requestedEntry.IsLiteralEntry(out var literalEntry))
            {
                return Serialize(literalEntry, dependencyOf);
            }
            return "";
        }

        private string SerializeEntryAttributes(IRequestedEntryModel requestedEntry, string dependencyOf = null)
        {
            var shouldPrintEntryAttribute = !string.IsNullOrEmpty(requestedEntry.Name);
            var shouldPrintKeyAttribute = !string.IsNullOrEmpty(requestedEntry.Key);
            var shouldPrintSinkAttribute = !string.IsNullOrEmpty(requestedEntry.Sink);
            var shouldPrintRequestedBy = !string.IsNullOrEmpty(dependencyOf);
            var shouldPrintRequiresAttribute = requestedEntry.Requires.Any();
            var shouldPrintRequiredByAttribute = requestedEntry.RequiredBy.Any();
            var shouldPrintCSSOnlyAttribute = requestedEntry.CSSOnly;
            var shouldPrintJSOnlyAttribute = requestedEntry.JSOnly;

            return string.Format(
                "{0}{1}{2}{3}{4}{5}{6}{7}",
                shouldPrintEntryAttribute ? $" entry=\"{requestedEntry.Name}\"" : "",
                shouldPrintKeyAttribute ? $" key=\"{requestedEntry.Key}\"" : "",
                shouldPrintSinkAttribute ? $" sink=\"{requestedEntry.Sink}\"" : "",
                shouldPrintRequestedBy ? $" dependency-of=\"{dependencyOf}\"" : "",
                shouldPrintRequiresAttribute ? $" requires=\"{string.Join(',', requestedEntry.Requires)}\"" : "",
                shouldPrintRequiredByAttribute ? $" required-by=\"{string.Join(',', requestedEntry.RequiredBy)}\"" : "",
                shouldPrintCSSOnlyAttribute ? $" css-only" : "",
                shouldPrintJSOnlyAttribute ? $" js-only" : ""
            );
        }
    }
}