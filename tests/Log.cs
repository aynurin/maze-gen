#define DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// TODO: Refactor this to use Trace
public class Log {
    private readonly string _name;
    private readonly BufferedLog _buffered;
    private readonly ILogWriter _writer;
    public BufferedLog Buffered => _buffered;

    public static int DebugLevel { get; set; } = 0;

    public Log(string name) : this(name, new DefaultLogWriter()) { }

    public Log(string name, ILogWriter writer) {
        _name = name;
        _buffered = new BufferedLog(this);
        _writer = writer;
    }

    public void D(int debugLevel, string message) {
#if DEBUG
        if (debugLevel <= DebugLevel) {
            Write(LogMessage.D(debugLevel, message));
        }
#endif
    }

    public void I(string message) => Write(LogMessage.I(message));

    public void W(string message) => Write(LogMessage.W(message));

    public void E(string message) => Write(LogMessage.E(message));

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

    // TODO: TextWriter is not used properly here, need to refactor when working
    //       on Trace migration.
    public class BufferedLog : TextWriter {
        private readonly List<LogMessage> _buffer = new List<LogMessage>();
        private readonly Log _parentLog;

        public override Encoding Encoding => throw new NotImplementedException();

        public BufferedLog(Log parentLog) {
            _parentLog = parentLog;
        }

        public void Reset() {
            _buffer.Clear();
        }

        override public void Write(string value) {
            _buffer.Add(LogMessage.I(value));
        }

        override public void WriteLine(string value) {
            _buffer.Add(LogMessage.I(value));
        }

        public void D(int debugLevel, string message) {
#if DEBUG
            if (debugLevel <= Log.DebugLevel) {
                _buffer.Add(LogMessage.D(debugLevel, message));
            }
#endif
        }

        public void I(string message) => _buffer.Add(LogMessage.I(message));

        public void W(string message) => _buffer.Add(LogMessage.W(message));

        public void E(string message) => _buffer.Add(LogMessage.E(message));

        override public void Flush() {
            foreach (var message in _buffer) {
                _parentLog.Write(message);
            }
            Reset();
        }
    }

    public class LogMessage {
        public LogLevel Level { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public int DebugLevel { get; set; }

        public LogMessage(LogLevel level, string message) :
            this(level, 0, message) { }

        public LogMessage(LogLevel level, int debugLevel, string message) {
            Level = level;
            Time = DateTime.UtcNow;
            Message = message;
            DebugLevel = debugLevel;
        }

        public static LogMessage D(int debugLevel, string message) =>
            new LogMessage(LogLevel.Debug, debugLevel, message);
        public static LogMessage I(string message) =>
            new LogMessage(LogLevel.Info, message);
        public static LogMessage W(string message) =>
            new LogMessage(LogLevel.Warning, message);
        public static LogMessage E(string message) =>
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

    public static Log Create(string name) =>
        new Log(name);

    public static Log CreateForThisTest() =>
        new Log(new StackTrace().GetFrame(1).GetMethod().Name.Split('_').Last());
}