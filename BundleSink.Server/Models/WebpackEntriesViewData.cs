using System.Collections.Generic;
using System.Linq;

namespace BundleSink.Models
{
    public class WebpackEntriesViewData {
        public ICollection<string> SerializedFiles { get; private set; } = new List<string>();
        public ICollection<RequestedEntryModel> RequestedEntries { get; private set; } = new List<RequestedEntryModel>();
        public void AddEntry(RequestedEntryModel entry) {
            if (!EntryAlreadyAdded(entry)) {
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

        private bool EntryAlreadyAdded(RequestedEntryModel entry) {
            return RequestedEntries.Any(x =>
                x.Name == entry.Name &&
                x.Key == entry.Key);
        }

    }

    public class RequestedEntryModel {

        public static string DEFAULT_SINK_NAME = "Default";

        public string Name { get; set; }
        public string Key { get; set; }
        public string Sink { get; set; } = DEFAULT_SINK_NAME;
        public bool Async { get; set; } = false;
        public bool Defer { get; set; } = false;
        public bool CSSOnly { get; set; } = false;
        public bool JSOnly { get; set; } = false;
        public ICollection<string> Requires { get; set; } = new string[] { };
        public ICollection<string> RequiredBy { get; set; } = new string[] { };
    }
}