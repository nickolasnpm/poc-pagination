namespace pagination.Application
{
    public class DefaultPaginationRequest
    {
        public int PaginationType { get; set; }
        public OffsetPaginationRequest? offsetPagination { get; set; }
        public CursorPaginationRequest? cursorPagination { get; set; }
    }

    public class OffsetPaginationRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class CursorPaginationRequest
    {
        public long Cursor { get; set; } = 0;
        public bool IsQueryPreviousPage { get; set; } = false;
        public bool IsIncludeTotalCount { get; set; } = false;
        public int PageSize { get; set; } = 50;
    }
}
