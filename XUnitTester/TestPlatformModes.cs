using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using Xunit;

namespace XUnitTester
{
    /// <summary>
    /// Tests of the HSV hue rotation, the CGA composite modes and the Amiga HAM encoding.
    /// </summary>
    public class TestPlatformModes
    {
        // ---------------------------------------------------------------- Helpers

        static ImageData Solid(int width, int height, int rgb)
            => new ImageData().Create(MatrixTools.CreateMatrix(width, height, rgb));

        /// <summary>
        /// Smooth color waves with sharp inverted bands: many colors and many edges.
        /// </summary>
        static int[,] Waves(int width, int height)
        {
            var matrix = new int[height, width];
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                {
                    double x = c / (double)width, y = r / (double)height;
                    int red = (int)(127 + 127 * Math.Sin(x * 23 + y * 3));
                    int green = (int)(127 + 127 * Math.Sin(x * 7 - y * 11 + 1));
                    int blue = (int)(127 + 127 * Math.Sin(x * 41 + 2));
                    if (((c / 9) + (r / 7)) % 5 == 0)
                    {
                        red = 255 - red;
                        blue = 255 - blue;
                    }
                    matrix[r, c] = ColorIntExt.FromRGB(red, green, blue);
                }
            return matrix;
        }

        // ---------------------------------------------------------------- HSV

        [Theory]
        [InlineData(0, 0)]
        [InlineData(360, 0)]
        [InlineData(390, 30)]
        [InlineData(-90, 270)]
        [InlineData(-360, 0)]
        [InlineData(359.99999999, 0)]
        public void WrapHue_KeepsTheHueOnTheColorWheel(double hue, double expected)
        {
            Assert.Equal(expected, ColorTransformLumSat.WrapHue(hue), 6);
        }

        [Theory]
        [InlineData(120, 0x00FF00)]
        [InlineData(480, 0x00FF00)]
        [InlineData(-240, 0x00FF00)]
        [InlineData(720, 0xFF0000)]
        public void HueShift_RotatesTheColorWheel(double shift, int expected)
        {
            var transform = new ColorTransformLumSat { Options = new HsvOptions { HueShift = shift } };

            var result = transform.Process(Solid(2, 2, 0xFF0000));

            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal(expected, result.DataOut.GetMatrix()[0, 0]);
        }

        [Fact]
        public void HueShift_KeepsNonColorPixels()
        {
            int mask = 0x123456.SetColorInfo(ColorInfo.IsMask);
            var matrix = new int[1, 2] { { 0xFF0000, mask } };
            var transform = new ColorTransformLumSat { Options = new HsvOptions { HueShift = 120 } };

            var result = transform.Process(new ImageData().Create(matrix));

            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal(0x00FF00, result.DataOut.GetMatrix()[0, 0]);
            Assert.Equal(mask, result.DataOut.GetMatrix()[0, 1]);
        }

        // ---------------------------------------------------------------- CGA

        [Theory]
        [InlineData(ColorTransformCGA.CGAVideoMode.Composite_0)]
        [InlineData(ColorTransformCGA.CGAVideoMode.Composite_1)]
        public void CgaComposite_WorksAtHalfResolution(ColorTransformCGA.CGAVideoMode mode)
        {
            var transform = new ColorTransformCGA { Options = new CgaOptions { VideoMode = mode } };
            var source = new ImageData().Create(Waves(64, 8));

            var result = transform.Process(source);

            Assert.True(result.IsSuccess, result.Message);
            var output = result.DataOut.GetMatrix();
            Assert.Equal(64, output.GetLength(1));
            var palette = transform.TargetPalette.ToList();
            for (int r = 0; r < output.GetLength(0); r++)
                for (int c = 0; c < output.GetLength(1); c += 2)
                {
                    // Each artifact color covers a pixel pair
                    Assert.Equal(output[r, c], output[r, c + 1]);
                    Assert.Contains(output[r, c], palette);
                }
        }

        [Fact]
        public void CgaCompositeRaw_UsesTheMode4PixelPairs()
        {
            // Palette 1, high intensity: black, light cyan, light magenta, white
            int[] mode4 = { 0x000000, 0x55FFFF, 0xFF55FF, 0xFFFFFF };
            var options = new CgaOptions
            {
                VideoMode = ColorTransformCGA.CGAVideoMode.Composite_1,
                OutputMode = ColorTransformCGA.CGACompositeOutputMode.RawImage,
            };
            var transform = new ColorTransformCGA { Options = options };

            var result = transform.Process(new ImageData().Create(Waves(64, 8)));

            Assert.True(result.IsSuccess, result.Message);
            var raw = result.DataOut.GetMatrix();
            Assert.Equal(64, raw.GetLength(1));
            foreach (var rgb in raw)
                Assert.Contains(rgb, mode4);

            // White is generated by two white pixels
            var white = new ColorTransformCGA { Options = options }.Process(Solid(4, 1, 0xFFFFFF));
            Assert.Equal(new[] { 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF }, white.DataOut.GetMatrix().Cast<int>());
        }

        [Theory]
        [InlineData(ColorTransformType.ColorReductionCGA)]
        [InlineData(ColorTransformType.ColorReductionCPC)]
        [InlineData(ColorTransformType.ColorReductionCBM64)]
        public void HalfResolutionModes_KeepAnOddWidth(ColorTransformType type)
        {
            TransformOptions options = type == ColorTransformType.ColorReductionCGA
                ? new CgaOptions { VideoMode = ColorTransformCGA.CGAVideoMode.Composite_0 }
                : TransformOptions.CreateDefault(type);
            var transform = ColorTransformBase.CreateTransform(options);

            var result = transform.Process(new ImageData().Create(Waves(17, 8)));

            Assert.True(result.IsSuccess, result.Message);
            Assert.Equal(17, result.DataOut.Columns);
            Assert.Equal(8, result.DataOut.Rows);
        }

        [Fact]
        public void CgaPaletteModeRaw_IsTheProcessedImage()
        {
            var source = new ImageData().Create(Waves(32, 4));
            var processed = new ColorTransformCGA { Options = new CgaOptions { VideoMode = ColorTransformCGA.CGAVideoMode.Mode4_1H } }
                .Process(source);
            var raw = new ColorTransformCGA
            {
                Options = new CgaOptions { VideoMode = ColorTransformCGA.CGAVideoMode.Mode4_1H, OutputMode = ColorTransformCGA.CGACompositeOutputMode.RawImage },
            }.Process(source);

            Assert.True(raw.IsSuccess, raw.Message);
            Assert.Equal(processed.DataOut.GetMatrix().Cast<int>(), raw.DataOut.GetMatrix().Cast<int>());
        }

        // ---------------------------------------------------------------- Amiga HAM

        static double HamError(int[,] source, int lookahead, out int[,] output)
        {
            var transform = new ColorTransformCbmAmiga { Options = new AmigaOptions { HamLookahead = lookahead } };
            var result = transform.Process(new ImageData().Create(source));
            Assert.True(result.IsSuccess, result.Message);
            output = result.DataOut.GetMatrix();
            double error = 0;
            for (int r = 0; r < source.GetLength(0); r++)
                for (int c = 0; c < source.GetLength(1); c++)
                {
                    int target = ColorTransformQuantization.QuantizeColor(ColorQuantizationMode.RGB444, source[r, c]);
                    error += target.Distance(output[r, c], ColorDistanceEvaluationMode.RGB);
                }
            return error;
        }

        [Fact]
        public void Ham_Lookahead_ReducesTheError()
        {
            var source = Waves(160, 32);

            double greedy = HamError(source, 0, out _);
            double lookahead = HamError(source, 2, out _);

            Assert.True(lookahead < greedy, $"lookahead {lookahead} not below greedy {greedy}");
        }

        [Fact]
        public void Ham_EveryPixelIsABaseColorOrModifiesOneChannel()
        {
            HamError(Waves(160, 32), 2, out var output);

            // Pixels changing more than one channel must be base colors: at most 16 of them in HAM6
            var baseColors = new HashSet<int>();
            for (int r = 0; r < output.GetLength(0); r++)
            {
                baseColors.Add(output[r, 0]);
                for (int c = 1; c < output.GetLength(1); c++)
                {
                    int prev = output[r, c - 1], cur = output[r, c];
                    int changed = (prev.ToR() != cur.ToR() ? 1 : 0) + (prev.ToG() != cur.ToG() ? 1 : 0) + (prev.ToB() != cur.ToB() ? 1 : 0);
                    if (changed > 1)
                        baseColors.Add(cur);
                }
            }
            Assert.InRange(baseColors.Count, 1, 16);
        }

        [Fact]
        public void Ham_LookaheadOutOfRange_IsInvalid()
        {
            Assert.Contains(new AmigaOptions { HamLookahead = AmigaOptions.MaxHamLookahead + 1 }.Validate(), e => e.StartsWith("HamLookahead"));
            Assert.Empty(new AmigaOptions { HamLookahead = 0 }.Validate());
        }
    }
}
