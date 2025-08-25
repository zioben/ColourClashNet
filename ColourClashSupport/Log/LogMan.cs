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

namespace ColourClashSupport.Log
{

    /// <summary>
    /// Log Manager
    /// </summary>
    public class LogMan
    {
        static LogLevel eMinLogLevel = LogLevel.Debug;

        public static event EventHandler<LogEventArgs> OnLogMessage;

        [Browsable(true), Category("Config")]
        [Description("Log minimum level. Can be set any time")]
        public static LogLevel MinLogLevel
        {
            get => eMinLogLevel;
            set
            {
                eMinLogLevel = value;
                LogManager.Configuration.AddRule(ToNLogLevel(eMinLogLevel), NLog.LogLevel.Fatal, sLogTarget);
            }
        }


        #region constant paths and rules

        static readonly string sLogFileName = "Log";
        static readonly string sLogFolder = "Logs";
        static readonly string sLogArchive = "LogsArchive";
        static readonly string sLogTarget = "DeltaLog";
        static readonly string sLogRule = "DeltaRule";
        static readonly string sLogID = "DefaultLog";

        #endregion

        static NLog.LogLevel ToNLogLevel(LogLevel eLevel)
        {
            switch (eLevel)
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

        static NLog.Logger NLogger => LogManager.GetLogger(sLogID);

        static void CreateNLog()
        {
            Directory.CreateDirectory($"./{sLogFolder}");
            LogManager.Setup();
            // Creazione configurazione
            var nLogConf = new LoggingConfiguration();
            // Target per scrivere su file
            var fTarget = new FileTarget(sLogTarget);
            fTarget.FileName = "${basedir}" + $"/{sLogFolder}/{sLogFileName}.txt";
            //fTarget.FileName = "C:/TestX/" + sLogFileName + ".txt";
            //fTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss}.${date:format=fff}" +
            //    "|${level:uppercase=true}" +
            //    "|${message}" +
            //    " ${onexception: ${exception:format=ToString,format=Data};}";
            fTarget.Layout = "${message}" +
                "${onexception: | ${exception:format=ToString,format=Data};}";
            fTarget.KeepFileOpen = true;
            fTarget.Encoding = Encoding.UTF8;
            fTarget.CreateDirs = true;
            fTarget.ArchiveAboveSize = 4 * 1024 * 1024;
            fTarget.ArchiveSuffixFormat = $"{0:yyyyMMdd-HHmmss}_{1:000}";
            //fTarget.ArchiveDateFormat = "yyyyMMdd-HHmmss";
            fTarget.ArchiveFileName = "${basedir}" + $"/{sLogFolder}/{sLogArchive}/{sLogFileName}" + "_{#}.txt";
            //fTarget.ArchiveNumbering = ArchiveNumberingMode.Sequence;
            fTarget.MaxArchiveFiles = 100;
            //fTarget.EnableArchiveFileCompression = true;
            //fTarget.ConcurrentWrites = true;

            nLogConf.AddTarget(sLogID, fTarget);
            nLogConf.AddRule(ToNLogLevel(MinLogLevel), NLog.LogLevel.Fatal, fTarget);
            // Windows Trace on NLOG
            //System.Diagnostics.Trace.Listeners.Clear();
            //System.Diagnostics.Trace.Listeners.Add(new NLog.NLogTraceListener { Name = "NLog" });
            //System.Diagnostics.Trace.WriteLine("Hello World");
            LogManager.Configuration = nLogConf;

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
            string sMsg = $"{Environment.NewLine}Exception Dump{Environment.NewLine}";
            do
            {
                sMsg += $"InnerMessage : {ex.Message}{Environment.NewLine}";
                ex = ex.InnerException;
            }
            while (ex != null);
            return sMsg;
        }


        /// <summary>
        /// Generic log message handler
        /// </summary>
        /// <param name="eLevel">Log Level</param>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="sMessage">Log message</param>
        /// <param name="ex">Exception info</param>
        static void Log(LogLevel eLevel, string sClass, string sMethod, string sMessage, Exception ex)
        {
            if (eLevel > MinLogLevel)
                return;

            var oDT = DateTime.Now;
            string sMsg = $"{oDT:yyyy-MM-dd HH:mm:ss.fff} | {eLevel} | {sClass}.{sMethod} : {sMessage} {GetExceptionMessage(ex)}";
            //string sMsgNLog = $"{eLevel} | {sClass}.{sMethod} : {sMessage}";

            switch (eLevel)
            {
                case LogLevel.Fatal:
                    System.Diagnostics.Trace.TraceError(sMsg);
                    NLogger?.Fatal(ex, sMsg);
                    break;
                case LogLevel.Exception:
                case LogLevel.Error:
                    System.Diagnostics.Trace.TraceError(sMsg);
                    NLogger?.Error(ex, sMsg);
                    break;
                case LogLevel.Warning:
                    System.Diagnostics.Trace.TraceWarning(sMsg);
                    NLogger?.Warn(ex, sMsg);
                    break;
                default:
                    System.Diagnostics.Trace.TraceInformation(sMsg);
                    NLogger?.Info(ex, sMsg);
                    break;
            }

            try
            {
                OnLogMessage?.Invoke(null, new LogEventArgs()
                {
                    Level = eLevel,
                    Timestamp = oDT,
                    Message = sMsg,
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
        /// <param name="sMessage">Log message</param>
        /// <param name="ex">Exception info</param>
        public static void Fatal(string sClass, string sMethod, string sMessage, Exception ex)
        {
            Log(LogLevel.Fatal, sClass, sMethod, sMessage, ex);
        }

        /// <summary>
        /// Trace an Exception message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="sMessage">Log message</param>
        /// <param name="ex">Exception info</param>
        public static void Exception(string sClass, string sMethod, string sMessage, Exception ex)
        {
            Log(LogLevel.Exception, sClass, sMethod, sMessage, ex);
        }

        /// <summary>
        /// Trace an Error message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="sMessage">Log message</param>
        /// <param name="ex">Exception info</param>
        public static void Error(string sClass, string sMethod, string sMessage, Exception ex)
        {
            var eLevel = ex == null ? LogLevel.Error : LogLevel.Fatal;
            Log(eLevel, sClass, sMethod, sMessage, ex);
        }

        /// <summary>
        /// Trace an Error message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="sMessage">Log message</param>
        public static void Error(string sClass, string sMethod, string sMessage)
        {
            Log(LogLevel.Error, sClass, sMethod, sMessage, null);
        }

        /// <summary>
        /// Trace a Warning message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="sMessage">Log message</param>
        public static void Warning(string sClass, string sMethod, string sMessage)
        {
            Log(LogLevel.Warning, sClass, sMethod, sMessage, null);
        }

        /// <summary>
        /// Trace a Pass OK message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="sMessage">Log message</param>
        public static void Pass(string sClass, string sMethod, string sMessage)
        {
            Log(LogLevel.Pass, sClass, sMethod, sMessage, null);
        }

        /// <summary>
        /// Trace a  message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="sMessage">Log message</param>
        public static void Message(string sClass, string sMethod, string sMessage)
        {
            Log(LogLevel.Message, sClass, sMethod, sMessage, null);
        }

        /// <summary>
        /// Trace a debug message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="sMessage">Log message</param>
        public static void Debug(string sClass, string sMethod, string sMessage)
        {
            Log(LogLevel.Debug, sClass, sMethod, sMessage, null);
        }

        /// <summary>
        /// Trace lowest level message
        /// </summary>
        /// <param name="sClass">Source Class</param>
        /// <param name="sMethod">Source Method</param>
        /// <param name="sMessage">Log message</param>
        public static void Trace(string sClass, string sMethod, string sMessage)
        {
            Log(LogLevel.Debug, sClass, sMethod, sMessage, null);
        }
    }
}
