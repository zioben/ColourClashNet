using ColourClashSupport.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib
{
    public static class ColorDefaults
    {
        public static bool TraceX
        {
            get { return LogMan.MinLogLevel == LogLevel.Trace; }
            set { LogMan.MinLogLevel = LogLevel.Trace; }
        }
        public static int DefaultDitherKernelSize { get; set; } = 4;

    }
}
