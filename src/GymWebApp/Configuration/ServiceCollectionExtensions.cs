namespace GymWebApp.Configuration;

/// <summary>
/// Helpers for wiring services that expose multiple interfaces from a single concrete.
/// Without these, calling AddScoped once per interface creates a separate instance per
/// interface — so a handler asking for both the command and query interfaces of one
/// service would get two distinct objects backed by the same DbContext.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a single scoped instance of <typeparamref name="TImpl"/> and exposes it
    /// under two interfaces. Both interface resolutions return the same instance per scope.
    /// </summary>
    public static IServiceCollection AddScopedShared<TImpl, TInterface1, TInterface2>(
        this IServiceCollection services)
        where TImpl : class, TInterface1, TInterface2
        where TInterface1 : class
        where TInterface2 : class
    {
        services.AddScoped<TImpl>();
        services.AddScoped<TInterface1>(sp => sp.GetRequiredService<TImpl>());
        services.AddScoped<TInterface2>(sp => sp.GetRequiredService<TImpl>());
        return services;
    }
}
