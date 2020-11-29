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
            if (context.AllAttributes.TryGetAttribute("name", out var name)) {
                var requestedEntryModel = name.Value.ToString();
                if (_webpackManifest.Value.ContainsKey(requestedEntryModel)) {

                    var entry = new RequestedEntryModel() {
                        Name = requestedEntryModel
                    };

                    if (context.AllAttributes.TryGetAttribute("key", out var key)) {
                        entry.Key = key.Value.ToString();
                    }

                    if (context.AllAttributes.TryGetAttribute("sink", out var sink)) {
                        entry.Sink = sink.Value.ToString();
                    }

                    if (context.AllAttributes.TryGetAttribute("async", out var async)) {
                        entry.Async = true;
                    }

                    if (context.AllAttributes.TryGetAttribute("defer", out var defer)) {
                        entry.Defer = true;
                    }

                    if (context.AllAttributes.TryGetAttribute("requires", out var requires)) {
                        entry.Requires = requires.Value.ToString().Split(',');
                    }

                    if (context.AllAttributes.TryGetAttribute("required-by", out var requiredBy)) {
                        entry.RequiredBy = requiredBy.Value.ToString().Split(',');
                    }

                    _webpackViewData.AddEntry(entry);
                }
            }
        }

    }
}