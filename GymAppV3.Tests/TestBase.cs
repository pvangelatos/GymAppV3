using GymAppV3.Core.Abstractions;
using GymAppV3.Infrastructure.Data;
using GymAppV3.Infrastructure.Data.Interceptors;
using GymAppV3.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Tests;

// Base class that spins up a fresh EF Core In-Memory database per test.
// This isolates each test to prevent data bleeding, allowing fast execution 
// without the translation restrictions of relational database engines.
public abstract class TestBase : IDisposable
{

    protected readonly ApplicationDbContext Context;

    protected TestBase()
    {
        var interceptor = new AuditableEntityInterceptor(
            new DateTimeProvider(),
            new UserContext(new HttpContextAccessor()));

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options);

        // Build the schema from the model. EnsureCreated (not Migrate) because we
        // want the current model shape without depending on migration files.
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        
        GC.SuppressFinalize(this);
    }
}
