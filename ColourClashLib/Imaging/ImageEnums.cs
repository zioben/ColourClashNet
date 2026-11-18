using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging
{
    /// <summary>
    /// Standard image file formats.
    /// </summary>
    public enum ImageSaveFormat
    {
        Unknown = 0,
        Bmp,
        Png,
        Jpg,
    }

    /// <summary>
    /// Export formats, including raw Amiga bitplane data.
    /// </summary>
    public enum DataExportMode
    {
        Unknown = 0,
        //
        Bmp,
        Png,
        CbmAmigaRawBitplane,
        CbmAmigaRawBitplaneInterleaved,
        CbmAmigaRawBitplaneCopperlist,
        CbmAmigaRawBitplaneInterleavedCopperlist,
    }

    /// <summary>
    /// Alignment of the image width (in pixels) required by the target hardware or file format.
    /// The enum value is the alignment multiple.
    /// </summary>
    public enum WidthAlignMode
    {
        Multiple001 = 1,
        Multiple002 = 2,
        Multiple004 = 4,
        Multiple008 = 8,
        Multiple016 = 16,
        Multiple032 = 32,
        Multiple064 = 64,
        Multiple128 = 128,
        Multiple256 = 256,
    }


    /// <summary>
    /// How the tile grid is highlighted on screen.
    /// </summary>
    public enum TileMode
    {
        None,
        CornerSpot,
        Box,
        BoundingBox,
        CheckerHighlight
    }

    /// <summary>
    /// Overlay drawing mode.
    /// </summary>
    public enum OverlayMode
    {
        Draw,
        TileMode,
        CookieCut,
    }
}
