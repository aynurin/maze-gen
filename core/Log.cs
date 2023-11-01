#define DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class Log {
    private string _name;
    private BufferedLog _buffered;
    public BufferedLog Buffered => _buffered;

    public Log(string name) {
        _name = name;
        _buffered = new BufferedLog(this);
    }

    public void D(object message) {
#if DEBUG
        Write(LogMessage.D(message));
#endif
    }

    public void I(object message) => Write(LogMessage.I(message));

    public void W(object message) => Write(LogMessage.W(message));

    public void E(object message) => Write(LogMessage.E(message));

    private void Write(LogMessage message) {
        Console.WriteLine($"[{_name}] {message.Time:s} {message.Level}: {message.Message}");
    }

    public class BufferedLog {
        private readonly List<LogMessage> _buffer = new List<LogMessage>();
        private Log _parentLog;

        public BufferedLog(Log parentLog) {
            _parentLog = parentLog;
        }

        public void Reset() {
            _buffer.Clear();
        }

        public void D(object message) {
#if DEBUG
            _buffer.Add(LogMessage.D(message));
#endif
        }

        public void I(object message) => _buffer.Add(LogMessage.I(message));

        public void W(object message) => _buffer.Add(LogMessage.W(message));

        public void E(object message) => _buffer.Add(LogMessage.E(message));

        public void Flush() {
            foreach (var message in _buffer) {
                switch (message.Level) {
                    case LogLevel.Debug: _parentLog.D(message); break;
                    case LogLevel.Info: _parentLog.I(message); break;
                    case LogLevel.Warning: _parentLog.W(message); break;
                    case LogLevel.Error: _parentLog.E(message); break;
                }
            }
            Reset();
        }
    }

    private class LogMessage {
        public LogLevel Level { get; set; }
        public DateTime Time { get; set; }
        public Object Message { get; set; }

        public LogMessage(LogLevel level, Object message) {
            Level = level;
            Time = DateTime.UtcNow;
            Message = message;
        }

        public static LogMessage D(Object message) =>
            new LogMessage(LogLevel.Debug, message);
        public static LogMessage I(Object message) =>
            new LogMessage(LogLevel.Info, message);
        public static LogMessage W(Object message) =>
            new LogMessage(LogLevel.Warning, message);
        public static LogMessage E(Object message) =>
            new LogMessage(LogLevel.Error, message);
    }

    public enum LogLevel {
        Debug,
        Info,
        Warning,
        Error
    }

    public static Log CreateDefault() =>
        new Log(new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name);

    public static Log Create(string name, StreamWriter appender) =>
        new Log(new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name);
}