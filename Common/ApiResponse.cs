namespace InstagramExtraApi.Common;

/// <summary>
/// Конверт ответа — 1:1 как в основном бэкенде (instagram-api.softclub.tj),
/// чтобы существующий фронт-клиент работал без изменений.
/// </summary>
public class ApiResponse<T>
{
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public int StatusCode { get; set; }

    public ApiResponse() { }

    public ApiResponse(T data, int statusCode = 200)
    {
        Data = data;
        StatusCode = statusCode;
    }

    public static ApiResponse<T> Ok(T data) => new(data, 200);

    public static ApiResponse<T> Fail(string error, int statusCode = 400) =>
        new() { Errors = { error }, StatusCode = statusCode };
}

/// <summary>Конверт со страничной разбивкой (как get-Locations в основном API).</summary>
public class PagedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPage { get; set; }
    public int TotalRecord { get; set; }
    public List<string> Errors { get; set; } = new();
    public int StatusCode { get; set; } = 200;

    public PagedResponse() { }

    public PagedResponse(List<T> data, int pageNumber, int pageSize, int totalRecord)
    {
        Data = data;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecord = totalRecord;
        TotalPage = pageSize > 0 ? (int)Math.Ceiling(totalRecord / (double)pageSize) : 0;
        StatusCode = 200;
    }
}
