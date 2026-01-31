using ColourClashNet.Color.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color;

public sealed class DitheringStrengthDescriptor
    : ColorTransformPropertyDescriptor<double>
{
    public DitheringStrengthDescriptor()
        : base(ColorTransformProperties.DitheringStrength) { }

    public override string DisplayName => "Dithering Strength";
    public override string Description => "Dithering intensity (0..1)";

    public override double DefaultValueTyped => 1.0;

    public override object Normalize(object value)
    {
        var d = (double)base.Normalize(value);
        return Math.Clamp(d, 0.0, 1.0);
    }

    protected override void ApplyTyped(ColorTransformBase transform, double value)
    {
        transform.DitheringStrength = value;
    }
}