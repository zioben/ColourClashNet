using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet
{
    public class Chrono
    {
        public DateTime? TimeStart { get; private set; } = null;
        public DateTime? TimeStop { get; private set; } = null;

        public void Start()
        {
            TimeStart = DateTime.Now;
            TimeStop = null;
        }

        public void Update()
        {
            TimeStop = DateTime.Now;
        }

        public double ElapsedMilliseconds
        {
            get
            {
                if (TimeStart == null)
                {
                    return 0;
                }
                if (TimeStop == null)
                {
                    return (DateTime.Now - TimeStart.Value).TotalMilliseconds;
                }
                return (TimeStop.Value - TimeStart.Value).TotalMilliseconds;
            }
        }
    }
}
