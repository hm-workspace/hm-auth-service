namespace AuthService.Utils.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T? data = default, string message = "Success") => new() { Success = true, Message = message, Data = data };
    public static ApiResponse<T> Fail(string message, params string[] errors) => new() { Success = false, Message = message, Errors = errors };
}

public record PagedResult<T>(IReadOnlyCollection<T> Items, int TotalCount, int PageNumber, int PageSize);

public class SearchQuery
{
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
