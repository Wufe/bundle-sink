using System.Collections.Generic;

namespace BundleSink.Models
{
    public class WebpackEntryModel {
        public IList<string> JS { get; set; } = new List<string>();
    }
}