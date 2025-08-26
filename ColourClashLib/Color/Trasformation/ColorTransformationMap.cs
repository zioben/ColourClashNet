using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Color.Trasformation
{
    /// <summary>
    /// Class to transform palette colors.
    /// </summary>
    public partial class ColorTransformationMap
    {
        static string sClass = nameof(ColorTransformationMap);
        public Dictionary<int, int> rgbTransformationMap { get; private init; } = new Dictionary<int, int>();
        //public int this[int index] => rgbTransformationMap[index];

        public void Reset()
        {
            rgbTransformationMap.Clear();
        }

        public int Colors => rgbTransformationMap.Count;

        public bool Create(ColorPalette oPalette)
        {
            Reset();
            if (oPalette == null)
            {
                return false;
            }
            foreach (var rgb in oPalette.rgbPalette)
            {
                if (rgb < 0)
                    continue;
                rgbTransformationMap[rgb] = rgb;    
            }
            return true;
        }

        public bool Create(int[,] oDataSource)
        {
            Reset();
            if (oDataSource == null)
            {
                return false;
            }
            ColorPalette oPalette = new ColorPalette();
            foreach (var rgb in oDataSource) 
            {
                oPalette.Add(rgb);
            }
            return Create(oPalette);
        }

        public void Add(int iSorceRGB, int iDestRGB)
        {
            if (iSorceRGB < 0 || iDestRGB < 0)
            {
                return;
            }
            if (rgbTransformationMap.ContainsKey(iSorceRGB))
            {
                rgbTransformationMap[iSorceRGB] = iDestRGB;
            }
            else
            {
                rgbTransformationMap.Add(iSorceRGB, iDestRGB);
            }
        }
        public void Remove(int iSorceRGB)
        {
            if (rgbTransformationMap.ContainsKey(iSorceRGB))
            {
                rgbTransformationMap.Remove(iSorceRGB);
            }
        }


        public ColorPalette ToColorPalette()
        {
            var oCP = new ColorPalette();
            foreach (var kvp in rgbTransformationMap)
            {
                if (kvp.Value < 0)
                    continue;
                oCP.Add(kvp.Value);
            }
            return oCP; 
        }

       
    }
}
