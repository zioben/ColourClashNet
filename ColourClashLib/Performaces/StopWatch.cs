using System;
using System.Diagnostics;

namespace ColourClashNet
{
    /// <summary>
    /// Lightweight timer that records wall-clock start/stop times and the high resolution elapsed time.
    /// </summary>
    public class StopWatch
    {
        private readonly Stopwatch stopwatch = new();

        /// <summary>Wall-clock time of the last <see cref="Start"/> call.</summary>
        public DateTime? TimeStart { get; private set; } = null;

        /// <summary>Wall-clock time of the last <see cref="Update"/> call; null while running.</summary>
        public DateTime? TimeStop { get; private set; } = null;

        /// <summary>
        /// Resets and starts the measurement.
        /// </summary>
        public void Start()
        {
            TimeStart = DateTime.Now;
            TimeStop = null;
            stopwatch.Restart();
        }

        /// <summary>
        /// Stops the measurement and stores the stop time.
        /// </summary>
        public void Update()
        {
            TimeStop = DateTime.Now;
            stopwatch.Stop();
        }

        /// <summary>Elapsed time between <see cref="Start"/> and <see cref="Update"/>, in milliseconds.</summary>
        public double ElapsedMilliseconds => stopwatch.Elapsed.TotalMilliseconds;
    }
}