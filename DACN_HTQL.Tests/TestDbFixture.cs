using Microsoft.EntityFrameworkCore;
using DACN_HTQL.Data;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace DACN_HTQL.Tests;

public static class TestDbFixture
{
    public const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=DACN_HTQL;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

    private static readonly object _lock = new();
    private static bool _initialized = false;

    public static ClinicDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseSqlServer(ConnectionString)
            .EnableSensitiveDataLogging()
            .Options;

        return new ClinicDbContext(options);
    }

    public static async Task EnsureDatabaseInitializedAsync()
    {
        if (_initialized) return;

        lock (_lock)
        {
            if (_initialized) return;
        }

        using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
        await DbInitializer.InitializeAsync(db);

        lock (_lock)
        {
            _initialized = true;
        }
    }
}
