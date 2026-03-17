using System.Text;
using Microsoft.Extensions.Logging;

namespace core;

// TODO this isn't disposed when requests finish
public class Logging: IDisposable, IAsyncDisposable {
    private readonly ILogger _consoleLogger;
    private readonly FileLogger? _fileLogger;
    public Logging() {
        _consoleLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("");
        var filePath = Environment.GetEnvironmentVariable(EnvVars.LogDirectory);
        if (!string.IsNullOrWhiteSpace(filePath)) {
            _fileLogger = new FileLogger(filePath);
        }
    }

    public void LogInfo(string s) {
        _consoleLogger.LogInformation(s);
    }

    public void LogError(string s) {
        _consoleLogger.LogError(s);
    }

    // TODO update this to write to a log file
    // Also todo, do I need to do this differently so we aren't constantly opening/closing the file?
    private class FileLogger(string path) : ILogger, IDisposable, IAsyncDisposable {
        private readonly FileStream _fp = File.Open(path, FileMode.Append, FileAccess.Read);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            var msg = formatter(state, exception) + Environment.NewLine;
            switch (logLevel) {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                    _fp.Write(Encoding.UTF8.GetBytes("Info: " + msg));
                    break;
                case LogLevel.Warning:
                case LogLevel.Error:
                case LogLevel.Critical:
                    _fp.Write(Encoding.UTF8.GetBytes("Error: " + msg));
                    break;
                case LogLevel.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }

        public bool IsEnabled(LogLevel logLevel) {
            return true;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
            throw new NotImplementedException();
        }

        public void Dispose() {
            _fp.Dispose();
        }

        public async ValueTask DisposeAsync() {
            await _fp.DisposeAsync();
        }
    }

    public void Dispose() {
        _fileLogger?.Dispose();
    }

    public async ValueTask DisposeAsync() {
        if (_fileLogger != null) await _fileLogger.DisposeAsync();
    }
}
