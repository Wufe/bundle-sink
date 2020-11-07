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
            // TODO: null check / existence check
            var files = new List<string>();
            foreach (var requestEntry in _webpackViewData.RequestedEntries)
            {
                var entryFiles = _webpackManifest.Value[requestEntry];
                foreach (var file in entryFiles.JS)
                {
                    if (!files.Contains(file)) {
                        files.Add(file);
                    }
                }
            }
            var scripts = files.Select(file => {
                var scriptPath = Path.Combine("/", _settings.PublicOutputPath, file);
                return $"<script type=\"text/javascript\" src=\"{scriptPath}\"></script>";
            });
            
            output.PostElement.AppendHtml(string.Join("\n", scripts));
        }
    }
}