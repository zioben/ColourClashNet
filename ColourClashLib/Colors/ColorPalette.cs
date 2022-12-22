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
        public HashSet<int> rgbPalette { get; private init; } = new HashSet<int>();
        public int Colors => rgbPalette.Count;
        public List<int> ToList() => rgbPalette?.ToList() ?? new List<int>();

        public void Reset()
        {
            rgbPalette.Clear(); 
        }

    }
}
