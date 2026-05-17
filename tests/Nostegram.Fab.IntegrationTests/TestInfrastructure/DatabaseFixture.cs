using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nostegram.Fab.Infrastructure.Persistence;
using Respawn;
using Testcontainers.MsSql;
using Xunit;

namespace Nostegram.Fab.IntegrationTests.TestInfrastructure;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder(
            "mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private Respawner _respawner = null!;
    private SqlConnection _connection = null!;

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _connection = new SqlConnection(ConnectionString);
        await _connection.OpenAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();

        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                TablesToIgnore = ["__EFMigrationsHistory"]
            });
    }

    public FabDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FabDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new FabDbContext(options);
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);
    }

    public async Task DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await _container.DisposeAsync();
    }
}