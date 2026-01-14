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
    public class ColorManagerProcessEventArgs : EventArgs
    {
        public ColorTransformInterface? Transformation { get; internal set; }
        public ImageData DataSource { get; internal set; }
        public ImageData DataDest { get; internal set; }
    }
}