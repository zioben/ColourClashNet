using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ColourClashNet.Color.Dithering
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DitherConfig
    {
        public ColorDithering DitheringType { get; set; } = ColorDithering.None;
        public double DitheringStrength { get; set; } = 1.0;
        public ColorDitheringFx DitheringFx { get; set; } = ColorDitheringFx.None;

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
