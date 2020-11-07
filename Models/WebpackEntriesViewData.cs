using System.Collections.Generic;

namespace BundleSink.Models
{
    public class WebpackEntriesViewData {
        public ICollection<string> RequestedEntries { get; private set; } = new List<string>();
        public void AddEntry(string name) {
            if (!RequestedEntries.Contains(name)) {
                RequestedEntries.Add(name);
            }
        }
    }
}