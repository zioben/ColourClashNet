using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Log
{
    /// <summary>
    /// Log levels
    /// </summary>

    [Serializable]
    public enum LogLevel
    {
        Fatal = 0,
        Exception = 1,
        Error = 2,
        Warning = 10,
        Pass = 50,
        Message = 100,
        Debug = 1000,
        Trace = 2000,
    }
}
