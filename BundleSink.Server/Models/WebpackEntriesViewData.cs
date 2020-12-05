using System.Collections.Generic;
using System.Linq;

namespace BundleSink.Models
{
    public class WebpackEntriesViewData {
        public IDictionary<string, bool> SerializedFiles { get; private set; } = new Dictionary<string, bool>();

        // A Collection to iterate over
        public ICollection<RequestedEntryModel> RequestedEntries { get; private set; } = new List<RequestedEntryModel>();
        // A Dictionary for fast checking
        public IDictionary<string, RequestedEntryModel> RequestedEntriesDictionary { get; set; } = new Dictionary<string, RequestedEntryModel>();
        public void AddEntry(RequestedEntryModel entry) {
            var identifier = entry.GetIdentifier();
            if (!RequestedEntriesDictionary.ContainsKey(identifier)) {
                RequestedEntries.Add(entry);
                RequestedEntriesDictionary.Add(identifier, entry);
            }
        }

        public bool TryGetRequestedEntry(string name, out RequestedEntryModel requestedEntry) {
            return RequestedEntriesDictionary.TryGetValue(name, out requestedEntry);
        }

        public bool TryMarkFileAsSerialized(string name) {
            if (!SerializedFiles.ContainsKey(name)) {
                SerializedFiles.Add(name, true);
                return true;
            }
            return false;
        }

        public IDictionary<string, RequestedEntryModel> SerializedEntries { get; private set; } =
            new Dictionary<string, RequestedEntryModel>();
        public bool TryMarkEntryAsSerialized(RequestedEntryModel requestedEntry) {
            var identifier = requestedEntry.GetIdentifier();
            if (!SerializedEntries.ContainsKey(identifier)) {
                SerializedEntries.Add(identifier, requestedEntry);
                return true;
            }
            return false;
        }

    }

    public class RequestedEntryModel {

        public static string DEFAULT_SINK_NAME = "Default";

        public string GetIdentifier() => $"{Name}{Key}";

        public string Name { get; set; }
        public string Key { get; set; } = "";
        public string Sink { get; set; } = DEFAULT_SINK_NAME;
        public bool Async { get; set; } = false;
        public bool Defer { get; set; } = false;
        public bool CSSOnly { get; set; } = false;
        public bool JSOnly { get; set; } = false;
        public ICollection<string> Requires { get; set; } = new string[] { };
        public ICollection<string> RequiredBy { get; set; } = new string[] { };
    }
}