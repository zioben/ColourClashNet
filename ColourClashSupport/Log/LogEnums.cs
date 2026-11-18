using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Log
{
    /// <summary>
    /// Log severity levels. Lower values are more severe.
    /// </summary>
    [Serializable]
    public enum LogLevel
    {
        /// <summary>Unrecoverable condition.</summary>
        Fatal = 0,
        /// <summary>Caught exception.</summary>
        Exception = 1,
        /// <summary>Error that does not stop the application.</summary>
        Error = 2,
        /// <summary>Unexpected but handled condition.</summary>
        Warning = 10,
        /// <summary>A processing step completed successfully.</summary>
        Pass = 50,
        /// <summary>Informational message.</summary>
        Message = 100,
        /// <summary>Debug information.</summary>
        Debug = 1000,
        /// <summary>Very detailed diagnostic information.</summary>
        Trace = 2000,
    }
}
