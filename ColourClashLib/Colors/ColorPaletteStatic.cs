using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Color
{
    public partial class ColorPalette
    {
        public static ColorPalette? MergeColorPalette(List<ColorPalette> lSourcePalette)
        {
            if (lSourcePalette == null)
            {
                return null;
            }
            var oRet = new ColorPalette();
            foreach (var oPal in lSourcePalette)
            {
                if( oPal == null) 
                    continue;
                foreach (var iRGB in oPal.rgbPalette)
                {
                    oRet.Add(iRGB);
                }
            }
            if (oRet.Count > 0)
            {
                return oRet;
            }
            return null;
        }

        public static ColorPalette? MergeColorPalette(ColorPalette oSourcePaletteA, ColorPalette oSourcePaletteB)
        {
            return MergeColorPalette(new List<ColorPalette> { oSourcePaletteA, oSourcePaletteB });
        }

        public static ColorPalette? CreateColorPalette(ColorPalette oSourcePalette)
        {
            return MergeColorPalette(new List<ColorPalette> { oSourcePalette } );
        }

        public static ColorPalette? FromList(List<int>? oSourceList)
        {
            if (oSourceList == null || oSourceList.Count == 0)
                return null;
            var oRet = new ColorPalette();
            oSourceList.ForEach(X => oRet.Add(X));
            return oRet;    
        }

        public static ColorPalette? FromList(IEnumerable<int>? oSourceEnumerable) => FromList(oSourceEnumerable?.ToList());
    }
}
