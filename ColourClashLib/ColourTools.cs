using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet
{
    public static class ColourTools
    {
        #region math/conversion

        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            else if (value.CompareTo(max) > 0) return max;
            else return value;
        }
        public static double Clamp(object value, double min, double max)
        {
            return Math.Min(Math.Max(ToDouble(value), min), max);
        }

        public static double ToDouble(object value)
        {
            if (value is IConvertible c)
                return c.ToDouble(CultureInfo.InvariantCulture);
            throw new ArgumentException($"Invalid value type for conversion to double: {value?.GetType().Name}");
        }

        public static bool ToBool(object value)
        {
            if (value is IConvertible c)
                return c.ToBoolean(CultureInfo.InvariantCulture);
            throw new ArgumentException($"Invalid value type for conversion to boolean: {value?.GetType().Name}");
        }
        public static int ToInt(object value)
        {
            if (value is IConvertible c)
                return c.ToInt32(CultureInfo.InvariantCulture);
            throw new ArgumentException($"Invalid value type for conversion to boolean: {value?.GetType().Name}");
        }

        public static T ToEnum<T>(object value) where T : struct, Enum
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (Enum.TryParse<T>(value.ToString(), out var eVal))
                return eVal;

            throw new ArgumentException($"Invalid value for enum conversion to {typeof(T).Name}: {value}", nameof(value));
        }

        #endregion
    }
}
