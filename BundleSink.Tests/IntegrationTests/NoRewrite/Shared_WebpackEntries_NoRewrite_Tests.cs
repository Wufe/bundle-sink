using BundleSink.TestServer;

namespace BundleSink.Tests.IntegrationTests.NoRewrite
{
    public class Shared_WebpackEntries_NoRewrite_Tests :
        Base_WebpackEntries_Tests<NoRewriteWebApplicationFactory<Startup>> {

        public Shared_WebpackEntries_NoRewrite_Tests(
            NoRewriteWebApplicationFactory<Startup> factory
        ) : base (factory) {}

    }
}