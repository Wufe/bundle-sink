using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BundleSink.Models;
using BundleSink.Models.Entry;
using BundleSink.Server.Serializers;
using BundleSink.Services;
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
        private readonly SinkService _sinkService;

        public ScriptSinkTagHelper(
            BundleSinkSettings settings,
            SinkService sinkService
        )
        {
            _settings = settings;
            _sinkService = sinkService;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var sinkName = RequestedEntryModel.DEFAULT_SINK_NAME;

            if (context.AllAttributes.TryGetAttribute("name", out var name)) {
                sinkName = name.Value.ToString();
            }

            var finalOutput = "";

            if (_settings.RewriteOutput) {
                finalOutput = $@"<sink name=""{sinkName}"" temp />";
            } else {
                finalOutput = _sinkService.SerializeSink(sinkName);
            }

            output.TagName = null;
            output.PostElement.AppendHtml(finalOutput);
        }

        
    }
}