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
            foreach (var requestEntry in _webpackViewData.RequestedEntries.Where(x => x.Sink == _sinkName))
            {
                finalOutput += GetEntryOutput(requestEntry);
            }
            
            output.PostElement.AppendHtml(finalOutput);
        }

        private string GetEntryOutput(IRequestedEntryModel requestedEntry) {
            var finalOutput = "";

            if (_webpackViewData.TryMarkEntryAsSerialized(requestedEntry)) {

                // Prevent importing if no script requires this one
                if (requestedEntry.RequiredBy.Any()) {
                    var dependants = requestedEntry.RequiredBy
                        .Where(dependant => _webpackViewData.TryGetRequestedEntry(dependant, out var _));
                    if (!dependants.Any()) {
                        if (_webHostEnvironment.IsDevelopment()) {
                            finalOutput += $"<!-- Preventing output of {requestedEntry.GetIdentifier()} because there are no dependants. -->\n";
                        }
                        return finalOutput;
                    }   
                }
                
                // Resolve requirements
                foreach (var requirement in requestedEntry.Requires)
                {
                    IRequestedEntryModel requirementEntry;
                    if (!_webpackViewData.TryGetRequestedEntry(requirement, out requirementEntry))
                    {
                        var requirementEntryModel = RequestedEntryModel.BuildWebpackEntry(requirement);
                        requirementEntryModel.Sink = _sinkName;
                        requirementEntry = requirementEntryModel;
                    }
                    finalOutput += GetEntryOutput(requirementEntry);
                }

                if (_webpackManifest.Value.TryGetValue(requestedEntry.Name, out var entryFiles))
                {
                    finalOutput += SerializeEntryJS(requestedEntry, entryFiles);
                    finalOutput += SerializeEntryCSS(requestedEntry, entryFiles);
                }
            }

            
            return finalOutput;
        }

        private string SerializeEntryJS(IRequestedEntryModel requestedEntry, WebpackEntryModel webpackEntry) {
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

                        finalOutput += string.Format(
                            "<script type=\"text/javascript\" src=\"{0}\"{1}{2}{3}></script>\n",
                            filePath,
                            !string.IsNullOrEmpty(async) ? " " + async : "",
                            !string.IsNullOrEmpty(defer) ? " " + defer : "",
                            _settings.PrintAllAttributes ? SerializeEntryAttributes(requestedEntry) : ""
                        );
                    }
                }
            }
            return finalOutput;
        }

        private string SerializeEntryCSS(IRequestedEntryModel requestedEntry, WebpackEntryModel webpackEntry)
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

                        finalOutput += string.Format(
                            "<link rel=\"stylesheet\" href=\"{0}\"{1}></script>\n",
                            filePath,
                            _settings.PrintAllAttributes ? SerializeEntryAttributes(requestedEntry) : ""
                        );
                    }
                }
            }
            return finalOutput;
        }

        private string SerializeEntryAttributes(IRequestedEntryModel requestedEntry) {
            var shouldPrintKeyAttribute = !string.IsNullOrEmpty(requestedEntry.Key);
            var shouldPrintSinkAttribute = !string.IsNullOrEmpty(requestedEntry.Sink);
            var shouldPrintRequiresAttribute = requestedEntry.Requires.Any();
            var shouldPrintRequiredByAttribute = requestedEntry.RequiredBy.Any();
            var shouldPrintCSSOnlyAttribute = requestedEntry.CSSOnly;
            var shouldPrintJSOnlyAttribute = requestedEntry.JSOnly;

            return string.Format(
                "{0}{1}{2}",
                $" entry=\"{requestedEntry.Name}\"",
                shouldPrintKeyAttribute ? $" key=\"{requestedEntry.Key}\"" : "",
                shouldPrintSinkAttribute ? $" sink=\"{requestedEntry.Sink}\"" : "",
                shouldPrintRequiresAttribute ? $" requires=\"{String.Join(',', requestedEntry.Requires)}\"" : "",
                shouldPrintRequiredByAttribute ? $" required-by=\"{String.Join(',', requestedEntry.RequiredBy)}\"" : "",
                shouldPrintCSSOnlyAttribute ? $" css-only" : "",
                shouldPrintJSOnlyAttribute ? $" js-only" : ""
            );
        }
    }
}