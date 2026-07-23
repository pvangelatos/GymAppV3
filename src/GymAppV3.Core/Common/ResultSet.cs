namespace GymAppV3.Core.Common;

/// <summary>
/// Generic paginated result set containing items and total count.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
public class ResultSet<T>
{
    /// <summary>
    /// Gets the items in the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Gets the total count of items across all pages.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Gets the total number of pages based on Count and page size.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Creates a new result set with items and total count.
    /// </summary>
    /// <param name="items">The items in the current page.</param>
    /// <param name="count">The total count of items.</param>
    /// <param name="pageSize">Optional page size for calculating total pages.</param>
    public ResultSet(IReadOnlyList<T> items, int count, int pageSize = 0)
    {
        Items = items ?? Array.Empty<T>();
        Count = count;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling((double)count / pageSize) : 0;
    }
}
