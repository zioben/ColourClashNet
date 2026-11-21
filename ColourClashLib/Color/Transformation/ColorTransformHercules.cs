using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Hercules monochrome mode: maps every color to black or white.
    /// </summary>
    public class ColorTransformHercules : ColorTransformPaletteMapping<HerculesOptions>
    {



        /// <summary>
        /// Creates the Hercules transformation.
        /// </summary>
        public ColorTransformHercules()
        {
            Type = ColorTransformType.ColorReductionHercules;
            Description = "Reduce color to Hercules palette";
        }

        static readonly Palette herculesPalette = new Palette().Create(new[] { ColorIntExt.FromRGB(0, 0, 0), ColorIntExt.FromRGB(255, 255, 255) });

        /// <summary>
        /// Returns the black and white palette.
        /// </summary>
        protected override Palette GetTargetPalette() => herculesPalette;


       
    }
}