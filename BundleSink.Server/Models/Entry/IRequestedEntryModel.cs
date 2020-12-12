using System.Collections.Generic;

namespace BundleSink.Models.Entry
{
    public interface IRequestedEntryModel
    {
        string GetIdentifier();
        string Name { get; }
        string Key { get; }
        string Sink { get; }
        bool Async { get; }
        bool Defer { get; }
        bool CSSOnly { get; }
        bool JSOnly { get; }
        ICollection<string> Requires { get; }
        ICollection<string> RequiredBy { get; }

        bool IsWebpackEntry(out IRequestedWebpackEntryModel webpackEntryModel);
        bool IsLiteralEntry(out IRequestedLiteralEntryModel literalEntryModel);

        bool Processed { get; set; }
        void MarkAsProcessed(bool processed = true);
    }

    public interface IRequestedWebpackEntryModel : IRequestedEntryModel {}

    public interface IRequestedLiteralEntryModel : IRequestedEntryModel {
        string Content { get; set; }
        string LiteralHash { get; set; }
    }
}