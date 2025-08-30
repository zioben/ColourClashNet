using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging
{
    public enum ImageExportFormat
    {
        Unknown = 0,
        //
        Bmp24,
        BmpIndex,
        Png24,
        PngIndex,
        //
        CbmAmigaRawBitplane,
        CbmAmigaRawBitplaneInterleaved,
        CbmAmigaRawBitplaneCopperlist,
        CbmAmigaRawBitplaneInterleavedCopperlist,
    }

    public enum ImageWidthAlignMode
    {
        None = 0,
        MultiplePixel16 = 16,
        MultiplePixel32 = 32,
        MultiplePixel64 = 64
    }
}
