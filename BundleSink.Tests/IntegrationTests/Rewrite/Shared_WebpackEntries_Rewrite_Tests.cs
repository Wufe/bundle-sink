using BundleSink.TestServer;

namespace BundleSink.Tests.IntegrationTests.Rewrite
{
    public class Shared_WebpackEntries_Rewrite_Tests :
        Base_WebpackEntries_Tests<RewriteWebApplicationFactory<Startup>> {

        public Shared_WebpackEntries_Rewrite_Tests(
            RewriteWebApplicationFactory<Startup> factory
        ) : base (factory) {}

    }
}