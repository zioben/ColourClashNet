using System;
using System.Diagnostics;

namespace ColourClashNet
{
    public class StopWatch
    {
        private readonly Stopwatch _sw = new();

        public DateTime? TimeStart { get; private set; } = null;
        public DateTime? TimeStop { get; private set; } = null;

        public void Start()
        {
            TimeStart = DateTime.Now;
            TimeStop = null;
            _sw.Restart();
        }

        public void Update()
        {
            TimeStop = DateTime.Now;
            _sw.Stop();
        }

        public double ElapsedMilliseconds => _sw.Elapsed.TotalMilliseconds;
    }
}