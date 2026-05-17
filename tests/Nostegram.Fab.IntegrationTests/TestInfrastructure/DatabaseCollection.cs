using Xunit;

namespace Nostegram.Fab.IntegrationTests.TestInfrastructure
{
    [CollectionDefinition("Database", DisableParallelization = true)]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}