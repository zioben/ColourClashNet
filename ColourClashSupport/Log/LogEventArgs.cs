using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Log
{
    /// <summary>
    /// Event data for <see cref="LogMan.OnLogMessage"/>.
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        /// <summary>Time the message was logged.</summary>
        public DateTime Timestamp { get; internal set; } = DateTime.Now;
        /// <summary>Message severity.</summary>
        public LogLevel Level { get; internal set; }
        /// <summary>Fully formatted log line.</summary>
        public string? Message { get; internal set; }
        /// <summary>Exception attached to the message, if any.</summary>
        public Exception? Except { get; internal set; }

        public override string ToString()
        {
            return $"{Message}";
        }
    }
}
