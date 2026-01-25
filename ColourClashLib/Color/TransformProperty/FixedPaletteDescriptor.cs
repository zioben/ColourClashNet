using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ColourClashNet.Color;

public sealed class FixedPaletteDescriptor
    : ColorTransformPropertyDescriptor<Palette>
{
    public FixedPaletteDescriptor()
        : base(ColorTransformProperties.Fixed_Palette) { }

    public override string DisplayName => "Fixed Palette";
    public override string Description => "Custom color palette";

    public override Palette DefaultValueTyped => new Palette();

    public override object Normalize(object value)
    {
        return value switch
        {
            Palette p => p,
            IEnumerable<int> seq => Palette.CreatePalette(seq),
            _ => throw new ArgumentException("Invalid palette value")
        };
    }

    protected override void ApplyTyped(ColorTransformBase transform, Palette value)
    {
        transform.FixedPalette = Palette.CreatePalette( value );
    }
}
