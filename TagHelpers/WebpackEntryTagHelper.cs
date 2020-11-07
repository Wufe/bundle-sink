using BundleSink.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace BundleSink.TagHelpers
{
    public class WebpackEntryTagHelper : TagHelper {
        private readonly IOptionsSnapshot<WebpackEntriesManifest> _webpackManifest;
        private readonly WebpackEntriesViewData _webpackViewData;

        public WebpackEntryTagHelper(
            IOptionsSnapshot<WebpackEntriesManifest> webpackManifest,
            WebpackEntriesViewData webpackViewData
        )
        {
            _webpackManifest = webpackManifest;
            _webpackViewData = webpackViewData;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            if (context.AllAttributes.TryGetAttribute("name", out var attribute)) {
                var requestedEntryName = attribute.Value.ToString();
                if (_webpackManifest.Value.ContainsKey(requestedEntryName)) {
                    _webpackViewData.AddEntry(requestedEntryName);
                }
            }
        }

    }
}