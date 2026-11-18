using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Conversion
{
    /// <summary>
    /// Common contract of color space converters (RGB, HSV, LAB, GRAY).
    /// </summary>
    public interface ColorConverterInterface
    {
        /// <summary>
        /// Loads the converter from an integer RGB color.
        /// </summary>
        /// <param name="rgb">Packed RGB color</param>
        /// <returns>True if the color was valid and has been converted.</returns>
        public bool FromIntRGB(int rgb);

        /// <summary>
        /// Converts the current value back to an integer RGB color.
        /// </summary>
        public int ToIntRGB();

        /// <summary>
        /// True when the converter holds a valid color.
        /// </summary>
        public bool IsValid { get; }
    }
}
