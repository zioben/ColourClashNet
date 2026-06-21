using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging
{
    public enum ImageSaveFormat
    {
        Unknown = 0,
        //
        Bmp,
       // BmpIndexed,
        Png,
       // PngIndexed,
        Jpg,
        //
     //   CbmAmigaRawBitplane,
     //   CbmAmigaRawBitplaneInterleaved,
     //   CbmAmigaRawBitplaneCopperlist,
     //   CbmAmigaRawBitplaneInterleavedCopperlist,
    }
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
