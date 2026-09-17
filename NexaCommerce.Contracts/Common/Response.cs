namespace NexaCommerce.Contracts.Common;

public class Response<T>
{
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool Success { get; set; }
    public string Code { get; set; } = ResponseCode.Success.ToString();
    public string Message { get; set; } = string.Empty;

    public static Response<T> Ok(T data, string message = "Success", ResponseCode code = ResponseCode.Success) =>
        new()
        {
            Data = data,
            Errors = new(),
            Success = true,
            Code = code.ToString(),
            Message = message
        };

    public static Response<T> Ok(T data, string message, string code) =>
        new()
        {
            Data = data,
            Errors = new(),
            Success = true,
            Code = code,
            Message = message
        };

    public static Response<T> Fail(string message, ResponseCode code = ResponseCode.Failed, List<string>? errors = null) =>
        new()
        {
            Data = default,
            Errors = errors ?? (string.IsNullOrEmpty(message) ? new() : new() { message }),
            Success = false,
            Code = code.ToString(),
            Message = message
        };

    public static Response<T> Fail(string message, string code, List<string>? errors = null) =>
        new()
        {
            Data = default,
            Errors = errors ?? (string.IsNullOrEmpty(message) ? new() : new() { message }),
            Success = false,
            Code = code,
            Message = message
        };
}

public class Response : Response<object>
{
    public static Response Ok(string message = "Success", ResponseCode code = ResponseCode.Success) =>
        new()
        {
            Data = null,
            Errors = new(),
            Success = true,
            Code = code.ToString(),
            Message = message
        };

    public static new Response Fail(string message, ResponseCode code = ResponseCode.Failed, List<string>? errors = null) =>
        new()
        {
            Data = null,
            Errors = errors ?? (string.IsNullOrEmpty(message) ? new() : new() { message }),
            Success = false,
            Code = code.ToString(),
            Message = message
        };
}
