using ColourClashNet.Color;
using ColourClashNet.Color;
using ColourClashNet.Imaging;
using ColourClashNet.Log;

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
       /// Gets the number of colors contained in the palette.
       /// </summary>
        public int Count => rgbPalette.Count;

        /// <summary>
        /// Gets a value indicating whether the collection contains one or more elements.
        /// </summary>
        public bool Valid => Count > 0;


        /// <summary>
        /// Adds the specified RGB color value to the palette if it represents a valid color.
        /// </summary>
        /// <param name="iRGB">The integer value representing an RGB color to add to the palette. Must satisfy the color validation
        /// criteria defined by IsColor().</param>
        public void Add(int iRGB)
        {
            if (iRGB.IsColor())
            {
                rgbPalette.Add(iRGB);
            }
        }

        /// <summary>
        /// Removes the color with the specified RGB value from the palette.
        /// </summary>
        /// <param name="iRGB">The RGB value of the color to remove from the palette.</param>
        public void Remove(int iRGB) => rgbPalette.Remove(iRGB);

        /// <summary>
        /// Removes all items from the palette, resetting it to an empty state.
        /// </summary>
        public void Reset() => rgbPalette.Clear();

        /// <summary>
        /// Creates a palette by adding colors from the specified collection of RGB values.
        /// </summary>
        /// <remarks>If the provided collection is null, the palette is reset and no colors are added. In
        /// case of an exception during processing, a new empty palette is returned instead of the current
        /// instance.</remarks>
        /// <param name="rgbData">An enumerable collection of integers representing RGB color values to add to the palette. Each integer
        /// should encode a color in 0xRRGGBB format. Cannot be null.</param>
        /// <returns>The current palette instance with the specified colors added. If an error occurs, a new empty palette is
        /// returned.</returns>
        public Palette Create( IEnumerable<int> rgbData ) 
        {
            var sM = nameof(Create);
            try
            {
                Reset();
                if (rgbData == null)
                {
                    LogMan.Error(sC, sM, "color source set null");
                    return this;
                }
                foreach (var rgb in rgbData)
                {
                   Add(rgb);
                }
                return this;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return new Palette();
            }
        }

        /// <summary>
        /// Creates a new Palette instance from a two-dimensional array of integer color data.
        /// </summary>
        /// <param name="oData">A two-dimensional array containing color data, where each element represents a color value. Cannot be null.</param>
        /// <returns>A Palette instance initialized with the specified color data.</returns>
        Palette Create( int[,] oData)
            => Create(oData?.Cast<int>());


        /// <summary>
        /// Creates a new palette from the specified image data.
        /// </summary>
        /// <param name="oImageData">The image data from which to generate the palette. Can be null, in which case the result depends on the
        /// implementation of the underlying Create method.</param>
        /// <returns>A Palette instance generated from the provided image data.</returns>
        public Palette Create(ImageData oImageData)
            => Create(oImageData?.DataX);

        /// <summary>
        /// Creates a new palette from the specified palette
        /// </summary>
        /// <param name="palette">The image data from which to generate the palette. Can be null, in which case the result depends on the
        /// implementation of the underlying Create method.</param>
        /// <returns>A Palette instance generated from the provided image data.</returns>
        public Palette Create(Palette palette)
            => Create(palette?.rgbPalette ?? new HashSet<int>());

        /// <summary>
        /// Returns a list containing all color values in the palette.
        /// </summary>
        /// <returns>A list of integers representing the color values in the palette. The list will be empty if the palette
        /// contains no colors.</returns>
        public List<int> ToList() => rgbPalette.ToList();


        /// <summary>
        /// Returns a string that represents the current palette, including the number of colors it contains.
        /// </summary>
        /// <returns>A string in the format "Palette Colors: {Count}", where {Count} is the number of colors in the palette.</returns>
        public override string ToString()
            => $"Palette(Colors: {Count})";  


    }
}