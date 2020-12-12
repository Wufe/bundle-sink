using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BundleSink.Models.Entry
{
    public class RequestedEntryModel : IRequestedEntryModel, IRequestedWebpackEntryModel, IRequestedLiteralEntryModel
    {
        public static string DEFAULT_SINK_NAME = "Default";
        private static Regex _replaceRegex = new Regex(@"[\s\r\n\t]");

        public string GetIdentifier() {
            if (IsWebpackEntry(out var webpackEntry)) {
                return $"{webpackEntry.Name}{webpackEntry.Key}";
            } else if (IsLiteralEntry(out var literalEntry)) {
                return $"{literalEntry.LiteralHash}{literalEntry.Key}";
            }
            return "";
        }

        public string Name { get; set; }
        public string Content { get; set; } = "";
        public string Key { get; set; } = "";
        public string Sink { get; set; } = DEFAULT_SINK_NAME;
        public bool Async { get; set; } = false;
        public bool Defer { get; set; } = false;
        public bool CSSOnly { get; set; } = false;
        public bool JSOnly { get; set; } = false;
        public ICollection<string> Requires { get; set; } = new string[] { };
        public ICollection<string> RequiredBy { get; set; } = new string[] { };

        public bool FromWebpack { get; private set; } = false;
        public bool Literal { get; private set; } = false;
        public string LiteralHash { get; set; } = "";

        public static RequestedEntryModel BuildWebpackEntry(string name) {
            return new RequestedEntryModel() {
                Name = name,
                FromWebpack = true
            };
        }

        public static RequestedEntryModel BuildLiteralEntry(string content) {
            return new RequestedEntryModel() {
                Content = content,
                Literal = true,
                LiteralHash = CalculateLiteralHash(content).ToString()
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

        public bool IsLiteralEntry(out IRequestedLiteralEntryModel literalEntryModel)
        {
            if (Literal)
            {
                literalEntryModel = this;
                return true;
            }
            literalEntryModel = null;
            return false;
        }

        private static ulong CalculateLiteralHash(string read)
        {
            read = _replaceRegex.Replace(read.Trim(), "");
            ulong hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }

        public bool Processed { get; set; } = false;

        public void MarkAsProcessed(bool processed = true)
        {
            Processed = processed;
        }
    }
}