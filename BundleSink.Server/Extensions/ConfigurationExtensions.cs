using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BundleSink.Models;
using System;
using BundleSink.Configuration;

namespace BundleSink
{
    public static class ConfigurationExtensions {

        public static IWebHostBuilder ConfigureBundleSink(this IWebHostBuilder builder, Action<ISinkConfiguration> configurationBuilder)
        {
            var configuration = new SinkConfiguration();
            configurationBuilder(configuration);

            return builder
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddJsonFile(configuration.WebpackManifestName, true, true);
                })
                .ConfigureServices((builderContext, services) =>
                {
                    var settings = new BundleSinkSettings()
                    {
                        PublicOutputPath = configuration.WebpackPublicOutputPath,
                        AppendVersion = configuration.AppendVersion,
                        PrintAllAttributes = configuration.PrintAdditionalAttributesCondition(builderContext.HostingEnvironment),
                        PrintComments = configuration.PrintCommentsCondition(builderContext.HostingEnvironment),
                    };

                    services.AddSingleton(settings);
                    services.Configure<WebpackEntriesManifest>(builderContext.Configuration.GetSection(WebpackEntriesManifest.SECTION_NAME));
                    services.AddScoped<WebpackEntriesViewData>();
                });
        }

        public static IHostBuilder ConfigureBundleSink(this IHostBuilder builder, Action<ISinkConfiguration> configurationAction)
        {
            return builder
                .ConfigureWebHostDefaults(builder =>
                    builder.ConfigureBundleSink(configurationAction));
        }
    }
}