using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ColourClashNet.Color.Dithering
{
    /// <summary>
    /// Serializable dithering settings, editable in a PropertyGrid.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DitherConfig
    {
        /// <summary>Dithering algorithm.</summary>
        public ColorDithering DitheringType { get; set; } = ColorDithering.None;

        /// <summary>Dithering strength multiplier (1.0 = nominal).</summary>
        public double DitheringStrength { get; set; } = 1.0;

        /// <summary>Optional post-dithering effect.</summary>
        public ColorDitheringFx DitheringFx { get; set; } = ColorDitheringFx.None;

        /// <summary>
        /// Creates a copy of this configuration.
        /// </summary>
        public object Clone()
        {
            return new DitherConfig()
            {
                DitheringType = this.DitheringType,
                DitheringStrength = this.DitheringStrength,
                DitheringFx = this.DitheringFx
            };
        }

        public override string ToString()
        {
            return $"DitheringType: {DitheringType}, DitheringStrength: {DitheringStrength}, DitheringFx: {DitheringFx}";
        }
    }
}
