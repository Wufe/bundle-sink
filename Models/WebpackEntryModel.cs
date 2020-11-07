using System.Collections.Generic;

namespace BundleSink.Models
{
    public class WebpackEntryModel {
        public ICollection<string> JS { get; set; } = new List<string>();
    }
}