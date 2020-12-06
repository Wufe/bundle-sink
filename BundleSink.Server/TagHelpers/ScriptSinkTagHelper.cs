using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BundleSink.Models;
using BundleSink.Models.Entry;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BundleSink.TagHelpers
{
    public class ScriptSinkTagHelper : TagHelper {
        private readonly BundleSinkSettings _settings;
        private readonly IOptionsSnapshot<WebpackEntriesManifest> _webpackManifest;
        private readonly WebpackEntriesViewData _webpackViewData;
        private readonly IFileVersionProvider _fileVersionProvider;
        private readonly IWebHostEnvironment _webHostEnvironment;

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        private string _sinkName;

        public ScriptSinkTagHelper(
            BundleSinkSettings settings,
            IOptionsSnapshot<WebpackEntriesManifest> webpackManifest,
            WebpackEntriesViewData webpackViewData,
            IFileVersionProvider fileVersionProvider,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _settings = settings;
            _webpackManifest = webpackManifest;
            _webpackViewData = webpackViewData;
            _fileVersionProvider = fileVersionProvider;
            _webHostEnvironment = webHostEnvironment;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            _sinkName = RequestedEntryModel.DEFAULT_SINK_NAME;

            if (context.AllAttributes.TryGetAttribute("name", out var name)) {
                _sinkName = name.Value.ToString();
            }

            var finalOutput = "";

            if (_settings.PrintComments) {
                finalOutput += $"<!-- Sink \"{_sinkName}\" -->\n";
            }

            foreach (var requestEntry in _webpackViewData.RequestedEntries.Where(x => x.Sink == _sinkName))
            {
                finalOutput += GetEntryOutput(requestEntry);
            }
            
            output.PostElement.AppendHtml(finalOutput);
        }

        private string GetEntryOutput(IRequestedEntryModel requestedEntry, string dependencyOf = null) {
            var finalOutput = "";

            if (_webpackViewData.TryMarkEntryAsSerialized(requestedEntry)) {

                // Prevent importing if no script requires this one
                if (requestedEntry.RequiredBy.Any()) {
                    var dependants = requestedEntry.RequiredBy
                        .Where(dependant => _webpackViewData.TryGetRequestedEntryByName(dependant, out var _));
                    if (!dependants.Any()) {
                        if (_settings.PrintComments) {
                            finalOutput += $"<!-- Preventing output of {requestedEntry.GetIdentifier()} because there are no dependants. -->\n";
                        }
                        return finalOutput;
                    }
                }

                // Resolve requirements
                foreach (var requirement in requestedEntry.Requires)
                {
                    IRequestedEntryModel requirementEntry;

                    // Check if the dependency has been requested
                    if (!_webpackViewData.TryGetRequestedEntryByName(requirement, out requirementEntry))
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

                if (_webpackManifest.Value.TryGetValue(requestedEntry.Name, out var entryFiles))
                {
                    finalOutput += SerializeEntryJS(requestedEntry, entryFiles, dependencyOf);
                    finalOutput += SerializeEntryCSS(requestedEntry, entryFiles, dependencyOf);
                }
            }

            
            return finalOutput;
        }

        private string SerializeEntryJS(IRequestedEntryModel requestedEntry, WebpackEntryModel webpackEntry, string dependencyOf = null) {
            var finalOutput = "";
            if (!requestedEntry.CSSOnly)
            {
                for (var i = 0; i < webpackEntry.Assets.JS.Count; i++)
                {
                    var file = webpackEntry.Assets.JS[i];

                    var fileIsADependency = i < webpackEntry.Assets.JS.Count - 1;

                    var serializedFileSearchKey = file;

                    if (!fileIsADependency && requestedEntry.Key != null)
                        serializedFileSearchKey += requestedEntry.Key;

                    if (_webpackViewData.TryMarkFileAsSerialized(serializedFileSearchKey))
                    {
                        var async = !fileIsADependency && requestedEntry.Async ? "async" : "";
                        var defer = !fileIsADependency && requestedEntry.Defer ? "defer" : "";

                        var filePath = Path.Combine("/", _settings.PublicOutputPath, file);

                        if (_settings.AppendVersion)
                            filePath = _fileVersionProvider.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, filePath);

                        var comments = "";
                        if (_settings.PrintComments) {
                            // Requested due to being a dependency
                            if (requestedEntry.Sink != _sinkName && !string.IsNullOrEmpty(dependencyOf)) {
                                comments += $"<!-- The following entry should be printed into \"{requestedEntry.Sink}\" sink. However it has been requested as a dependency of \"{dependencyOf}\". -->\n";
                            }
                        }

                        finalOutput += string.Format(
                            "{0}<script type=\"text/javascript\" src=\"{1}\"{2}{3}{4}></script>\n",
                            comments,
                            filePath,
                            !string.IsNullOrEmpty(async) ? " " + async : "",
                            !string.IsNullOrEmpty(defer) ? " " + defer : "",
                            _settings.PrintAllAttributes ? SerializeEntryAttributes(requestedEntry, dependencyOf) : ""
                        );
                    }
                }
            }
            return finalOutput;
        }

        private string SerializeEntryCSS(IRequestedEntryModel requestedEntry, WebpackEntryModel webpackEntry, string dependencyOf = null)
        {
            var finalOutput = "";
            if (!requestedEntry.JSOnly)
            {
                for (var i = 0; i < webpackEntry.Assets.CSS.Count; i++)
                {
                    var file = webpackEntry.Assets.CSS[i];

                    var serializedFileSearchKey = file;

                    if (requestedEntry.Key != null)
                        serializedFileSearchKey += requestedEntry.Key;

                    if (_webpackViewData.TryMarkFileAsSerialized(serializedFileSearchKey))
                    {
                        var filePath = Path.Combine("/", _settings.PublicOutputPath, file);

                        if (_settings.AppendVersion)
                            filePath = _fileVersionProvider.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, filePath);

                        var comments = "";
                        if (_settings.PrintComments)
                        {
                            // Requested due to being a dependency
                            if (requestedEntry.Sink != _sinkName && !string.IsNullOrEmpty(dependencyOf))
                            {
                                comments += $"<!-- The following entry should be printed into \"{requestedEntry.Sink}\" sink. However it has been requested as a dependency of \"{dependencyOf}\". -->\n";
                            }
                        }

                        finalOutput += string.Format(
                            "{0}<link rel=\"stylesheet\" href=\"{1}\"{2} />\n",
                            comments,
                            filePath,
                            _settings.PrintAllAttributes ? SerializeEntryAttributes(requestedEntry, dependencyOf) : ""
                        );
                    }
                }
            }
            return finalOutput;
        }

        private string SerializeEntryAttributes(IRequestedEntryModel requestedEntry, string dependencyOf = null) {
            var shouldPrintKeyAttribute = !string.IsNullOrEmpty(requestedEntry.Key);
            var shouldPrintSinkAttribute = !string.IsNullOrEmpty(requestedEntry.Sink);
            var shouldPrintRequestedBy = !string.IsNullOrEmpty(dependencyOf);
            var shouldPrintRequiresAttribute = requestedEntry.Requires.Any();
            var shouldPrintRequiredByAttribute = requestedEntry.RequiredBy.Any();
            var shouldPrintCSSOnlyAttribute = requestedEntry.CSSOnly;
            var shouldPrintJSOnlyAttribute = requestedEntry.JSOnly;

            return string.Format(
                "{0}{1}{2}{3}{4}{5}{6}{7}",
                $" entry=\"{requestedEntry.Name}\"",
                shouldPrintKeyAttribute ? $" key=\"{requestedEntry.Key}\"" : "",
                shouldPrintSinkAttribute ? $" sink=\"{requestedEntry.Sink}\"" : "",
                shouldPrintRequestedBy ? $" dependency-of=\"{dependencyOf}\"" : "",
                shouldPrintRequiresAttribute ? $" requires=\"{String.Join(',', requestedEntry.Requires)}\"" : "",
                shouldPrintRequiredByAttribute ? $" required-by=\"{String.Join(',', requestedEntry.RequiredBy)}\"" : "",
                shouldPrintCSSOnlyAttribute ? $" css-only" : "",
                shouldPrintJSOnlyAttribute ? $" js-only" : ""
            );
        }
    }
}