using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Log
{

    /// <summary>
    /// Static logging facade used by the whole solution.
    /// <para/>
    /// Each message is formatted once and dispatched to <see cref="System.Diagnostics.Trace"/>,
    /// to an NLog rolling file target and to the <see cref="OnLogMessage"/> event, so that UI
    /// components can display the log in real time.
    /// </summary>
    public class LogMan
    {
        static LogLevel minLogLevel = LogLevel.Debug;

        /// <summary>
        /// Raised for every message that passes the <see cref="MinLogLevel"/> filter.
        /// </summary>
        public static event EventHandler<LogEventArgs> OnLogMessage;

        /// <summary>
        /// Minimum severity that is logged; messages with a higher (less severe) level are discarded.
        /// </summary>
        [Browsable(true), Category("Config")]
        [Description("Log minimum level. Can be set any time")]
        public static LogLevel MinLogLevel
        {
            get => minLogLevel;
            set
            {
                minLogLevel = value;
                LogManager.Configuration.AddRule(ToNLogLevel(minLogLevel), NLog.LogLevel.Fatal, logTarget);
            }
        }


        #region constant paths and rules

        static readonly string logFileName = "Log";
        static readonly string logFolder = "Logs";
        static readonly string logArchive = "LogsArchive";
        static readonly string logTarget = "DeltaLog";
        static readonly string logRule = "DeltaRule";
        static readonly string logId = "DefaultLog";

        #endregion

        /// <summary>
        /// Maps the internal <see cref="LogLevel"/> to the equivalent NLog level.
        /// </summary>
        static NLog.LogLevel ToNLogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Fatal:
                    return NLog.LogLevel.Fatal;
                case LogLevel.Exception:
                    return NLog.LogLevel.Error;
                case LogLevel.Error:
                    return NLog.LogLevel.Error;
                case LogLevel.Warning:
                    return NLog.LogLevel.Warn;
                case LogLevel.Pass:
                case LogLevel.Message:
                    return NLog.LogLevel.Info;
                case LogLevel.Debug:
                    return NLog.LogLevel.Debug;
                case LogLevel.Trace:
                    return NLog.LogLevel.Trace;
                default:
                    return NLog.LogLevel.Info;
            }
        }

        static NLog.Logger NLogger => LogManager.GetLogger(logId);

        /// <summary>
        /// Builds the NLog configuration: a single size-based rolling file target under <c>./Logs</c>.
        /// </summary>
        static void CreateNLog()
        {
            Directory.CreateDirectory($"./{logFolder}");
            LogManager.Setup();
            // Build the logging configuration
            var logConf = new LoggingConfiguration();
            // File target: the message is already fully formatted by Log(), so the layout only appends exception data
            var fileTarget = new FileTarget(logTarget);
            fileTarget.FileName = "${basedir}" + $"/{logFolder}/{logFileName}.txt";
            //fileTarget.FileName = "C:/TestX/" + logFileName + ".txt";
            //fileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss}.${date:format=fff}" +
            //    "|${level:uppercase=true}" +
            //    "|${message}" +
            //    " ${onexception: ${exception:format=ToString,format=Data};}";
            fileTarget.Layout = "${message}" +
                "${onexception: | ${exception:format=ToString,format=Data};}";
            fileTarget.KeepFileOpen = true;
            fileTarget.Encoding = Encoding.UTF8;
            fileTarget.CreateDirs = true;
            fileTarget.ArchiveAboveSize = 4 * 1024 * 1024;
            // Archive when the file exceeds 4 MB, keeping at most 100 archived files
            fileTarget.ArchiveSuffixFormat = $"{0:yyyyMMdd-HHmmss}_{1:000}";
            //fileTarget.ArchiveDateFormat = "yyyyMMdd-HHmmss";
            fileTarget.ArchiveFileName = "${basedir}" + $"/{logFolder}/{logArchive}/{logFileName}" + "_{#}.txt";
            //fileTarget.ArchiveNumbering = ArchiveNumberingMode.Sequence;
            fileTarget.MaxArchiveFiles = 100;
            //fileTarget.EnableArchiveFileCompression = true;
            //fileTarget.ConcurrentWrites = true;

            logConf.AddTarget(logId, fileTarget);
            logConf.AddRule(ToNLogLevel(MinLogLevel), NLog.LogLevel.Fatal, fileTarget);
            // Windows Trace on NLOG
            //System.Diagnostics.Trace.Listeners.Clear();
            //System.Diagnostics.Trace.Listeners.Add(new NLog.NLogTraceListener { Name = "NLog" });
            //System.Diagnostics.Trace.WriteLine("Hello World");
            LogManager.Configuration = logConf;

        }

        /// <summary>
        /// Flushes and closes all NLog targets.
        /// </summary>
        static void DestroyNLog()
        {
            LogManager.Shutdown();
        }

        /// <summary>
        /// Initializes the logging infrastructure. Call once at application startup.
        /// </summary>
        public static void Create()
        {
            CreateNLog();
            // Test only
            //NLogger.Trace("Sample trace message");
            //NLogger.Debug("Sample debug message");
            //NLogger.Info("Sample informational message");
            //NLogger.Warn("Sample warning message");
            //NLogger.Error("Sample error message");
            //NLogger.Fatal("Sample fatal error message");
        }

        /// <summary>
        /// Releases the logging infrastructure. Call once at application shutdown.
        /// </summary>
        public static void Destroy()
        {
            DestroyNLog();
        }


        /// <summary>
        /// Builds a text dump of the exception chain (outer and all inner exception messages).
        /// </summary>
        /// <param name="ex">Exception to dump; may be null.</param>
        /// <returns>The dump text, or an empty string when <paramref name="ex"/> is null.</returns>
        static string GetExceptionMessage(Exception ex)
        {
            if (ex == null)
                return "";
            string logLine = $"{Environment.NewLine}Exception Dump{Environment.NewLine}";
            do
            {
                logLine += $"InnerMessage : {ex.Message}{Environment.NewLine}";
                ex = ex.InnerException;
            }
            while (ex != null);
            return logLine;
        }


        /// <summary>
        /// Formats a log line and dispatches it to Trace, NLog and <see cref="OnLogMessage"/>.
        /// </summary>
        /// <param name="level">Log Level</param>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        /// <param name="ex">Exception info</param>
        static void Log(LogLevel level, string sClass, string sMethod, string message, Exception ex)
        {
            if (level > MinLogLevel)
                return;

            var now = DateTime.Now;
            string logLine = $"{now:yyyy-MM-dd HH:mm:ss.fff} | {level} | {sClass}.{sMethod} : {message} {GetExceptionMessage(ex)}";
            //string nlogLine = $"{level} | {sClass}.{sMethod} : {message}";

            switch (level)
            {
                case LogLevel.Fatal:
                    System.Diagnostics.Trace.TraceError(logLine);
                    NLogger?.Fatal(ex, logLine);
                    break;
                case LogLevel.Exception:
                case LogLevel.Error:
                    System.Diagnostics.Trace.TraceError(logLine);
                    NLogger?.Error(ex, logLine);
                    break;
                case LogLevel.Warning:
                    System.Diagnostics.Trace.TraceWarning(logLine);
                    NLogger?.Warn(ex, logLine);
                    break;
                default:
                    System.Diagnostics.Trace.TraceInformation(logLine);
                    NLogger?.Info(ex, logLine);
                    break;
            }

            try
            {
                // Subscriber failures must never break the caller: exceptions are swallowed on purpose
                OnLogMessage?.Invoke(null, new LogEventArgs()
                {
                    Level = level,
                    Timestamp = now,
                    Message = logLine,
                    Except = ex
                });
            }
            catch
            {
            }
        }

        /// <summary>
        /// Logs a fatal message (unrecoverable condition).
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        /// <param name="ex">Exception info</param>
        public static void Fatal(string sClass, string sMethod, string message, Exception ex)
        {
            Log(LogLevel.Fatal, sClass, sMethod, message, ex);
        }

        /// <summary>
        /// Logs a caught exception with a custom message.
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        /// <param name="ex">Exception info</param>
        public static void Exception(string sClass, string sMethod, string message, Exception ex)
        {
            Log(LogLevel.Exception, sClass, sMethod, message, ex);
        }

        /// <summary>
        /// Logs a caught exception with a default message.
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="ex">Exception info</param>
        public static void Exception(string sClass, string sMethod, Exception ex)
        {
            Log(LogLevel.Exception, sClass, sMethod, "Exception raised", ex);
        }


        /// <summary>
        /// Logs an error message.
        /// <para/>
        /// When <paramref name="ex"/> is not null the message is promoted to <see cref="LogLevel.Fatal"/>.
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        /// <param name="ex">Exception info</param>
        public static void Error(string sClass, string sMethod, string message, Exception ex)
        {
            var level = ex == null ? LogLevel.Error : LogLevel.Fatal;
            Log(level, sClass, sMethod, message, ex);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        public static void Error(string sClass, string sMethod, string message)
        {
            Log(LogLevel.Error, sClass, sMethod, message, null);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        public static void Warning(string sClass, string sMethod, string message)
        {
            Log(LogLevel.Warning, sClass, sMethod, message, null);
        }

        /// <summary>
        /// Logs a "step passed" message.
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        public static void Pass(string sClass, string sMethod, string message)
        {
            Log(LogLevel.Pass, sClass, sMethod, message, null);
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        public static void Message(string sClass, string sMethod, string message)
        {
            Log(LogLevel.Message, sClass, sMethod, message, null);
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        public static void Debug(string sClass, string sMethod, string message)
        {
            Log(LogLevel.Debug, sClass, sMethod, message, null);
        }

        /// <summary>
        /// Logs a trace (lowest severity) message.
        /// <para/>
        /// Note: the message is currently written with <see cref="LogLevel.Debug"/> severity.
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        public static void Trace(string sClass, string sMethod, string message)
        {
            Log(LogLevel.Debug, sClass, sMethod, message, null);
        }
    }
}
