using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color;

public abstract class ColorTransformPropertyDescriptor<T>
 : IColorTransformPropertyDescriptor
{
    public ColorTransformProperties Property { get; }

    public abstract string DisplayName { get; }
    public abstract string Description { get; }

    public Type ValueType => typeof(T);

    public abstract T DefaultValueTyped { get; }
    public object DefaultValue => DefaultValueTyped;

    protected ColorTransformPropertyDescriptor(ColorTransformProperties property)
    {
        Property = property;
    }

    public virtual bool IsSupported(ColorTransformBase transform) => true;

    public void Apply(ColorTransformBase transform, object value)
    {
        var normalized = (T)Normalize(value);
        ApplyTyped(transform, normalized);
    }

    public virtual object Normalize(object value)
    {
        if (value is T t)
            return t;

        if (typeof(T).IsEnum)
            return Enum.Parse(typeof(T), value.ToString(), true);

        if (value is IConvertible c)
            return Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);

        throw new ArgumentException($"Invalid value for {Property}");
    }

    protected abstract void ApplyTyped(ColorTransformBase transform, T value);
}
