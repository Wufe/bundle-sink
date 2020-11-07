using System.Collections.Generic;

namespace BundleSink.Models
{
    public class WebpackEntriesManifest : Dictionary<string, WebpackEntryModel> {
        public static string SECTION_NAME = "entrypoints";
    }
}