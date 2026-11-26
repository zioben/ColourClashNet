using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace ColourClashNet.Drawing
{
    /// <summary>
    /// Event data raised by the color transformation manager around a processing run.
    /// </summary>
    public class ColorManagerProcessEventArgs : EventArgs
    {
        /// <summary>
        /// Transformation being executed.
        /// </summary>
        public IColorTransform? Transformation { get; internal set; }
        /// <summary>
        /// Source image.
        /// </summary>
        public ImageData DataSource { get; internal set; }
        /// <summary>
        /// Processed image.
        /// </summary>
        public ImageData DataDest { get; internal set; }
    }
}