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

        public HashSet<int> rgbPalette { get; private init; } = new HashSet<int>();
        public int Count => rgbPalette.Count;
        public void Add(int iRGB ) => rgbPalette.Add(iRGB);
        public void Remove(int iRGB) => rgbPalette.Remove(iRGB);
        public List<int> ToList() => rgbPalette?.ToList() ?? new List<int>();
        public void Reset()
        {
            rgbPalette.Clear(); 
        }     
    }
}
