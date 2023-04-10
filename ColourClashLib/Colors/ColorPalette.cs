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
    public class ColorPalette
    {
        public static ColorPalette MergeColorPalette(List<ColorPalette> lSourcePalette)
        {
            if (lSourcePalette == null )
                return null;
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
            if (oRet.Colors > 0)
            {
                return oRet;
            }
            return null;
        }

        public static ColorPalette MergeColorPalette(ColorPalette oSourcePaletteA, ColorPalette oSourcePaletteB)
        {
            return MergeColorPalette(new List<ColorPalette> { oSourcePaletteA, oSourcePaletteB });
        }

        public static ColorPalette CreateColorPalette(ColorPalette oSourcePalette)
        {
            return MergeColorPalette(new List<ColorPalette> { oSourcePalette } );
        }

        public static ColorPalette CreateColorPalette(List<int>? oSourcePalette)
        {
            if (oSourcePalette == null || oSourcePalette.Count == 0)
                return null;
            var oRet = new ColorPalette();
            oSourcePalette.ForEach(X => oRet.Add(X));
            return oRet;    
        }


        public HashSet<int> rgbPalette { get; private init; } = new HashSet<int>();
        public int Colors => rgbPalette.Count;
        public void Add(int iRGB ) => rgbPalette.Add( iRGB );
        public void Remove(int iRGB) => rgbPalette.Remove(iRGB);

        public List<int> ToList() => rgbPalette?.ToList() ?? new List<int>();

        public void Reset()
        {
            rgbPalette.Clear(); 
        }

        

    }
}
