using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public enum ColorQuantizationMode
    {
        Unknown = 0,
        RGB888,
        RGB565,
        RGB555,
        RGB444,
        RGB333,
    }

    public enum Colorspace
    {
        RGB,
        HSV,
        LAB,
        XYZ,
    }

    public enum ColorDistanceEvaluationMode
    {
        All,
        RGB,
        HSV,
    }

}
