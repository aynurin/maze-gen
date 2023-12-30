#define DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class Log {
    private string _name;
    private BufferedLog _buffered;
    private ILogWriter _writer;
    public BufferedLog Buffered => _buffered;

    public static int DebugLevel { get; set; } = 0;

    public Log(string name) : this(name, new DefaultLogWriter()) { }

    public Log(string name, ILogWriter writer) {
        _name = name;
        _buffered = new BufferedLog(this);
        _writer = writer;
    }

    public void D(int debugLevel, String message) {
#if DEBUG
        if (debugLevel <= DebugLevel) {
            Write(LogMessage.D(debugLevel, message));
        }
#endif
    }

    public void I(String message) => Write(LogMessage.I(message));

    public void W(String message) => Write(LogMessage.W(message));

    public void E(String message) => Write(LogMessage.E(message));

    private void Write(LogMessage message) {
        _writer.Write(_name, message);
    }

    public static void WriteImmediate(object message) {
        new ImmediateFileLogWriter()
            .Write("immediate", LogMessage.I(message.ToString()));
    }

    public interface ILogWriter {
        void Write(string logName, LogMessage message);
    }

    private class DefaultLogWriter : ILogWriter {
        public void Write(string logName, LogMessage message) {
            Console.WriteLine($"{message.Message}");
        }
    }

    public class ImmediateFileLogWriter : ILogWriter {
        private static readonly object s_lock = new object();
        public void Write(string logName, LogMessage message) {
            lock (s_lock) {
                using (var writer = new StreamWriter(Path.Combine(Environment.CurrentDirectory, $"{logName}.log"), true)) {
                    writer.WriteLine($"{message.Message}");
                }
            }
        }
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

        public void D(int debugLevel, String message) {
#if DEBUG
            if (debugLevel <= Log.DebugLevel) {
                _buffer.Add(LogMessage.D(debugLevel, message));
            }
#endif
        }

        public void I(String message) => _buffer.Add(LogMessage.I(message));

        public void W(String message) => _buffer.Add(LogMessage.W(message));

        public void E(String message) => _buffer.Add(LogMessage.E(message));

        public void Flush() {
            foreach (var message in _buffer) {
                _parentLog.Write(message);
            }
            Reset();
        }
    }

    public class LogMessage {
        public LogLevel Level { get; set; }
        public DateTime Time { get; set; }
        public String Message { get; set; }
        public int DebugLevel { get; set; }

        public LogMessage(LogLevel level, String message) :
            this(level, 0, message) { }

        public LogMessage(LogLevel level, int debugLevel, String message) {
            Level = level;
            Time = DateTime.UtcNow;
            Message = message;
            DebugLevel = debugLevel;
        }

        public static LogMessage D(int debugLevel, String message) =>
            new LogMessage(LogLevel.Debug, debugLevel, message);
        public static LogMessage I(String message) =>
            new LogMessage(LogLevel.Info, message);
        public static LogMessage W(String message) =>
            new LogMessage(LogLevel.Warning, message);
        public static LogMessage E(String message) =>
            new LogMessage(LogLevel.Error, message);

        public override string ToString() {
            return Message.GetType().FullName;
        }
    }

    public enum LogLevel {
        Debug,
        Info,
        Warning,
        Error
    }

    public static Log CreateDefault() =>
        new Log(new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name);

    public static Log Create(string name) =>
        new Log(name);

    public static Log CreateForThisTest() =>
        new Log(new StackTrace().GetFrame(1).GetMethod().Name.Split('_').Last());

    public static Log Create(string name, StreamWriter appender) =>
        new Log(new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name);
}