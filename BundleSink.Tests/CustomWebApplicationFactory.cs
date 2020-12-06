using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace BundleSink.Tests
{
    #region snippet1
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder webBuilder)
        {
            webBuilder
                .ConfigureBundleSink(builder => {
                    builder.PrintAdditionalAttributesCondition = environment => true;
                    builder.PrintCommentsCondition = environment => true;
                    builder.WithWebpack("wwwroot/dist/client-manifest.json", "/dist/");
                });
        }
    }
    #endregion
}