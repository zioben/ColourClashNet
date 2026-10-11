
namespace ColourClashNet.Color.Transformation
{

    public class EncodingPattern<T> where T : class
    {
        internal int RGB { get; set; }
        internal T? Pattern { get; set; }
    }

    public class EncodingPatternDictionary<T> where T : class
    {
        private readonly Dictionary<int, EncodingPattern<T>> patterns = new();

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

        internal EncodingPatternDictionary<T> AddPattern(int rgb)
            => AddPattern(rgb, null);

        internal EncodingPatternDictionary<T> Reset()
        {
            patterns.Clear();
            return this;
        }

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