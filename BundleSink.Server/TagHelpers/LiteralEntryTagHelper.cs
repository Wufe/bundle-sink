using System.Threading.Tasks;
using BundleSink.Models;
using BundleSink.Models.Entry;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BundleSink.TagHelpers
{
    public class LiteralEntryTagHelper : TagHelper {
        private readonly EntriesViewData _viewData;

        public LiteralEntryTagHelper(
            EntriesViewData viewData
        )
        {
            _viewData = viewData;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {

            var content = (await output.GetChildContentAsync()).GetContent();

            output.TagName = null;
            output.SuppressOutput();

            var entry = RequestedEntryModel.BuildLiteralEntry(content);

            if (context.AllAttributes.TryGetAttribute("name", out var name)) {
                entry.Name = name.Value.ToString();
            }

            if (context.AllAttributes.TryGetAttribute("key", out var key)) {
                entry.Key = key.Value.ToString();
            }

            if (context.AllAttributes.TryGetAttribute("sink", out var sink))
            {
                entry.Sink = sink.Value.ToString();
            }

            if (context.AllAttributes.TryGetAttribute("requires", out var requires))
            {
                entry.Requires = requires.Value.ToString().Split(',');
            }

            if (context.AllAttributes.TryGetAttribute("required-by", out var requiredBy))
            {
                entry.RequiredBy = requiredBy.Value.ToString().Split(',');
            }

            _viewData.AddEntry(entry);
        }
    }
}