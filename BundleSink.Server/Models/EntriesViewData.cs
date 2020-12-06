using System.Collections.Generic;
using System.Linq;
using BundleSink.Models.Entry;

namespace BundleSink.Models
{
    public class EntriesViewData {
        public IDictionary<string, bool> SerializedFiles { get; private set; } = new Dictionary<string, bool>();

        // A Collection to iterate over
        public ICollection<IRequestedEntryModel> RequestedEntries { get; private set; } = new List<IRequestedEntryModel>();
        // A Dictionary for fast checking
        public IDictionary<string, IRequestedEntryModel> RequestedEntriesDictionaryByIdentifier { get; private set; } = new Dictionary<string, IRequestedEntryModel>();
        public IDictionary<string, IRequestedEntryModel> RequestedEntriesDictionaryByName { get; private set; } = new Dictionary<string, IRequestedEntryModel>();
        public void AddEntry(RequestedEntryModel entry) {
            var identifier = entry.GetIdentifier();
            if (!RequestedEntriesDictionaryByIdentifier.ContainsKey(identifier)) {
                RequestedEntries.Add(entry);
                RequestedEntriesDictionaryByIdentifier.Add(identifier, entry);
                RequestedEntriesDictionaryByName.TryAdd(entry.Name ?? identifier, entry);
            }
        }

        public bool TryGetRequestedEntryByIdentifier(string identifier, out IRequestedEntryModel requestedEntry) {
            return RequestedEntriesDictionaryByIdentifier.TryGetValue(identifier, out requestedEntry);
        }

        public bool TryGetRequestedEntryByName(string name, out IRequestedEntryModel requestedEntry) {
            return RequestedEntriesDictionaryByName.TryGetValue(name, out requestedEntry);
        }

        public bool TryMarkFileAsSerialized(string name) {
            if (!SerializedFiles.ContainsKey(name)) {
                SerializedFiles.Add(name, true);
                return true;
            }
            return false;
        }

        public IDictionary<string, IRequestedEntryModel> SerializedEntries { get; private set; } =
            new Dictionary<string, IRequestedEntryModel>();
        public bool TryMarkEntryAsSerialized(IRequestedEntryModel requestedEntry) {
            var identifier = requestedEntry.GetIdentifier();
            if (!SerializedEntries.ContainsKey(identifier)) {
                SerializedEntries.Add(identifier, requestedEntry);
                return true;
            }
            return false;
        }

    }
}