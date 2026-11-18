using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet
{
    /// <summary>
    /// Generic math and culture-invariant conversion helpers.
    /// </summary>
    public static class ColourTools
    {
        #region math/conversion

        /// <summary>
        /// Clamps <paramref name="value"/> into the closed range [<paramref name="min"/>, <paramref name="max"/>].
        /// </summary>
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            else if (value.CompareTo(max) > 0) return max;
            else return value;
        }

        /// <summary>
        /// Converts <paramref name="value"/> to double and clamps it into [<paramref name="min"/>, <paramref name="max"/>].
        /// </summary>
        public static double Clamp(object value, double min, double max)
        {
            return Math.Min(Math.Max(ToDouble(value), min), max);
        }

        /// <summary>
        /// Converts an <see cref="IConvertible"/> value to double using the invariant culture.
        /// </summary>
        /// <exception cref="ArgumentException">The value is not convertible.</exception>
        public static double ToDouble(object value)
        {
            if (value is IConvertible c)
                return c.ToDouble(CultureInfo.InvariantCulture);
            throw new ArgumentException($"Invalid value type for conversion to double: {value?.GetType().Name}");
        }

        /// <summary>
        /// Converts an <see cref="IConvertible"/> value to boolean using the invariant culture.
        /// </summary>
        /// <exception cref="ArgumentException">The value is not convertible.</exception>
        public static bool ToBool(object value)
        {
            if (value is IConvertible c)
                return c.ToBoolean(CultureInfo.InvariantCulture);
            throw new ArgumentException($"Invalid value type for conversion to boolean: {value?.GetType().Name}");
        }

        /// <summary>
        /// Converts an <see cref="IConvertible"/> value to int using the invariant culture.
        /// </summary>
        /// <exception cref="ArgumentException">The value is not convertible.</exception>
        public static int ToInt(object value)
        {
            if (value is IConvertible c)
                return c.ToInt32(CultureInfo.InvariantCulture);
            throw new ArgumentException($"Invalid value type for conversion to int: {value?.GetType().Name}");
        }

        /// <summary>
        /// Parses the string representation of <paramref name="value"/> into the enum <typeparamref name="T"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">The value is not a valid <typeparamref name="T"/> name or number.</exception>
        public static T ToEnum<T>(object value) where T : struct, Enum
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (Enum.TryParse<T>(value.ToString(), out var val))
                return val;

            throw new ArgumentException($"Invalid value for enum conversion to {typeof(T).Name}: {value}", nameof(value));
        }

        #endregion
    }
}
