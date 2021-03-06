using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using BundleSink.Tests.Helpers;
using BundleSink.TestServer;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BundleSink.Tests.IntegrationTests
{
    public abstract class Base_WebpackEntries_Tests<TWebApplicationFactory> :
        IClassFixture<TWebApplicationFactory>
        where TWebApplicationFactory: WebApplicationFactory<Startup> {
        private readonly TWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public Base_WebpackEntries_Tests(
            TWebApplicationFactory factory
        )
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task MultipleEntriesOnSameSink_ShouldNotBeDuplicated() {
            // Arrange
            var page1 = await _client.GetAsync("/Test/DuplicateEntriesOnSameSinkTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);

            // Act

            // Assert
            Assert.Single(scripts, script => script.Source.Contains("page-a"));
        }

        [Fact]
        public async Task MultipleEntriesWithDifferentKey_ShouldBeDuplicated() {
            // Arrange
            var page1 = await _client.GetAsync("/Test/SameEntryDifferentKeyTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);

            // Act

            // Assert
            Assert.Equal(2, scripts.Count);
            Assert.Single(scripts, script => script.Source.Contains("page-a") && script.Attributes.Any(attr => attr.Name == "key" && attr.Value == "1"));
            Assert.Single(scripts, script => script.Source.Contains("page-a") && script.Attributes.Any(attr => attr.Name == "key" && attr.Value == "2"));
        }

        [Fact]
        public async Task DifferentEntriesOnDifferentSinks_ShouldBeRenderedInsideTwoDifferentSinks()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/DifferentEntriesOnDifferentSinksTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);

            // Act

            // Assert
            Assert.Equal(2, scripts.Count);
            Assert.Single(scripts, script => script.Source.Contains("page-a") && script.Attributes.Any(attr => attr.Name == "sink" && attr.Value == "above"));
            Assert.Single(scripts, script => script.Source.Contains("page-b") && script.Attributes.Any(attr => attr.Name == "sink" && attr.Value == "below"));
        }

        [Fact]
        public async Task SameEntryOnDifferentSinks_ShouldBeRenderedOnceInsideFirstSink()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/SameEntryDifferentSinksTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);

            // Act

            // Assert
            Assert.Equal(1, scripts.Count);
            Assert.Single(scripts, script => script.Source.Contains("page-a") && script.Attributes.Any(attr => attr.Name == "sink" && attr.Value == "above"));
        }

        [Fact]
        public async Task OneEntryWithOneDependency_ShouldPrintTheEntryAndItsDependency()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/OneEntryWithOneDependencyTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);
            var links = HtmlHelpers.GetLinks(content);

            // Act

            // Assert
            Assert.Equal(1, scripts.Count);
            Assert.Equal(1, links.Count);
            Assert.Single(scripts, script => script.Source.Contains("page-a") && script.Attributes.Any(attr => attr.Name == "requires" && attr.Value == "page-a-style"));
            Assert.Single(links, link => link.Href.Contains("page-a-style") && link.Attributes.Any(attr => attr.Name == "dependency-of" && attr.Value == "page-a"));
        }

        [Fact]
        public async Task OneEntryWithOneAlreadyDeclaredDependency_ShouldPrintTheEntryAndItsDependencyWithoutDuplication()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/OneEntryWithOneAlreadyDeclaredDependencyTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);
            var links = HtmlHelpers.GetLinks(content);

            // Act

            // Assert
            Assert.Equal(1, scripts.Count);
            Assert.Equal(1, links.Count);
            Assert.Single(scripts, script => script.Source.Contains("page-a") && script.Attributes.Any(attr => attr.Name == "requires" && attr.Value == "page-a-style"));
            Assert.Single(links, link => link.Href.Contains("page-a-style"));
        }

        [Fact]
        public async Task OneEntryWithOneAlreadyDeclaredDependencyInPreviousSink_ShouldPrintTheEntryAndItsDependencyWithoutDuplication()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/OneEntryWithOneAlreadyDeclaredDependencyInPreviousSinkTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);
            var links = HtmlHelpers.GetLinks(content);

            // Act

            // Assert
            Assert.Equal(1, links.Count);
            Assert.Equal(1, scripts.Count);

            Assert.Single(links,
                link =>
                    link.Href.Contains("page-a-style") &&
                    link.Attributes.Any(attr => attr.Name == "sink" && attr.Value == "above"));

            Assert.Single(scripts,
                script =>
                    script.Source.Contains("page-a") &&
                    script.Attributes.Any(attr => attr.Name == "requires" && attr.Value == "page-a-style") &&
                    script.Attributes.Any(attr => attr.Name == "sink" && attr.Value == "below"));
            
        }

        [Fact]
        public async Task OneEntryWithOneAlreadyDeclaredDependencyInNextSink_ShouldPrintTheEntryAndItsDependencyWithoutDuplication()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/OneEntryWithOneAlreadyDeclaredDependencyInNextSinkTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);
            var links = HtmlHelpers.GetLinks(content);

            // Act

            // Assert
            Assert.Equal(1, links.Count);
            Assert.Equal(1, scripts.Count);

            Assert.Single(links,
                link =>
                    link.Href.Contains("page-a-style") &&
                    link.Attributes.Any(attr => attr.Name == "sink" && attr.Value == "below") &&
                    link.Attributes.Any(attr => attr.Name == "dependency-of" && attr.Value == "page-a"));

            Assert.Single(scripts,
                script =>
                    script.Source.Contains("page-a") &&
                    script.Attributes.Any(attr => attr.Name == "requires" && attr.Value == "page-a-style") &&
                    script.Attributes.Any(attr => attr.Name == "sink" && attr.Value == "above"));

            Assert.Single(content.Descendents<IComment>(), comment => comment.TextContent.Contains("The following entry should be printed into \"below\" sink. However it has been requested as a dependency of \"page-a\"."));

        }

        [Fact]
        public async Task OneDependencyWithNoDependantsTest_ShouldPreventPrintingTheDependency()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/OneDependencyWithNoDependantsTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);
            var links = HtmlHelpers.GetLinks(content);

            // Act

            // Assert
            Assert.Equal(0, links.Count);
            Assert.Equal(0, scripts.Count);

            Assert.Single(content.Descendents<IComment>(), comment => comment.TextContent.Contains("Preventing output of \"page-a-style\" because there are no dependants."));

        }

        [Fact]
        public async Task SameEntryWithinAPartial_ShouldPrintWithoutDuplications()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/SameEntryWithinAPartialTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);
            var links = HtmlHelpers.GetLinks(content);

            // Act

            // Assert
            Assert.Equal(1, scripts.Count);

            Assert.Single(scripts,
                script =>
                    script.Source.Contains("page-a"));

        }

        [Fact]
        public async Task SameEntryWithinNestedPartials_ShouldPrintWithoutDuplications()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/SameEntryWithinNestedPartialsTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);
            var links = HtmlHelpers.GetLinks(content);

            // Act

            // Assert
            Assert.Equal(1, scripts.Count);

            Assert.Single(scripts,
                script =>
                    script.Source.Contains("page-a"));

        }

        [Fact]
        public async Task SinkBeforeNestedContentWithEntry_ShouldPrintTheEntryEvenThoughTheSinkHasBeenDeclaredBeforeTheRenderBodyFunction()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/SinkBeforeNestedContentWithEntryTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);
            var links = HtmlHelpers.GetLinks(content);

            // Act

            // Assert
            Assert.Equal(1, scripts.Count);

            Assert.Single(scripts,
                script =>
                    script.Source.Contains("page-a"));

        }

        [Fact]
        public async Task SinkBeforeNestedContentWithNestedEntries_ShouldPrintTheNestedEntriesEvenThoughTheSinkHasBeenDeclaredBeforeTheRenderBodyFunction()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/SinkBeforeNestedContentWithNestedEntriesTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);
            var links = HtmlHelpers.GetLinks(content);

            // Act

            // Assert
            Assert.Equal(2, scripts.Count);

            Assert.Single(scripts,
                script =>
                    script.Source.Contains("page-a"));

            Assert.Single(scripts,
                script =>
                    script.Source.Contains("page-b"));

        }

        [Fact]
        public async Task WebpackEntryWithCSSOnlyAttribute_ShouldPrintCSSOnly()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/WebpackEntryWithCSSOnlyAttributeTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);
            var links = HtmlHelpers.GetLinks(content);

            // Act

            // Assert
            Assert.Equal(0, scripts.Count);
            Assert.Equal(1, links.Count);

            Assert.Single(links,
                link =>
                    link.Href.Contains("page-c"));

        }

        [Fact]
        public async Task WebpackEntryWithJSOnlyAttribute_ShouldPrintJSOnly()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/WebpackEntryWithJSOnlyAttributeTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);
            var links = HtmlHelpers.GetLinks(content);

            // Act

            // Assert
            Assert.Equal(1, scripts.Count);
            Assert.Equal(0, links.Count);

            Assert.Single(scripts,
                script =>
                    script.Source.Contains("page-c"));

        }

        [Fact]
        public async Task WebpackEntryWithTwoChunksAndStyle_ShouldPrintTwoScriptsAndOneLink()
        {
            // Arrange
            var page1 = await _client.GetAsync("/Test/WebpackEntryWithTwoChunksAndStyleTest");
            var content = await HtmlHelpers.GetDocumentAsync(page1);
            var scripts = HtmlHelpers.GetScripts(content);
            var links = HtmlHelpers.GetLinks(content);

            // Act

            // Assert
            Assert.Equal(2, scripts.Count);
            Assert.Equal(1, links.Count);

            Assert.Single(scripts,
                script =>
                    script.Source.Contains("page-e"));
            Assert.Single(scripts,
                script =>
                    script.Source.Contains("node_modules"));
            Assert.Single(links,
                link =>
                    link.Href.Contains("page-e"));

        }
    }
}