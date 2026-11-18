using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Imaging
{
    public static partial class ImageTools
    {
        /// <summary>
        /// Returns the number of bitplanes needed to index <paramref name="paletteEntries"/> colors (1..8),
        /// or 0 when the palette is empty or larger than 256 colors.
        /// </summary>
        static int BitplaneGetPlanes(int paletteEntries)
        {
            if (paletteEntries > 256)
                return 0;
            if (paletteEntries > 128)
                return 8;
            if (paletteEntries > 64)
                return 7;
            if (paletteEntries > 32)
                return 6;
            if (paletteEntries > 16)
                return 5;
            if (paletteEntries > 8)
                return 4;
            if (paletteEntries > 4)
                return 3;
            if (paletteEntries > 2)
                return 2;
            if (paletteEntries > 1)
                return 1;
            return 0;
        }

        /// <summary>
        /// Converts an indexed (chunky) image into Amiga planar bitplane data (C2P conversion).
        /// <para/>
        /// Each output row holds one bitplane line, 8 pixels per byte. With <paramref name="interleavedData"/>
        /// the planes of each image line are consecutive (ILBM style); otherwise all lines of plane 0
        /// come first, then plane 1, and so on.
        /// </summary>
        /// <param name="indexMatrix">Palette index per pixel; width must be a multiple of 8</param>
        /// <param name="interleavedData">True for interleaved, false for sequential planes</param>
        /// <param name="maxColors">Number of palette entries, used to compute the plane count</param>
        /// <returns>Bitplane matrix [rows * planes, columns / 8], or null if <paramref name="indexMatrix"/> is null.</returns>
        static public byte[,] BitplaneCreateData(byte[,] indexMatrix, bool interleavedData, int maxColors)
        {
            if (indexMatrix == null)
                return null;

            var rows = indexMatrix.GetLength(0);
            var columns = indexMatrix.GetLength(1);
            var bytes = columns / 8;
            var planes = BitplaneGetPlanes(maxColors);// linePalettes.Max(x => x.Count));
            var bitplaneData = new byte[rows * planes, bytes];

            byte[] chunky = new byte[8];
            byte[] planar = new byte[8];
            //----------------------------
            // C2P conversion
            //----------------------------
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0, w = 0; c < columns; c += 8, w++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        chunky[k] = indexMatrix[r, c + k];
                        //a7a6a5a4a3a2a1a0-b7b6b5b4b3b2b1b0-.....-g7g6g5g4g3g2g1g0
                        //a0b0..........g0-a1b1..........g1-.....-a7b7..........g7
                    }
                    // Transpose the 8x8 bit block: bit k of the 8 pixel indexes becomes byte k (plane k)
                    for (int k = 0; k < 8; k++)
                    {
                        int planeByte = 0;
                        for (int j = 0; j < 8; j++)
                            planeByte |= (chunky[j] & 0x01) << (7 - j);   // leftmost pixel -> MSB
                        planar[k] = (byte)planeByte;
                        for (int j = 0; j < 8; j++)
                            chunky[j] >>= 1;
                    }
                    if (interleavedData)
                    {
                        for (int k = 0; k < planes; k++)
                        {
                            bitplaneData[r * planes + k, w] = planar[k];
                        }
                    }
                    else
                    {
                        for (int k = 0; k < planes; k++)
                        {
                            bitplaneData[r + k * rows, w] = planar[k];
                        }
                    }
                }
            }
            return bitplaneData;
        }

        //static public byte[,] BitplaneCreateData(int[,] sourceMatrix, List<List<int>> linePalettes, ImageWidthAlignMode pixelWidthAlign, bool interleavedData)
        //{
        //    var indexMatrix = CreateIndexedData(sourceMatrix, linePalettes, pixelWidthAlign);
        //    return BitplaneCreateData(indexMatrix, interleavedData, linePalettes.Max(x => x.Count));
          
        //}

        /// <summary>
        /// Indexes a color matrix with <paramref name="paletteSrc"/> and converts it to bitplane data.
        /// </summary>
        static public byte[,] BitplaneCreateData(int[,] sourceMatrix, List<int> paletteSrc, WidthAlignMode pixelWidthAlign, bool interleavedData)
        {
            var indexMatrix = MatrixTools.CreateIndexedMatrix(sourceMatrix, paletteSrc, pixelWidthAlign);
            return BitplaneCreateData(indexMatrix, interleavedData, paletteSrc.Count);
        }

        //static public Bitmap BitplaneCreateIndexedBitmap(int[,] sourceMatrix, List<List<int>> linePalettes, ImageWidthAlignMode pixelWidthAlign, bool interleavedData)
        //{
        //    var bitplaneData = BitplaneCreateData(sourceMatrix, linePalettes, pixelWidthAlign, interleavedData);
        //    var bmp = CreateIndexedBitmap(bitplaneData);
        //    BitmapSetPalette(bmp, linePalettes);
        //    return bmp;
        //}

        //static public Bitmap BitplaneCreateIndexedBitmap(int[,] sourceMatrix, List<int> paletteSrc, ImageWidthAlignMode pixelWidthAlign, bool interleavedData)
        //{
        //    var bitplaneData = BitplaneCreateData(sourceMatrix, paletteSrc, pixelWidthAlign, interleavedData);
        //    var bmp = CreateIndexedBitmap(bitplaneData);
        //    BitmapSetPalette(bmp, paletteSrc);
        //    return bmp;
        //}


        /// <summary>
        /// Writes raw bitplane data to a <c>.bpl</c> file. Size, plane count and layout are encoded in the file name.
        /// </summary>
        /// <param name="file">File name prefix</param>
        /// <param name="indexMatrix">Bitplane data</param>
        /// <param name="maxColors">Number of palette entries</param>
        /// <param name="interleavedData">Layout tag written in the file name ("il" or "pr")</param>
        public static void BitplaneWriteFileBpl(string file, byte[,] indexMatrix, int maxColors, bool interleavedData)
        {
            if (indexMatrix == null)
                return;
            var rows = indexMatrix.GetLength(0);
            var columns = indexMatrix.GetLength(1);
            var halfColumns = columns / 2;
            var planes = BitplaneGetPlanes(maxColors);
            var layoutTag = interleavedData ? "il" : "pr";
            var fileName = $"{file}_r{rows/4:D4}_w{halfColumns:D4}_p{planes}_{layoutTag}.planeByte";

            using (var dataFile = File.OpenWrite(fileName))
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < columns; c++)
                    {
                        dataFile.WriteByte(indexMatrix[r, c]);
                    }
                }
            }
        }

        /// <summary>
        /// Writes the palettes in two raw formats: <c>.rgb24</c> (3 bytes per color) and
        /// <c>.rgb12</c> (Amiga OCS 0x0RGB word, 2 bytes per color).
        /// </summary>
        /// <param name="file">File name prefix</param>
        /// <param name="linePalettes">One palette per line (a single palette for static images)</param>
        public static void BitplaneWriteFilePalette(string file, List<List<int>> linePalettes)
        {
            if (linePalettes == null || linePalettes.Count == 0)
                return;
            var fileName = $"{file}_{linePalettes.Count}_{linePalettes.Max(x=>x.Count)}";
            using (var rgb24File = File.OpenWrite(fileName + ".rgb24"))
            {
                linePalettes.ForEach(linePalette=>
                    {
                        linePalette.ForEach(color =>
                        {
                            rgb24File.WriteByte((byte)color.ToR());
                            rgb24File.WriteByte((byte)color.ToG());
                            rgb24File.WriteByte((byte)color.ToB());
                        });
                });
            }
            using (var rgb12File = File.OpenWrite(fileName + ".rgb12"))
            {
                linePalettes.ForEach(linePalette =>
                {
                    linePalette.ForEach(color =>
                    {
                        var r = (byte)color.ToR();
                        var g = (byte)color.ToG();
                        var b = (byte)color.ToB();
                        rgb12File.WriteByte((byte)(r>>4));
                        rgb12File.WriteByte((byte)((g & 0xF0) | (b>>4)));
                    });
                });
            }
        }

        //public static void BitplaneWriteFile(string file, int[,] sourceMatrix, List<List<int>> linePalettes, ImageWidthAlignMode pixelWidthAlign, bool interleavedData)
        //{
        //    var indexMatrix = BitplaneCreateData( sourceMatrix,linePalettes,pixelWidthAlign,interleavedData);
        //    BitplaneWriteFileBpl(file, indexMatrix, linePalettes.Max(x => x.Count),interleavedData);
        //    BitplaneWriteFilePalette(file, linePalettes);
        //}
        /// <summary>
        /// Converts an image to bitplanes and writes both the bitplane file and the palette files.
        /// </summary>
        public static void BitplaneWriteFile(string file, int[,] sourceMatrix, List<int> paletteSrc, WidthAlignMode pixelWidthAlign, bool interleavedData)
        {
            var indexMatrix = BitplaneCreateData(sourceMatrix, paletteSrc, pixelWidthAlign, interleavedData);
            BitplaneWriteFileBpl(file, indexMatrix, paletteSrc.Count, interleavedData);
            BitplaneWriteFilePalette(file, new List<List<int>>() { paletteSrc });
        }

    }
}
