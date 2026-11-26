using ColourClashProcessing.Threshold;
using ColourClashNet;
using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashProcessing.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace XUnitTester
{
    /// <summary>
    /// Regression tests for bugs found during code review
    /// </summary>
    public class TestReviewFixes
    {
        static ImageData Solid(int width, int height, int rgb)
            => new ImageData().Create(MatrixTools.CreateMatrix(width, height, rgb));

        static ImageData Random(int width, int height, int seed = 42)
        {
            var rnd = new Random(seed);
            var m = new int[height, width];
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    m[r, c] = ColorIntExt.FromRGB(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            return new ImageData().Create(m);
        }

        static HashSet<int> Colors(ImageData image)
        {
            var set = new HashSet<int>();
            foreach (var rgb in image.GetMatrix())
                set.Add(rgb);
            return set;
        }

        static readonly int[] c64Palette =
        {
            0x000000, 0xFFFFFF, 0x894036, 0x7ABFC7, 0x8A46AE, 0x68A941, 0x3E31A2, 0xD0DC71,
            0x905F25, 0x5C4700, 0xBB776D, 0x555555, 0x808080, 0xACEA88, 0x7C70DA, 0xABABAB,
        };

        // ---------------------------------------------------------------- C64

        [Fact]
        public void C64_HiRes_OutputUsesOnlyC64Colors_TwoPerCell()
        {
            var t = new ColorTransformCbm64 { Options = new C64Options { VideoMode = ColorTransformCbm64.C64VideoMode.HiRes } };

            var res = t.Process(Random(16, 16));

            Assert.True(res.IsSuccess, res.Message);
            var m = t.ImageOutput.GetMatrix();
            Assert.All(Colors(t.ImageOutput), rgb => Assert.Contains(rgb, c64Palette));
            for (int tr = 0; tr < 2; tr++)
                for (int tc = 0; tc < 2; tc++)
                {
                    var cell = new HashSet<int>();
                    for (int r = 0; r < 8; r++)
                        for (int c = 0; c < 8; c++)
                            cell.Add(m[tr * 8 + r, tc * 8 + c]);
                    Assert.True(cell.Count <= 2);
                }
        }

        [Fact]
        public void C64_LightBlue_IsInPalette()
        {
            var t = new ColorTransformCbm64 { Options = new C64Options { VideoMode = ColorTransformCbm64.C64VideoMode.HiRes } };

            var res = t.Process(Solid(8, 8, 0x7C70DA));

            Assert.True(res.IsSuccess, res.Message);
            Assert.Equal(0x7C70DA, t.ImageOutput.GetMatrix()[0, 0]);
        }

        // ---------------------------------------------------------------- Tiles

        [Fact]
        public void TileManager_PartialEdgeTiles_AreProcessed()
        {
            var t = new ColorTransformCbm64 { Options = new C64Options { VideoMode = ColorTransformCbm64.C64VideoMode.HiRes } };

            var res = t.Process(Solid(12, 12, 0xFFFFFF));

            Assert.True(res.IsSuccess, res.Message);
            var m = t.ImageOutput.GetMatrix();
            Assert.Equal(0xFFFFFF, m[11, 0]);
            Assert.Equal(0xFFFFFF, m[0, 11]);
            Assert.Equal(0xFFFFFF, m[11, 11]);
        }

        // ---------------------------------------------------------------- Fixed palette enhanced

        [Fact]
        public void FixedPaletteEnhanced_UsesUserPaletteMeans()
        {
            var reference = new List<int> { 0x000000, 0xFF0000, 0x00FF00, 0x0000FF };
            var allowed = new HashSet<int>(reference);
            for (int i = 0; i < reference.Count; i++)
                for (int j = i + 1; j < reference.Count; j++)
                    allowed.Add(ColorIntExt.GetColorMean(reference[i], reference[j]));

            // Fewer image colors than reference colors used to throw ArgumentOutOfRangeException
            var twoColors = new ImageData().Create(new int[,] { { 0x000000, 0xFFFFFF } });
            var t1 = new ColorTransformFixedPaletteEnhanced { Options = new EnhancedPaletteOptions { UserPalette = reference } };
            Assert.True(t1.Process(twoColors).IsSuccess);

            var t2 = new ColorTransformFixedPaletteEnhanced { Options = new EnhancedPaletteOptions { UserPalette = reference } };
            var res = t2.Process(Random(16, 16));
            Assert.True(res.IsSuccess, res.Message);
            Assert.All(Colors(t2.ImageOutput), rgb => Assert.Contains(rgb, allowed));
        }

        // ---------------------------------------------------------------- Median cut

        [Fact]
        public void MedianCut_WithTies_DoesNotCollapseToOneColor()
        {
            var image = new ImageData().Create(new int[,] { { 0x000000, 0x0000FF, 0xFF0000, 0xFFFFFF, 0x00FF00 } });
            var t = new ColorTransformMedianCut { Options = new MedianCutOptions { MaxColors = 4 } };

            var res = t.Process(image);

            Assert.True(res.IsSuccess, res.Message);
            Assert.True(Colors(t.ImageOutput).Count >= 3);
        }

        [Fact]
        public void MedianCut_ExactColorCount_IsKept()
        {
            var image = new ImageData().Create(new int[,] { { 0x000000, 0x0000FF, 0xFF0000, 0xFFFFFF } });
            var t = new ColorTransformMedianCut { Options = new MedianCutOptions { MaxColors = 4 } };

            t.Process(image);

            Assert.Equal(4, Colors(t.ImageOutput).Count);
        }

        [Fact]
        public void MedianCut_Options_AreStoredAsGiven()
        {
            var options = new MedianCutOptions { MaxColors = 8, Selection = ColorSelectionMode.UseColorPalette };
            var t = new ColorTransformMedianCut { Options = options };
            Assert.Same(options, t.Options);
            Assert.Equal(ColorSelectionMode.UseColorPalette, t.Options.Selection);
        }

        // ---------------------------------------------------------------- Amiga HAM

        [Fact]
        public void AmigaHam6_FirstColumn_IsNotBlack()
        {
            var t = new ColorTransformCbmAmiga { Options = new AmigaOptions { VideoMode = ColorTransformCbmAmiga.EnumAmigaVideoMode.Ham6 } };

            var res = t.Process(Solid(8, 4, 0xFFFFFF));

            Assert.True(res.IsSuccess, res.Message);
            var m = t.ImageOutput.GetMatrix();
            for (int r = 0; r < 4; r++)
                Assert.Equal(0xFFFFFF, m[r, 0]);
        }

        // ---------------------------------------------------------------- CGA / EGA / CPC / LumSat

        [Fact]
        public void CGA_ModeChange_OnSameInstance_IsApplied()
        {
            var image = Random(8, 8);
            var t = new ColorTransformCGA();

            t.Options = new CgaOptions { VideoMode = ColorTransformCGA.CGAVideoMode.Mode6 };
            Assert.True(t.Process(image).IsSuccess);
            var palMode6 = t.TargetPalette.ToList().OrderBy(x => x).ToList();

            t.Options = t.Options with { VideoMode = ColorTransformCGA.CGAVideoMode.Mode4_0L };
            Assert.True(t.Process(image).IsSuccess);
            var palMode4 = t.TargetPalette.ToList().OrderBy(x => x).ToList();

            Assert.NotEqual(palMode6, palMode4);
        }

        [Fact]
        public void CGA_DebugPalette_Succeeds()
        {
            var t = new ColorTransformCGA { Options = new CgaOptions { VideoMode = ColorTransformCGA.CGAVideoMode.DebugPalette } };

            Assert.True(t.Process(Random(8, 8)).IsSuccess);
        }

        [Fact]
        public void EGA_LightCyan_IsInPalette()
        {
            var t = new ColorTransformEGA();

            t.Process(Solid(4, 4, 0x55FFFF));

            Assert.Equal(16, t.TargetPalette.Count);
            Assert.Equal(0x55FFFF, t.ImageOutput.GetMatrix()[0, 0]);
        }

        [Fact]
        public void TransformTypes_AreCorrect()
        {
            Assert.Equal(ColorTransformType.ColorReductionCPC, new ColorTransformAmstradCPC().Type);
            Assert.Equal(ColorTransformType.ColorReductionSaturation, new ColorTransformLumSat().Type);
        }

        [Fact]
        public void LumSat_NegativeHueShift_WrapsHue()
        {
            var t = new ColorTransformLumSat { Options = new HsvOptions { HueShift = -60 } };

            var res = t.Process(Solid(2, 2, 0xFF0000));

            Assert.True(res.IsSuccess, res.Message);
            Assert.Equal(0xFF00FF, t.ImageOutput.GetMatrix()[0, 0]);
        }

        // ---------------------------------------------------------------- Quantization / map

        [Fact]
        public void QuantizationRGB333_White_StaysWhite()
        {
            var t = new ColorTransformQuantization { Options = new QuantizationOptions { Mode = ColorQuantizationMode.RGB333 } };

            t.Process(Solid(2, 2, 0xFFFFFF));

            Assert.Equal(0xFFFFFF, t.ImageOutput.GetMatrix()[0, 0]);
        }

        [Fact]
        public void TransformationMap_KeepsTaggedPixels()
        {
            int tagged = ColorIntExt.FromRGB(10, 10, 10, ColorInfo.IsTransparent);
            var image = new ImageData().Create(new int[,] { { tagged, 0x102030 } });
            var t = new ColorTransformFixedPalette { Options = new FixedPaletteOptions { UserPalette = new List<int> { 0x000000, 0xFFFFFF } } };

            t.Process(image);

            Assert.Equal(tagged, t.ImageOutput.GetMatrix()[0, 0]);
        }

        // ---------------------------------------------------------------- Ordered dithering

        [Theory]
        [InlineData(ColorDithering.Ordered_6x6, ColorDithering.Ordered_6x6)]
        [InlineData(ColorDithering.Ordered_8x8, ColorDithering.Ordered_8x8)]
        public void OrderedDither_Size_MatchesRequested(ColorDithering requested, ColorDithering expected)
        {
            var dither = DitherBase.CreateDitherInterface(new DitherOptions { DitheringType = requested }) as DitherOrdered;
            Assert.NotNull(dither);
            Assert.Equal(expected, dither.Type);
        }

        [Fact]
        public void OrderedDither2x2_MidGray_IsHalfWhite()
        {
            int width = 8, height = 8;
            var reference = Solid(width, height, 0x808080);
            // Processed image only provides the output palette (black and white)
            var pm = new int[height, width];
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    pm[r, c] = (r + c) % 2 == 0 ? 0x000000 : 0xFFFFFF;
            var processed = new ImageData().Create(pm);

            var dither = DitherBase.CreateDitherInterface(new DitherOptions { DitheringType = ColorDithering.Ordered_2x2, DitheringStrength = 1.0 });
            var res = dither.Dither(reference, processed, ColorDistanceEvaluationMode.RGB);

            Assert.True(res.IsSuccess, res.Message);
            int whites = res.DataOut.GetMatrix().Cast<int>().Count(x => x == 0xFFFFFF);
            Assert.Equal(width * height / 2, whites);
        }

        // ---------------------------------------------------------------- Blob

        [Fact]
        public void Blob_SizeFilters_KeepBlobsWithinTheLimits()
        {
            int colBkg = ColorIntExt.FromRGB(255, 0, 255);
            int colFgd = ColorIntExt.FromRGB(128, 128, 128);
            var m = MatrixTools.CreateMatrix(20, 20, colBkg);
            MatrixTools.Blit(MatrixTools.CreateMatrix(3, 3, colFgd), m, 0, 0, 5, 5, 3, 3);

            var blob = new Blob
            {
                Options = new BlobOptions
                {
                    Threshold = new ThresholdOptions { BackgroundColors = new[] { colBkg } },
                    MaxArea = 1000000,
                    MaxWidth = 100,
                    MaxHeight = 100,
                    ConnectionMode = BlobConnectionModes.FourConnected,
                },
            };
            blob.Process(new ImageData().Create(m));

            Assert.Single(blob.BlobList);

            // A minimum width above the blob width filters it out
            blob.Options = blob.Options with { MinWidth = 4 };
            blob.Process(new ImageData().Create(m));
            Assert.Empty(blob.BlobList);
        }

        [Fact]
        public void Blob_InvalidOptions_ReturnAnErrorResult()
        {
            var image = new ImageData().Create(MatrixTools.CreateMatrix(8, 8, 0));
            var blob = new Blob { Options = new BlobOptions { MinArea = 50, MaxArea = 10, Threshold = new ThresholdOptions { BackgroundColors = Array.Empty<int>() } } };

            var result = blob.Process(image);

            Assert.False(result.IsSuccess);
            Assert.Contains("MinArea (50) is greater than MaxArea (10)", result.Message);
            Assert.Contains("Threshold : BackgroundColors", result.Message);
        }

        [Fact]
        public void BlobOptions_With_LeavesTheOriginalUnchanged()
        {
            var original = new BlobOptions { MinArea = 5 };
            var changed = original with { MinArea = 9, Threshold = original.Threshold with { Tolerance = 3 } };

            Assert.Equal(5, original.MinArea);
            Assert.Equal(0, original.Threshold.Tolerance);
            Assert.Equal(9, changed.MinArea);
            Assert.Equal(3, changed.Threshold.Tolerance);
        }

        // ---------------------------------------------------------------- MatrixTools

        [Fact]
        public void MatrixTools_Clear_UsesXAsColumn()
        {
            var m = MatrixTools.CreateMatrix(4, 4, 0);

            MatrixTools.Clear(m, 2, 0, 2, 1, 7);

            Assert.Equal(7, m[0, 2]);
            Assert.Equal(7, m[0, 3]);
            Assert.Equal(0, m[2, 0]);
        }

        [Fact]
        public void MatrixTools_HalveColumns_MeanColor_AveragesPairsFromColumnZero()
        {
            var m = new int[,] { { 0x000000, 0x0000FE, 0x00FE00, 0xFE0000 } };

            var h = MatrixTools.HalveMatrixColumns(m, HalveResolutionMode.MeanColor);

            Assert.Equal(0x00007F, h[0, 0]);
            Assert.Equal(0x7F7F00, h[0, 1]);
        }

        [Fact]
        public void MatrixTools_HalveColumns_EvenPixel_OddWidth_FillsLastColumn()
        {
            var m = new int[,] { { 1, 2, 3, 4, 5 } };

            var h = MatrixTools.HalveMatrixColumns(m, HalveResolutionMode.EvenPixel);

            Assert.Equal(new[] { 2, 4, 5 }, h.Cast<int>().ToArray());
        }

        [Fact]
        public void MatrixTools_CreateSafeMatrix_MapsTransparentToBackground()
        {
            int transparent = ColorIntExt.FromRGB(1, 2, 3, ColorInfo.IsTransparent);

            var safe = MatrixTools.CreateSafeMatrix(new int[,] { { transparent } });

            Assert.Equal(ColorDefaults.DefaultBkgColorInt, safe[0, 0]);
        }
    }
}
