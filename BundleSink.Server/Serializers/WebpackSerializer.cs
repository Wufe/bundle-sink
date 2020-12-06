using System.IO;
using System.Linq;
using BundleSink.Models;
using BundleSink.Models.Entry;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace BundleSink.Server.Serializers
{
    public partial class Serializer {

        private string Serialize(IRequestedWebpackEntryModel webpackEntry, string dependencyOf = null) {
            var finalOutput = "";
            if (_webpackManifest.Value.TryGetValue(webpackEntry.Name, out var entryFiles))
            {
                finalOutput += SerializeWebpackEntryJS(webpackEntry, entryFiles, dependencyOf);
                finalOutput += SerializeWebpackEntryCSS(webpackEntry, entryFiles, dependencyOf);
            }
            return finalOutput;
        }

        private string SerializeWebpackEntryJS(IRequestedEntryModel requestedEntry, WebpackEntryModel webpackEntry, string dependencyOf = null)
        {
            var finalOutput = "";
            if (!requestedEntry.CSSOnly)
            {
                for (var i = 0; i < webpackEntry.Assets.JS.Count; i++)
                {
                    var file = webpackEntry.Assets.JS[i];

                    var fileIsADependency = i < webpackEntry.Assets.JS.Count - 1;

                    var serializedFileSearchKey = file;

                    if (!fileIsADependency && requestedEntry.Key != null)
                        serializedFileSearchKey += requestedEntry.Key;

                    if (_viewData.TryMarkFileAsSerialized(serializedFileSearchKey))
                    {
                        var async = !fileIsADependency && requestedEntry.Async ? "async" : "";
                        var defer = !fileIsADependency && requestedEntry.Defer ? "defer" : "";

                        var filePath = Path.Combine("/", _settings.PublicOutputPath, file);

                        if (_settings.AppendVersion)
                            filePath = _fileVersionProvider.AddFileVersionToPath(_pathBase, filePath);

                        var comments = "";
                        if (_settings.PrintComments)
                        {
                            // Requested due to being a dependency
                            if (requestedEntry.Sink != _sinkName && !string.IsNullOrEmpty(dependencyOf))
                            {
                                comments += $"<!-- The following entry should be printed into \"{requestedEntry.Sink}\" sink. However it has been requested as a dependency of \"{dependencyOf}\". -->\n";
                            }
                        }

                        finalOutput += string.Format(
                            "{0}<script type=\"text/javascript\" src=\"{1}\"{2}{3}{4}></script>\n",
                            comments,
                            filePath,
                            !string.IsNullOrEmpty(async) ? " " + async : "",
                            !string.IsNullOrEmpty(defer) ? " " + defer : "",
                            _settings.PrintAllAttributes ? SerializeEntryAttributes(requestedEntry, dependencyOf) : ""
                        );
                    }
                }
            }
            return finalOutput;
        }

        private string SerializeWebpackEntryCSS(IRequestedEntryModel requestedEntry, WebpackEntryModel webpackEntry, string dependencyOf = null)
        {
            var finalOutput = "";
            if (!requestedEntry.JSOnly)
            {
                for (var i = 0; i < webpackEntry.Assets.CSS.Count; i++)
                {
                    var file = webpackEntry.Assets.CSS[i];

                    var serializedFileSearchKey = file;

                    if (requestedEntry.Key != null)
                        serializedFileSearchKey += requestedEntry.Key;

                    if (_viewData.TryMarkFileAsSerialized(serializedFileSearchKey))
                    {
                        var filePath = Path.Combine("/", _settings.PublicOutputPath, file);

                        if (_settings.AppendVersion)
                            filePath = _fileVersionProvider.AddFileVersionToPath(_pathBase, filePath);

                        var comments = "";
                        if (_settings.PrintComments)
                        {
                            // Requested due to being a dependency
                            if (requestedEntry.Sink != _sinkName && !string.IsNullOrEmpty(dependencyOf))
                            {
                                comments += $"<!-- The following entry should be printed into \"{requestedEntry.Sink}\" sink. However it has been requested as a dependency of \"{dependencyOf}\". -->\n";
                            }
                        }

                        finalOutput += string.Format(
                            "{0}<link rel=\"stylesheet\" href=\"{1}\"{2} />\n",
                            comments,
                            filePath,
                            _settings.PrintAllAttributes ? SerializeEntryAttributes(requestedEntry, dependencyOf) : ""
                        );
                    }
                }
            }
            return finalOutput;
        }
    }
}