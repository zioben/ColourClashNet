using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
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
        public int[,]? DataSource { get; internal set; }
        public int[,]? DataDest { get; internal set; }
    }
}