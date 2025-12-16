public class PaginatedResult<T>
{
    public int TotalCount { get; set; }
    public IEnumerable<T> Items { get; set; }

    public PaginatedResult(IEnumerable<T> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }
}