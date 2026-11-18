using ColourClashNet.Color;
using ColourClashNet.Imaging;
using System.Collections;

namespace ColourClashNet.Color
{
    /// <summary>
    /// Represents a set of distinct RGB colors.
    /// <para/>
    /// Only real colors (<see cref="ColorInfo.IsColor"/>) are stored; tagged values are silently discarded.
    /// </summary>
    public partial class Palette : IEnumerable<int>
    {
        /// <summary>
        /// Internal storage for unique colors.
        /// </summary>
        protected internal HashSet<int> RgbPalette { get; private init; } = new HashSet<int>();

        /// <summary>
        /// Gets the number of colors contained in the palette.
        /// </summary>
        public int Count => RgbPalette.Count;

        /// <summary>
        /// Gets a value indicating whether the palette contains at least one color.
        /// </summary>
        public bool IsValid => Count > 0;


        /// <summary>
        /// Adds the specified RGB color value to the palette if it represents a valid color.
        /// </summary>
        /// <param name="rgb">The integer value representing an RGB color to add to the palette. Must satisfy the color validation
        /// criteria defined by IsColor().</param>
        public void Add(int rgb)
        {
            if (rgb.IsColor())
                RgbPalette.Add(rgb);
        }

        /// <summary>
        /// Removes the color with the specified RGB value from the palette.
        /// </summary>
        /// <param name="rgb">The RGB value of the color to remove from the palette.</param>
        public void Remove(int rgb) => RgbPalette.Remove(rgb);

        /// <summary>
        /// Removes all items from the palette, resetting it to an empty state.
        /// </summary>
        public void Reset() => RgbPalette.Clear();

        /// <summary>
        /// Initializes the palette as empty.
        /// </summary>
        /// <returns>This instance.</returns>
        public Palette Create()
        {
            Reset();
            return this;
        }

        /// <summary>
        /// Resets the palette and fills it with the distinct real colors of <paramref name="rgbData"/>.
        /// </summary>
        /// <param name="rgbData">Packed RGB colors; duplicates and tagged values are ignored.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rgbData"/> is null.</exception>
        public Palette Create( IEnumerable<int> rgbData ) 
        {
            Reset();
            if (rgbData == null)
                throw new ArgumentNullException(nameof(rgbData));
            foreach (var rgb in rgbData)
                Add(rgb);
            return this;
        }

        /// <summary>
        /// Fills the palette with the distinct colors of a color matrix.
        /// </summary>
        /// <param name="data">Color matrix. Cannot be null.</param>
        /// <returns>This instance.</returns>
        internal Palette Create( int[,] data)
            => Create(data?.Cast<int>());


        /// <summary>
        /// Fills the palette with the distinct colors of an image.
        /// </summary>
        /// <param name="imageData">Source image. Cannot be null.</param>
        /// <returns>This instance.</returns>
        public Palette Create(ImageData imageData)
            => Create(imageData?.matrix);

        /// <summary>
        /// Copies the colors of another palette.
        /// </summary>
        /// <param name="palette">Source palette; null produces an empty palette.</param>
        /// <returns>This instance.</returns>
        public Palette Create(Palette palette)
            => Create(palette?.RgbPalette ?? new HashSet<int>());

        /// <summary>
        /// Returns a list containing all color values in the palette.
        /// </summary>
        /// <returns>A list of integers representing the color values in the palette. The list will be empty if the palette
        /// contains no colors.</returns>
        public List<int> ToList() => RgbPalette.ToList();


        /// <summary>
        /// Returns a string that represents the current palette, including the number of colors it contains.
        /// </summary>
        /// <returns>A string in the format "Palette Colors: {Count}", where {Count} is the number of colors in the palette.</returns>
        public override string ToString()
            => $"Palette(Colors: {Count})";

        /// <inheritdoc/>
        public IEnumerator<int> GetEnumerator()
        {
            return ((IEnumerable<int>)RgbPalette).GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)RgbPalette).GetEnumerator();
        }
    }
}