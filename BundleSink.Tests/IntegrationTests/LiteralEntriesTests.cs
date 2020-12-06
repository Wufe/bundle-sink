using System;
using System.Net.Http;
using System.Threading.Tasks;
using BundleSink.Tests.Helpers;
using BundleSink.TestServer;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BundleSink.Tests.IntegrationTests
{
    public class LiteralEntriesTests : IClassFixture<CustomWebApplicationFactory<Startup>> {
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;

        public LiteralEntriesTests(
            CustomWebApplicationFactory<BundleSink.TestServer.Startup> factory
        )
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task SimpleLiteralEntry_ShouldBePrintedOut() {
            var page1 = await _client.GetAsync("/Test/SimpleLiteralEntryTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);

            // Act

            // Assert
            Assert.Equal(1, scripts.Count);
            Assert.Single(scripts, script => script.TextContent.Contains("console.log('simple entry')"));
        }

        [Fact]
        public async Task LiteralEntryWithDependency_ShouldPrintTheLiteralEntryAndItsDependency()
        {
            var page1 = await _client.GetAsync("/Test/LiteralEntryWithDependencyTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);

            // Act

            // Assert
            Assert.Equal(2, scripts.Count);
            Assert.Single(scripts,
                script =>
                    !string.IsNullOrEmpty(script.TextContent) &&
                    script.TextContent.Contains("console.log('simple entry')"));
            Assert.Single(scripts,
                script =>
                    !string.IsNullOrEmpty(script.Source) &&
                    script.Source.Contains("page-a"));
        }

        [Fact]
        public async Task DuplicateLiteralEntries_ShouldPrintOnlyOnce()
        {
            var page1 = await _client.GetAsync("/Test/DuplicateLiteralEntriesTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);

            // Act

            // Assert
            Assert.Equal(1, scripts.Count);
            Assert.Single(scripts,
                script =>
                    !string.IsNullOrEmpty(script.TextContent) &&
                    script.TextContent.Contains("console.log('simple entry')"));
        }

        [Fact]
        public async Task DependencyDeclaringLiterals_ShouldPrintLiteralsAndDependency()
        {
            var page1 = await _client.GetAsync("/Test/DependencyDeclaringLiteralsTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);

            // Act

            // Assert
            Assert.Equal(3, scripts.Count);
            Assert.Single(scripts,
                script =>
                    !string.IsNullOrEmpty(script.Source) &&
                    script.Source.Contains("page-a"));
            Assert.Single(scripts,
                script =>
                    !string.IsNullOrEmpty(script.TextContent) &&
                    script.TextContent.Contains("literal 1')"));
            Assert.Single(scripts,
                script =>
                    !string.IsNullOrEmpty(script.TextContent) &&
                    script.TextContent.Contains("literal 2')"));
        }
    }
}