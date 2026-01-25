using ColourClashNet.Color.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color;

public interface IColorTransformPropertyDescriptor
{
    ColorTransformProperties Property { get; }

    string DisplayName { get; }
    string Description { get; }

    Type ValueType { get; }

    object DefaultValue { get; }

    bool IsSupported(ColorTransformBase transform);

    void Apply(ColorTransformBase transform, object value);

    object Normalize(object value); // clamp / cast / enum parse
}
