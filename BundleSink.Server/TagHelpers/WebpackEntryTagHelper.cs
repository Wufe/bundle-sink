using BundleSink.Models;
using BundleSink.Models.Entry;
using BundleSink.Services;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace BundleSink.TagHelpers
{
    public class WebpackEntryTagHelper : TagHelper {
        private readonly EntriesManifest _entriesManifest;
        private readonly EntriesViewData _viewData;

        public WebpackEntryTagHelper(
            EntriesManifest entriesManifest,
            EntriesViewData viewData
        )
        {
            _entriesManifest = entriesManifest;
            _viewData = viewData;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            if (context.AllAttributes.TryGetAttribute("name", out var name)) {
                var entryName = name.Value.ToString();
                if (_entriesManifest.Value.ContainsKey(entryName)) {

                    var entry = RequestedEntryModel.BuildWebpackEntry(entryName);

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

                    if (context.AllAttributes.TryGetAttribute("css-only", out var cssOnly)) {
                        entry.CSSOnly = true;
                    }

                    if (context.AllAttributes.TryGetAttribute("js-only", out var jsOnly)) {
                        entry.JSOnly = true;
                    }

                    if (context.AllAttributes.TryGetAttribute("requires", out var requires)) {
                        entry.Requires = requires.Value.ToString().Split(',');
                    }

                    if (context.AllAttributes.TryGetAttribute("required-by", out var requiredBy)) {
                        entry.RequiredBy = requiredBy.Value.ToString().Split(',');
                    }

                    _viewData.AddEntry(entry);
                }
            }
        }

    }
}