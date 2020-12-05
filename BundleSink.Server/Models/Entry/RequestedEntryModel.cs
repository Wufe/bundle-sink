using System.Collections.Generic;

namespace BundleSink.Models.Entry
{
    public class RequestedEntryModel : IRequestedEntryModel, IRequestedWebpackEntryModel
    {
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

        public bool FromWebpack { get; private set; } = false;

        public static RequestedEntryModel BuildWebpackEntry(string name) {
            return new RequestedEntryModel() {
                Name = name,
                FromWebpack = true
            };
        }

        public bool IsWebpackEntry(out IRequestedWebpackEntryModel webpackEntryModel)
        {
            if (FromWebpack) {
                webpackEntryModel = this;
                return true;
            }
            webpackEntryModel = null;
            return false;
        }
    }
}