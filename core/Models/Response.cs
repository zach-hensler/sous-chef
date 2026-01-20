using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace core.Models;

public class Response {
    private readonly object? _data;
    public readonly HttpStatusCode StatusCode = HttpStatusCode.OK;
    public readonly string ErrorMessage = "";

    public Response() {}
    
    public Response(object? data) {
        _data = data;
    }
    
    public Response(HttpStatusCode statusCode, string message) {
        StatusCode = statusCode;
        ErrorMessage = message;
    }
    public async Task WriteResponse(HttpResponse response) {
        response.StatusCode = (int)StatusCode;
        if (!string.IsNullOrWhiteSpace(ErrorMessage)) {
            await response.WriteAsync(ErrorMessage);
            return;
        }
        if (_data != null) {
            response.ContentType = "application/json";
            await response.WriteAsync(JsonSerializer.Serialize(_data));
        }
    }
}

// For type tracking
public class Response<T>: Response {
    public readonly T? Data;
    public readonly HttpStatusCode StatusCode = HttpStatusCode.OK;
    public readonly string ErrorMessage = "";

    public Response(HttpStatusCode statusCode, string message) : base(statusCode, message) {
        StatusCode = statusCode;
        ErrorMessage = message;
    }

    public Response(T data) : base(data) {
        Data = data;
    }
}