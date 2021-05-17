using BundleSink.Models;
using Microsoft.Extensions.Options;

namespace BundleSink.Services
{
    public class EntriesManifest {
        public EntriesManifest(WebpackEntriesManifest manifest)
        {
            Value = manifest;
        }
        public WebpackEntriesManifest Value { get; private set; }
    }
}