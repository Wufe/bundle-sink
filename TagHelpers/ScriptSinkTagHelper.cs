using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BundleSink.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace BundleSink.TagHelpers
{
    public class ScriptSinkTagHelper : TagHelper {
        private readonly BundleSinkSettings _settings;
        private readonly IOptionsSnapshot<WebpackEntriesManifest> _webpackManifest;
        private readonly WebpackEntriesViewData _webpackViewData;

        public ScriptSinkTagHelper(
            BundleSinkSettings settings,
            IOptionsSnapshot<WebpackEntriesManifest> webpackManifest,
            WebpackEntriesViewData webpackViewData
        )
        {
            _settings = settings;
            _webpackManifest = webpackManifest;
            _webpackViewData = webpackViewData;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            var sinkName = RequestedEntryModel.DEFAULT_SINK_NAME;

            if (context.AllAttributes.TryGetAttribute("name", out var name)) {
                sinkName = name.Value.ToString();
            }

            var finalOutput = "";

            var serializedFilesWithinThisSink = new List<string>();
            foreach (var requestEntry in _webpackViewData.RequestedEntries.Where(x => x.Sink == sinkName))
            {
                if (_webpackManifest.Value.TryGetValue(requestEntry.Name, out var entryFiles)) {
                    for (var i = 0; i < entryFiles.JS.Count; i++) {
                        var file = entryFiles.JS[i];

                        var fileIsADependency = i < entryFiles.JS.Count -1;

                        var serializedFileSearchKey = file;

                        if (!fileIsADependency && requestEntry.Key != null)
                            serializedFileSearchKey += requestEntry.Key;

                        if (!serializedFilesWithinThisSink.Contains(serializedFileSearchKey) &&
                            _webpackViewData.TryMarkFileAsSerialized(serializedFileSearchKey))
                        {
                            var async = !fileIsADependency && requestEntry.Async ? "async" : "";
                            var defer = !fileIsADependency && requestEntry.Defer ? "defer" : "";

                            var filePath = Path.Combine("/", _settings.PublicOutputPath, file);

                            finalOutput += string.Format(
                                "<script type=\"text/javascript\" src=\"{0}\"{1}{2}></script>\n",
                                filePath,
                                !string.IsNullOrEmpty(async) ? " " + async : "",
                                !string.IsNullOrEmpty(defer) ? " " + defer : ""
                            );
                        }
                    }
                }
            }
            
            output.PostElement.AppendHtml(finalOutput);
        }
    }
}