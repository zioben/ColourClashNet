using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color;

    public sealed class DitheringTypeDescriptor
    : ColorTransformPropertyDescriptor<ColorDithering>
    {
        public DitheringTypeDescriptor()
            : base(ColorTransformProperties.Dithering_Type) { }

        public override string DisplayName => "Dithering Type";
        public override string Description => "Dithering algorithm";

        public override ColorDithering DefaultValueTyped => ColorDithering.None;

        protected override void ApplyTyped(ColorTransformBase transform, ColorDithering value)
        {
            transform.DitheringType = value;
        }
    }

