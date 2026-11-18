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
    /// Log Manager
    /// </summary>
    public class LogMan
    {
        static LogLevel minLogLevel = LogLevel.Debug;

        public static event EventHandler<LogEventArgs> OnLogMessage;

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

        static void CreateNLog()
        {
            Directory.CreateDirectory($"./{logFolder}");
            LogManager.Setup();
            // Creazione configurazione
            var logConf = new LoggingConfiguration();
            // Target per scrivere su file
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

        static void DestroyNLog()
        {
            LogManager.Shutdown();
        }

        /// <summary>
        /// Log resource allocator
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

        public static void Destroy()
        {
            DestroyNLog();
        }


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
        /// Generic log message handler
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
            //string sMsgNLog = $"{level} | {sClass}.{sMethod} : {message}";

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
        /// Trace a Fatal message (application hangs)
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
        /// Trace an Exception message
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
        /// Trace an Exception message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="ex">Exception info</param>
        public static void Exception(string sClass, string sMethod, Exception ex)
        {
            Log(LogLevel.Exception, sClass, sMethod, "Exception raised", ex);
        }


        /// <summary>
        /// Trace an Error message
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
        /// Trace an Error message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        public static void Error(string sClass, string sMethod, string message)
        {
            Log(LogLevel.Error, sClass, sMethod, message, null);
        }

        /// <summary>
        /// Trace a Warning message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        public static void Warning(string sClass, string sMethod, string message)
        {
            Log(LogLevel.Warning, sClass, sMethod, message, null);
        }

        /// <summary>
        /// Trace a Pass OK message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        public static void Pass(string sClass, string sMethod, string message)
        {
            Log(LogLevel.Pass, sClass, sMethod, message, null);
        }

        /// <summary>
        /// Trace a  message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        public static void Message(string sClass, string sMethod, string message)
        {
            Log(LogLevel.Message, sClass, sMethod, message, null);
        }

        /// <summary>
        /// Trace a debug message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="message">Log message</param>
        public static void Debug(string sClass, string sMethod, string message)
        {
            Log(LogLevel.Debug, sClass, sMethod, message, null);
        }

        /// <summary>
        /// Trace lowest level message
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
