namespace ChatMentor.Backend.Responses;

public class JSendResponse<T>
{
    private JSendResponse(string status, T data, string? message = null, PaginationMeta? meta = null)
    {
        Status = status;
        Data = data;
        Message = message;
        Meta = meta;
    }

    public string Status { get; set; }
    public T Data { get; set; }
    public string? Message { get; set; }
    public PaginationMeta? Meta { get; set; }

    public static JSendResponse<T> Success(T data, string? message = null, PaginationMeta? meta = null)
    {
        return new JSendResponse<T>("success", data, message, meta);
    }

    public static JSendResponse<T> Fail(T data)
    {
        return new JSendResponse<T>("fail", data);
    }

    public static JSendResponse<T> Error(string? message)
    {
        return new JSendResponse<T>("error", default, message);
    }
}

public class PaginationMeta(int currentPage, int pageSize, int totalRecords)
{
    public int CurrentPage { get; set; } = currentPage;
    public int PageSize { get; set; } = pageSize;
    public int TotalRecords { get; set; } = totalRecords;
    public int TotalPages { get; set; } = (int)Math.Ceiling(totalRecords / (double)pageSize);
}