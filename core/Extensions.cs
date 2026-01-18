using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace core;

public static class Extensions {
    public static DateTime ToDateTime(this DateOnly dt) {
        return dt.ToDateTime(new TimeOnly(0, 0));
    }
    public static async Task WriteNullBodyResponse(this HttpResponse res) {
        res.StatusCode = StatusCodes.Status400BadRequest;
        await res.WriteAsync("Unable to parse request body");
    }
    public static async Task<T?> Deserialize<T>(this Stream stream) where T : class {
        try {
            using var reader = new StreamReader(stream);
            var s = await reader.ReadToEndAsync();
            var obj = JsonSerializer.Deserialize<T>(s);
            return obj;
        }
        catch (Exception ex) {
            return null;
        }
    }

    public static async Task<MemoryStream> ToMemoryStream(this string s) {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(s);
        await writer.FlushAsync();
        stream.Position = 0;
        return stream;
    }
}