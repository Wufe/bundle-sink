using BundleSink.ActionFilters;
using BundleSink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BundleSink.Configuration
{
    public class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
    {
        private readonly BundleSinkSettings _settings;

        public ConfigureMvcOptions(
            BundleSinkSettings settings
        )
        {
            _settings = settings;
        }
        
        public void Configure(MvcOptions options)
        {
            if (_settings.RewriteOutput) {
                options.Filters.Add(typeof(BodyRewriteResultFilter));
            } else {
                if (_settings.CheckIntegrity)
                    options.Filters.Add(typeof(IntegrityCheckerResultFilterAttribute));
            }
        }
    }
}