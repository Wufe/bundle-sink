using System.Collections.Generic;

namespace BundleSink.Models
{
    public class WebpackEntryModel {
        public WebpackEntryAssets Assets { get; set; } = new WebpackEntryAssets();
    }

    public class WebpackEntryAssets {
        public IList<string> JS { get; set; } = new List<string>();
        public IList<string> CSS { get; set; } = new List<string>();
    }
}