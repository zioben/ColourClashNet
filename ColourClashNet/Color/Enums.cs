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

    public enum ColorDithering
    {
        None=0,
        Ordered_2x2,
        Ordered_4x4,
        Ordered_8x8,
    }

    public enum Parameters
    {
        Unknown = 0,
        ColorQuantization,
        ColorDistanceMode,
        PaletteEntries,
        DitherMode,
    }

}
