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
        Bmp,
        BmpIndexed,
        Png,
        PngIndexed,
        Jpg,
        //
        CbmAmigaRawBitplane,
        CbmAmigaRawBitplaneInterleaved,
        CbmAmigaRawBitplaneCopperlist,
        CbmAmigaRawBitplaneInterleavedCopperlist,
    }

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

}
