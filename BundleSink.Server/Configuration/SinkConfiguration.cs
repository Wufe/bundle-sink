using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace BundleSink.Configuration
{
    public interface ISinkConfiguration : IWebpackSinkConfiguration {

    }
    public interface IWebpackSinkConfiguration {
        bool AppendVersion { get; set; }
        Func<IWebHostEnvironment, bool> PrintAdditionalAttributesCondition { get; set; }
        Func<IWebHostEnvironment, bool> PrintCommentsCondition { get; set; }
        ISinkConfiguration WithWebpack(string manifestName, string publicOutputPath);
    }

    internal class SinkConfiguration : ISinkConfiguration
    {
        public bool UseWebpack { get; set; } = false;
        public string WebpackManifestName { get; set; } = "";
        public string WebpackPublicOutputPath { get; set; } = "";

        public bool AppendVersion { get; set; } = true;

        public Func<IWebHostEnvironment, bool> PrintAdditionalAttributesCondition { get; set; } =
            environment => environment.IsDevelopment();

        public Func<IWebHostEnvironment, bool> PrintCommentsCondition { get; set; } =
            environment => environment.IsDevelopment();

        public ISinkConfiguration WithWebpack(string manifestName, string publicOutputPath)
        {
            UseWebpack = true;
            WebpackManifestName = manifestName;
            WebpackPublicOutputPath = publicOutputPath;

            return this;
        }
    }
}