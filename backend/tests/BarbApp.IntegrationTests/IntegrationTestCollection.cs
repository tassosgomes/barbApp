using Xunit;

namespace BarbApp.IntegrationTests;

/// <summary>
/// xUnit Collection definition for integration tests.
/// 
/// This ensures a SINGLE DatabaseFixture instance (and therefore a single PostgreSQL container)
/// is shared across ALL test classes that use [Collection(nameof(IntegrationTestCollection))].
/// 
/// Best Practice from Testcontainers .NET documentation:
/// - Use ICollectionFixture for container sharing across test classes
/// - This dramatically reduces test execution time by avoiding container startup for each test class
/// - The container is started once before all tests and disposed after all tests complete
/// 
/// Usage in test classes:
/// 1. Add [Collection(nameof(IntegrationTestCollection))] attribute to the test class
/// 2. Inject DatabaseFixture via constructor
/// 3. Use _dbFixture.CreateFactory() to get the IntegrationTestWebAppFactory
/// </summary>
[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
