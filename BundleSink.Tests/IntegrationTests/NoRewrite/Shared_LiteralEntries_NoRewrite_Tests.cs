using BundleSink.TestServer;

namespace BundleSink.Tests.IntegrationTests.NoRewrite
{
    public class Shared_LiteralEntries_NoRewrite_Tests :
        Base_LiteralEntries_Tests<NoRewriteWebApplicationFactory<Startup>> {

        public Shared_LiteralEntries_NoRewrite_Tests(
            NoRewriteWebApplicationFactory<Startup> factory
        ) : base (factory) {}

    }
}