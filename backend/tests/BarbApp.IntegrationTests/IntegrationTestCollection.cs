using Xunit;

namespace BarbApp.IntegrationTests;

/// <summary>
/// xUnit Collection definition for integration tests.
/// This ensures a single DatabaseFixture instance is shared across all tests.
/// </summary>
[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
