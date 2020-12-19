using System;
using System.Net.Http;
using System.Threading.Tasks;
using BundleSink.Tests.Helpers;
using BundleSink.TestServer;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BundleSink.Tests.IntegrationTests.Rewrite
{
    public class WebpackEntries_Rewrite_Tests : IClassFixture<RewriteWebApplicationFactory<Startup>> {
        private readonly RewriteWebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;

        public WebpackEntries_Rewrite_Tests(
            RewriteWebApplicationFactory<Startup> factory
        ) {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task SinkBeforeWebpackEntry_ShouldPrintTheEntry() {
            // Arrange
            var page = await _client.GetAsync("/Test/SinkBeforeWebpackEntryTest");
            var content = await HtmlHelpers.GetDocumentAsync(page);
            var scripts = HtmlHelpers.GetScripts(content);

            // Assert
            Assert.Single(scripts, script => script.Source.Contains("page-a"));
        }

    }
}