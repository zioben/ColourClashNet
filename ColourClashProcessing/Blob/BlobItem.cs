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
        double sumX = 0;
        double sumY = 0;

        /// <summary>1-based identifier assigned by <see cref="BlobBuilder"/>.</summary>
        public int BlobId { get; internal set; } = -1;

        /// <summary>
        /// Number of pixels.
        /// </summary>
        public int Area { get; private set; } = 0;
        /// <summary>
        /// Bounding box left column.
        /// </summary>
        public int XMin { get; private set; } = int.MaxValue;
        /// <summary>
        /// Bounding box right column (inclusive).
        /// </summary>
        public int XMax { get; private set; } = int.MinValue;
        /// <summary>
        /// Bounding box top row.
        /// </summary>
        public int YMin { get; private set; } = int.MaxValue;
        /// <summary>
        /// Bounding box bottom row (inclusive).
        /// </summary>
        public int YMax { get; private set; } = int.MinValue;

        /// <summary>
        /// Bounding box width.
        /// </summary>
        public int Width => Area == 0 ? 0 : XMax - XMin + 1;
        /// <summary>
        /// Bounding box height.
        /// </summary>
        public int Height => Area == 0 ? 0 : YMax - YMin + 1;

        /// <summary>
        /// Centroid X in pixel coordinates (pixel centers).
        /// </summary>
        public double CentroidX => Area == 0 ? 0 : sumX / Area;
        /// <summary>
        /// Centroid Y in pixel coordinates (pixel centers).
        /// </summary>
        public double CentroidY => Area == 0 ? 0 : sumY / Area;

        /// <summary>
        /// Runs that make up the blob.
        /// </summary>
        public List<RunLength> RunLengths { get; private set; } = new List<RunLength>();

        /// <summary>
        /// Adds a run and updates area, bounding box and centroid sums.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="rl"/> is null.</exception>
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
            sumX += (rl.XS + rl.XE) * (double)rl.Length / 2.0;
            sumY += (double)rl.Y * rl.Length;
        }
    }
}
