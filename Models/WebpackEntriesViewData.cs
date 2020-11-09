using System.Collections.Generic;

namespace BundleSink.Models
{
    public class WebpackEntriesViewData {
        public ICollection<string> SerializedFiles { get; private set; } = new List<string>();
        public ICollection<RequestedEntryModel> RequestedEntries { get; private set; } = new List<RequestedEntryModel>();
        public void AddEntry(RequestedEntryModel entry) {
            if (!RequestedEntries.Contains(entry)) {
                RequestedEntries.Add(entry);
            }
        }

        public bool TryMarkFileAsSerialized(string name) {
            if (!SerializedFiles.Contains(name)) {
                SerializedFiles.Add(name);
                return true;
            }
            return false;
        }


    }

    public class RequestedEntryModel {

        public static string DEFAULT_SINK_NAME = "Default";

        public string Name { get; set; }
        public string Key { get; set; }
        public string Sink { get; set; } = DEFAULT_SINK_NAME;
        public bool Async { get; set; } = false;
        public bool Defer { get; set; } = false;
    }
}