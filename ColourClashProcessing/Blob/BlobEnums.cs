using System;
using System.Collections.Generic;
using System.Text;

namespace ColourClashProcessing.Blob
{
    /// <summary>
    /// Pixel connectivity used to group pixels into blobs.
    /// </summary>
    public enum BlobConnectionModes
    {
        /// <summary>
        /// Pixels touch horizontally or vertically.
        /// </summary>
        FourConnected,
        /// <summary>
        /// Pixels touch horizontally, vertically or diagonally.
        /// </summary>
        EightConnected,
    }
}
