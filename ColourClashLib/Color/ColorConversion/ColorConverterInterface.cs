using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    public interface ColorConverterInterface
    {

        public bool FromIntRGB(int rgb);
       
        public int ToIntRGB();

        public bool IsValid { get; }
    }
}
