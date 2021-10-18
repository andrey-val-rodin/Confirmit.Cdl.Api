using Xunit;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    [CollectionDefinition(nameof(SharedFixture))]
    public class FixtureCollection : ICollectionFixture<SharedFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}