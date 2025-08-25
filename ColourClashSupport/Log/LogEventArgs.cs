using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashSupport.Log
{
    public class LogEventArgs : EventArgs
    {
        public DateTime Timestamp { get; internal set; } = DateTime.Now;
        public LogLevel Level { get; internal set; }
        public string? Message { get; internal set; }
        public Exception? Except { get; internal set; }

        public override string ToString()
        {
            return $"{Message}";
        }
    }
}
