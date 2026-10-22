namespace CountrySecure.Application.DTOs.Common
{
    /// <summary>
    /// Parámetros de paginación comunes a todos los listados (?page=1&amp;pageSize=20).
    /// </summary>
    public class PageQuery
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = DefaultPageSize;

        /// <summary>Página acotada a un valor válido (&gt;= 1).</summary>
        public int NormalizedPage => Page < 1 ? 1 : Page;

        /// <summary>Tamaño acotado entre 1 y <see cref="MaxPageSize"/>.</summary>
        public int NormalizedPageSize => PageSize < 1 ? DefaultPageSize : Math.Min(PageSize, MaxPageSize);

        public int Skip => (NormalizedPage - 1) * NormalizedPageSize;
    }

    /// <summary>
    /// Formato único de respuesta para todos los listados paginados.
    /// </summary>
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;

        public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        /// <summary>Convierte los elementos conservando los metadatos de paginación (entidad → DTO).</summary>
        public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector)
            => new(Items.Select(selector).ToList(), Page, PageSize, TotalCount);
    }
}
