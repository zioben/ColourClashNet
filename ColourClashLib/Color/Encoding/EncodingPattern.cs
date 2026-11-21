
namespace ColourClashNet.Color.Transformation
{

    /// <summary>
    /// Associates a color with the hardware specific pattern (e.g. pixel bits, attribute bytes) that encodes it.
    /// </summary>
    /// <typeparam name="T">Pattern type</typeparam>
    public class EncodingPattern<T> where T : class
    {
        /// <summary>Encoded color.</summary>
        internal int RGB { get; set; }

        /// <summary>Hardware pattern for <see cref="RGB"/>; null when not defined.</summary>
        internal T? Pattern { get; set; }
    }

    /// <summary>
    /// Collection of <see cref="EncodingPattern{T}"/> indexed by color.
    /// </summary>
    /// <typeparam name="T">Pattern type</typeparam>
    public class EncodingPatternDictionary<T> where T : class
    {
        private readonly Dictionary<int, EncodingPattern<T>> patterns = new();

        /// <summary>
        /// Adds a color/pattern pair, or replaces the pattern if the color already exists.
        /// </summary>
        /// <returns>This instance (fluent).</returns>
        internal EncodingPatternDictionary<T> AddPattern(int rgb, T? pattern)
        {
            if (!patterns.ContainsKey(rgb))
            {
                patterns.Add(rgb, new EncodingPattern<T>
                {
                    RGB = rgb,
                    Pattern = pattern
                });
            }
            else
            {
                patterns[rgb].Pattern = pattern;
            }

            return this;
        }

        /// <summary>
        /// Adds a color without pattern.
        /// </summary>
        internal EncodingPatternDictionary<T> AddPattern(int rgb)
            => AddPattern(rgb, null);

        /// <summary>
        /// Removes all entries.
        /// </summary>
        internal EncodingPatternDictionary<T> Reset()
        {
            patterns.Clear();
            return this;
        }

        /// <summary>
        /// Returns the pattern registered for <paramref name="rgb"/>, or null when the color is unknown or has no pattern.
        /// </summary>
        internal T? GetPattern(int rgb)
            => patterns.TryGetValue(rgb, out var pattern) ? pattern.Pattern : null;

        /// <summary>
        /// Returns the palette of all registered colors.
        /// </summary>
        internal Palette GetPalette()
        {
            var palette = new Palette().Create();

            foreach (var pattern in patterns.Values)
            {
                palette.Add(pattern.RGB);
            }

            return palette;
        }
    }
}