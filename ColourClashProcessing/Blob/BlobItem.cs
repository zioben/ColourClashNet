using System;
using System.Collections.Generic;

namespace ColourClashProcessing.Blob
{
    /// <summary>
    /// A connected component: the run lengths it is made of plus the statistics
    /// (area, bounding box, centroid) accumulated while the runs are added.
    /// </summary>
    public class BlobItem
    {
        double _sumX = 0;
        double _sumY = 0;

        /// <summary>1-based identifier assigned by <see cref="BlobBuilder"/>.</summary>
        public int BlobId { get; internal set; } = -1;

        public int Area { get; private set; } = 0;
        public int XMin { get; private set; } = int.MaxValue;
        public int XMax { get; private set; } = int.MinValue;
        public int YMin { get; private set; } = int.MaxValue;
        public int YMax { get; private set; } = int.MinValue;

        public int Width => Area == 0 ? 0 : XMax - XMin + 1;
        public int Height => Area == 0 ? 0 : YMax - YMin + 1;

        /// <summary>Centroid in pixel coordinates (pixel centers).</summary>
        public double CentroidX => Area == 0 ? 0 : _sumX / Area;
        public double CentroidY => Area == 0 ? 0 : _sumY / Area;

        public List<RunLength> RunLengths { get; private set; } = new List<RunLength>();

        public void Add(RunLength rl)
        {
            ArgumentNullException.ThrowIfNull(rl);

            RunLengths.Add(rl);
            Area += rl.Length;

            if (rl.XS < XMin) XMin = rl.XS;
            if (rl.XE > XMax) XMax = rl.XE;
            if (rl.Y < YMin) YMin = rl.Y;
            if (rl.Y > YMax) YMax = rl.Y;

            // sum of x over xs..xe = (xs + xe) * length / 2
            _sumX += (rl.XS + rl.XE) * (double)rl.Length / 2.0;
            _sumY += (double)rl.Y * rl.Length;
        }
    }
}
