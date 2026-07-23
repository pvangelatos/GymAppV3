using GymAppV3.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Infrastructure.Extensions;

/// <summary>
/// Extension methods for IQueryable to support pagination.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Materializes an IQueryable using ListOptions for paging and sorting.
    /// </summary>
    /// <typeparam name="T">The type to contain in the result items.</typeparam>
    /// <param name="source">The source queryable collection.</param>
    /// <param name="options">The options to use for sorting and paging.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A paginated result set with items and total count.</returns>
    public static async Task<ResultSet<T>> ToResultSetAsync<T>(
        this IQueryable<T> source,
        ListOptions? options,
        CancellationToken cancellationToken = default)
    {
        options ??= new ListOptions();

        // Apply sorting if specified
        if (!string.IsNullOrWhiteSpace(options.Sort))
        {
            source = source.ApplySorting(options.Sort);
        }

        return await source.ToResultSetAsync(options.Page, options.Size, cancellationToken);
    }

    /// <summary>
    /// Materializes an IQueryable using page number and size for paging.
    /// </summary>
    /// <typeparam name="T">The type to contain in the result items.</typeparam>
    /// <param name="source">The source queryable collection.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="size">The page size.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A paginated result set with items and total count.</returns>
    public static async Task<ResultSet<T>> ToResultSetAsync<T>(
        this IQueryable<T> source,
        int page,
        int size,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Must be a positive integer.");
        }

        if (size < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Must be a non-negative integer.");
        }

        var index = page - 1;

        // Size 0 means return count only, no items
        if (size == 0)
        {
            var count = await source.CountAsync(cancellationToken);
            return new ResultSet<T>(Array.Empty<T>(), count);
        }

        // Get items for current page
        var items = await source
            .Skip(index * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        // Optimize count query: if we got fewer items than requested and we have items,
        // we know we're on the last page
        var isLastPage = items.Count < size && items.Count > 0;
        var totalCount = isLastPage
            ? (index * size) + items.Count
            : await source.CountAsync(cancellationToken);

        return new ResultSet<T>(items, totalCount, size);
    }

    /// <summary>
    /// Applies sorting to an IQueryable based on a sort expression.
    /// Supports simple property names and "propertyname desc" for descending order.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="sortExpression">Sort expression (e.g., "Firstname", "Lastname desc").</param>
    /// <returns>Sorted queryable.</returns>
    private static IQueryable<T> ApplySorting<T>(this IQueryable<T> source, string sortExpression)
    {
        if (string.IsNullOrWhiteSpace(sortExpression))
            return source;

        var parts = sortExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var propertyName = parts[0];
        var isDescending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        // Use reflection to get property
        var type = typeof(T);
        var property = type.GetProperty(propertyName,
            System.Reflection.BindingFlags.IgnoreCase |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance);

        if (property == null)
            return source; // Property not found, return unsorted

        var parameter = System.Linq.Expressions.Expression.Parameter(type, "x");
        var propertyAccess = System.Linq.Expressions.Expression.Property(parameter, property);
        var lambda = System.Linq.Expressions.Expression.Lambda(propertyAccess, parameter);

        var methodName = isDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = System.Linq.Expressions.Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { type, property.PropertyType },
            source.Expression,
            System.Linq.Expressions.Expression.Quote(lambda));

        return source.Provider.CreateQuery<T>(resultExpression);
    }
}
