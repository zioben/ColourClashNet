using ColourClashNet.Color;
using ColourClashNet.Color;

namespace ColourClashNet.Color
{
    /// <summary>
    /// Represents a collection of distinct RGB colors.
    /// Only valid colors (ColorIntType.IsColor) are stored.
    /// </summary>
    public partial class Palette
    {
        /// <summary>
        /// Internal storage for unique colors.
        /// </summary>
        protected HashSet<int> rgbPalette { get; private init; } = new HashSet<int>();

        /// <summary>
        /// Number of distinct colors in the palette.
        /// </summary>
        public int Count => rgbPalette.Count;

        /// <summary>
        /// Adds a valid RGB color to the palette.
        /// </summary>
        public void Add(int iRGB)
        {
            if (iRGB.GetColorInfo() == ColorInfo.IsColor)
                rgbPalette.Add(iRGB);
        }

        /// <summary>
        /// Removes a color from the palette if it exists.
        /// </summary>
        public void Remove(int iRGB) => rgbPalette.Remove(iRGB);

        /// <summary>
        /// Clears all colors from the palette.
        /// </summary>
        public void Reset() => rgbPalette.Clear();

        /// <summary>
        /// Converts the palette to a List of integer colors.
        /// </summary>
        public List<int> ToList() => rgbPalette.ToList();

        /// <summary>
        /// Returns a string describing the palette.
        /// </summary>
        public override string ToString() => $"Palette Colors: {Count}";
    }
}