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
    }

    public interface IRequestedWebpackEntryModel : IRequestedEntryModel {}
}