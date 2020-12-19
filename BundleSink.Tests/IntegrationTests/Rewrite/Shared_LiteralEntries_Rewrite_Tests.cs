using BundleSink.TestServer;

namespace BundleSink.Tests.IntegrationTests.Rewrite
{
    public class Shared_LiteralEntries_Rewrite_Tests :
        Base_LiteralEntries_Tests<RewriteWebApplicationFactory<Startup>> {

        public Shared_LiteralEntries_Rewrite_Tests(
            RewriteWebApplicationFactory<Startup> factory
        ) : base (factory) {}

    }
}