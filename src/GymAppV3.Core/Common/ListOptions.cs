namespace GymAppV3.Core.Common;

/// <summary>
/// Options for list queries including pagination and sorting.
/// </summary>
public class ListOptions
{
    private int _page = 1;
    private int _size = 50;

    /// <summary>
    /// Upper bound for page size, to prevent unbounded result sets.
    /// </summary>
    public const int MaxSize = 200;

    /// <summary>
    /// Gets or sets the page number (1-based). Defaults to 1.
    /// </summary>
    public int Page
    {
        get => _page;
        set => _page = value > 0 ? value : 1;
    }

    /// <summary>
    /// Gets or sets the page size. Defaults to 50, capped at MaxSize. Set to 0 to return count only without items.
    /// </summary>
    public int Size
    {
        get => _size;
        set => _size = value < 0 ? 50 : Math.Min(value, MaxSize);
    }

    /// <summary>
    /// Gets or sets the sort expression (e.g., "Firstname", "Lastname desc").
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>
    /// Creates default list options with page 1 and size 50.
    /// </summary>
    public ListOptions() { }

    /// <summary>
    /// Creates list options with specified page and size.
    /// </summary>
    public ListOptions(int page, int size)
    {
        Page = page;
        Size = size;
    }
}
