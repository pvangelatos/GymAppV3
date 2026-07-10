using GymAppV3.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Tests;

// Base class that spins up a fresh SQLite in-memory database per test.
// SQLite (unlike the EF in-memory provider) enforces real relational rules —
// foreign keys, unique indexes, and NOT NULL — so tests exercise the actual schema.
public abstract class TestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly ApplicationDbContext Context;

    protected TestBase()
    {
        // The connection must stay open for the lifetime of the test: an in-memory
        // SQLite database exists only while at least one connection to it is open.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new ApplicationDbContext(options);

        // Build the schema from the model. EnsureCreated (not Migrate) because we
        // want the current model shape without depending on migration files.
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();   // closing the connection drops the in-memory database
        GC.SuppressFinalize(this);
    }
}
