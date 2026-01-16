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
        /// Number of distinct colors in the palette.
        /// </summary>
        public int Count => rgbPalette.Count;

        /// <summary>
        /// Adds a valid RGB color to the palette.
        /// </summary>
        public void Add(int iRGB)
        {
            if (iRGB.IsColor())
            {
                rgbPalette.Add(iRGB);
            }
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
        /// 
        /// </summary>
        /// <param name="rgbData"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="rgbData"></param>
        /// <returns></returns>
        Palette Create( int[,] oData)
            => Create(oData?.Cast<int>());


        /// <summary>
        /// 
        /// </summary>
        /// <param name="rgbData"></param>
        /// <returns></returns>
        Palette Create(ImageData oImageData)
            => Create(oImageData?.Data);

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