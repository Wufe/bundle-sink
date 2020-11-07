namespace BundleSink.Configuration
{
    public interface ISinkConfiguration : IWebpackSinkConfiguration {

    }
    public interface IWebpackSinkConfiguration {
        ISinkConfiguration WithWebpack(string manifestName, string publicOutputPath);
    }

    internal class SinkConfiguration : ISinkConfiguration
    {
        public bool UseWebpack { get; set; } = false;
        public string WebpackManifestName { get; set; } = "";
        public string WebpackPublicOutputPath { get; set; } = "";

        public ISinkConfiguration WithWebpack(string manifestName, string publicOutputPath)
        {
            UseWebpack = true;
            WebpackManifestName = manifestName;
            WebpackPublicOutputPath = publicOutputPath;

            return this;
        }
    }
}