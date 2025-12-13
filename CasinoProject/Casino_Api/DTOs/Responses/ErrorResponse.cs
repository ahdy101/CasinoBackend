using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Casino_Api.DTOs.Responses;

public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public List<string> Errors { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ErrorResponse()
    {
    }

    public ErrorResponse(string message)
    {
        Message = message;
        Errors.Add(message);
    }

    public ErrorResponse(ModelStateDictionary modelState)
    {
        Message = "Validation failed";
        foreach (var error in modelState.Values.SelectMany(v => v.Errors))
        {
            Errors.Add(error.ErrorMessage);
        }
    }
}

public class SuccessResponse<T>
{
    public bool Success { get; set; } = true;
    public T Data { get; set; } = default!;
    public string Message { get; set; } = string.Empty;
}
