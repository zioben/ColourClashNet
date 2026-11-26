using ColourClashNet.Color.Dithering;

using ColourClashLib.Color.Transformation;
using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ModuleTester
{
    /// <summary>
    /// Manual test bench: runs every color transformation on a preset or loaded image
    /// and shows input, output, configuration and log side by side.
    /// </summary>
    public partial class FormTester : Form
    {
        Dictionary<string, Image> presetImages = new();

        public FormTester()
        {
            InitializeComponent();
            bitmapRenderIN.Control = pbIN;
            bitmapRenderOUT.Control = pbOUT;
            CreateCombos();
            LogMan.OnLogMessage += LogMan_OnLogMessage;

        }

        ConcurrentQueue<string> logBag = new ConcurrentQueue<string>();

        private void LogMan_OnLogMessage(object? sender, LogEventArgs e)
        {
            logBag.Enqueue(e.Message);
        }
        void CreateCombo(List<string> stringList, ComboBox comboBox, int? defaultIndex = null)
        {
            comboBox.Items.Clear();
            if (stringList == null)
            {
                return;
            }
            foreach (var item in stringList)
            {
                comboBox.Items.Add(item);
            }
            if (defaultIndex.HasValue && defaultIndex.Value >= 0 && defaultIndex.Value < comboBox.Items.Count)
            {
                comboBox.SelectedIndex = defaultIndex.Value;
            }
        }

        void CreateCombo(Type enumType, ComboBox comboBox, int? defaultIndex = null)
            => CreateCombo(Enum.GetNames(enumType).ToList(), comboBox, defaultIndex);

        void CreateCombos()
        {
            presetImages = new();
            presetImages.Add("1M Colors", ColourClashDrawings.Properties.Resources.BMP_1Mcolors);
            presetImages.Add("RGB Palette", ColourClashDrawings.Properties.Resources.BMP_RGB_Palette);
            presetImages.Add("FADE Base", ColourClashDrawings.Properties.Resources.BMP_FadeTest);
            presetImages.Add("GRANGER Charts", ColourClashDrawings.Properties.Resources.BMP_Granger_Chart);

            CreateCombo(presetImages.Select(x => x.Key).ToList(), cbPreset, 1);
            CreateCombo(typeof(ColorDithering), cbDithering, 9);
            CreateCombo(typeof(ColorDistanceEvaluationMode), cbColorMode, 0);
            CreateCombo(typeof(ColorQuantizationMode), cbQuantization, 4);
            CreateCombo(typeof(ColorTransformCbmAmiga.EnumAmigaVideoMode), cbAmigaMode, 0);
            CreateCombo(typeof(ColorTransformAmstradCPC.CPCVideoMode), cbCPCVideoMode, 0);
            CreateCombo(typeof(ColorTransformCbm64.C64VideoMode), cbC64VideoMode, 0);
            CreateCombo(typeof(ColorTransformCGA.CGAVideoMode), cbCgaVideoMode, 0);
            CreateCombo(typeof(ColorTransformZxSpectrum.ZxAutotuneMode), cbZXAutotuneMode, 0);
            CreateCombo(typeof(ColorTransformZxSpectrum.ZxPaletteMode), cbZXPaletteMode, 0);
        }

        void ClearOldTransform() => lastOptions = null;

        // Options of the last run: the "reprocess" button runs them again (they can be edited in the property grid)
        TransformOptions? lastOptions = null;

        /// <summary>
        /// Applies the common UI settings (distance, dithering) to <paramref name="options"/> and runs the matching
        /// transformation on the input image in background, with a progress form.
        /// </summary>
        void Process(TransformOptions options)
        {
            var dither = ColorDithering.None;
            if (!Enum.TryParse<ColorDithering>(cbDithering.SelectedItem?.ToString(), out dither))
            {
                dither = ColorDithering.None;
            }
            var distance = ColorDistanceEvaluationMode.RGB;
            if (!Enum.TryParse<ColorDistanceEvaluationMode>(cbColorMode.SelectedItem?.ToString(), out distance))
            {
                distance = ColorDistanceEvaluationMode.RGB;
            }
            Run(options with
            {
                DistanceMode = distance,
                Dithering = new DitherOptions { DitheringType = dither, DitheringStrength = (double)numDitheringStrenght.Value / 100.0 },
            });
        }

        /// <summary>
        /// Runs the transformation that matches <paramref name="options"/> on the input image in background.
        /// </summary>
        void Run(TransformOptions options)
        {
            lastOptions = options;
            bitmapRenderOUT.Image = null;
            // Deep copy: edits made in the property grid during the run do not affect it
            var transform = ColorTransformBase.CreateTransform(options.DeepClone());
            var imageData = ImageToolsGDI.GdiImageToImageData(bitmapRenderIN.Image as Bitmap);
            _ = Task.Run(() =>
            {
                var cts = new CancellationTokenSource();
                ProcessingForm.CreateProcessingForm(transform, cts);
                var result = transform.Process(imageData, null, cts.Token);
                Invoke(() =>
                {
                    bitmapRenderOUT.Image = result.IsSuccess ? ImageToolsGDI.ImageDataToGdiImage(result.DataOut) : null;
                    if (!result.IsSuccess)
                        LogMan.Error(nameof(FormTester), nameof(Run), result.Message);
                    propertyGrid1.SelectedObject = options;
                    propertyGrid2.SelectedObject = transform;
                    pbIN.Refresh();
                    pbOUT.Refresh();
                });
            });
        }

        void TestTransformID() => Process(new IdentityOptions());

        void TestTransformQuantizer() => Process(new QuantizationOptions
        {
            Mode = Enum.Parse<ColorQuantizationMode>(cbQuantization.SelectedItem?.ToString() ?? nameof(ColorQuantizationMode.RGB888)),
        });

        void TestTransformBkgRemover()
        {
            // Every color with blue below 128 is background
            var bkgColors = new List<int>();
            for (int r = 0; r < 256; r++)
                for (int g = 0; g < 256; g++)
                    for (int b = 0; b < 128; b++)
                        bkgColors.Add(ColorIntExt.FromRGB(r, g, b));
            Process(new BackgroundRemovalOptions { BackgroundColors = bkgColors, ReplacementColor = 0 });
        }

        void TestTransformAmiga() => Process(new AmigaOptions
        {
            VideoMode = Enum.Parse<ColorTransformCbmAmiga.EnumAmigaVideoMode>(cbAmigaMode.SelectedItem?.ToString() ?? nameof(ColorTransformCbmAmiga.EnumAmigaVideoMode.Ham6)),
            BasePaletteReduction = new ClusteringOptions { Selection = ColorSelectionMode.EvaluateColorMean },
        });

        void TestTransformLumSat() => Process(new HsvOptions
        {
            HueShift = (double)numLumH.Value,
            SaturationFactor = (double)numLumS.Value / 100,
            BrightnessFactor = (double)numLumV.Value / 100,
        });

        void TestTransformC64() => Process(new C64Options
        {
            VideoMode = Enum.Parse<ColorTransformCbm64.C64VideoMode>(cbC64VideoMode.SelectedItem?.ToString() ?? nameof(ColorTransformCbm64.C64VideoMode.Multicolor)),
            ShowTileBorders = false,
        });

        void TestTransformCluster() => Process(new ClusteringOptions { MaxColors = 16, TrainingLoops = 10, Selection = ColorSelectionMode.EvaluateColorMean });

        void TestTransformAmigaEHB() => Process(new AmigaOptions
        {
            VideoMode = ColorTransformCbmAmiga.EnumAmigaVideoMode.ExtraHalfBright,
            BasePaletteReduction = new ClusteringOptions { Selection = ColorSelectionMode.EvaluateColorMean },
        });

        void TestTransformCPC() => Process(new CpcOptions
        {
            VideoMode = Enum.Parse<ColorTransformAmstradCPC.CPCVideoMode>(cbCPCVideoMode.SelectedItem?.ToString() ?? nameof(ColorTransformAmstradCPC.CPCVideoMode.Mode0)),
        });

        void TestTransformEGA() => Process(new EgaOptions());

        void TestTransformCGA() => Process(new CgaOptions
        {
            VideoMode = Enum.Parse<ColorTransformCGA.CGAVideoMode>(cbCgaVideoMode.SelectedItem?.ToString() ?? nameof(ColorTransformCGA.CGAVideoMode.Mode4_0L)),
        });

        void TestTransformFast() => Process(new FastOptions { MaxColors = 16 });

        void TestTransformMedianCut() => Process(new MedianCutOptions { MaxColors = 16 });

        void TestTransformSpectrum() => Process(new ZxOptions
        {
            PaletteMode = Enum.Parse<ColorTransformZxSpectrum.ZxPaletteMode>(cbZXPaletteMode.SelectedItem?.ToString() ?? nameof(ColorTransformZxSpectrum.ZxPaletteMode.BothPalettes)),
            SeedLow = 128,
            SeedHigh = 255,
            Autotune = Enum.Parse<ColorTransformZxSpectrum.ZxAutotuneMode>(cbZXAutotuneMode.SelectedItem?.ToString() ?? nameof(ColorTransformZxSpectrum.ZxAutotuneMode.None)),
            DitherLowColorImage = true,
            DitherHighColorImage = true,
            IncludeBlackInHighColor = true,
        });

        void TestTransformPalette()
        {
            // 16 gray levels
            var palette = new List<int>();
            for (int i = 0; i < 256; i += 16)
                palette.Add(ColorIntExt.FromRGB(i, i, i));
            Process(new FixedPaletteOptions { UserPalette = palette });
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var txt = cbPreset.Text.ToString();
            if (presetImages.ContainsKey(txt))
            {
                bitmapRenderIN.Image = presetImages[txt];
            }
        }


        private async void butReprocess(object sender, EventArgs e)
        {
            if (lastOptions != null)
            {
                Run(lastOptions);
            }
        }



        private async void butID(object sender, EventArgs e)
        {
            TestTransformID();
        }

        private async void btnQuantizer_Click(object sender, EventArgs e)
        {
            TestTransformQuantizer();
        }

        private async void btnBkgRem_Click(object sender, EventArgs e)
        {
            TestTransformBkgRemover();
        }

        private async void btnAmiga_Click(object sender, EventArgs e)
        {
            TestTransformAmiga();
        }

        private async void btnLumSat_Click(object sender, EventArgs e)
        {
            TestTransformLumSat();
        }

        private async void btnC64_Click(object sender, EventArgs e)
        {
            TestTransformC64();
        }

        private async void btnEGA_Click(object sender, EventArgs e)
        {
            TestTransformEGA();
        }

        private async void btnAmstrad_Click(object sender, EventArgs e)
        {
            TestTransformCPC();
        }

        private async void btnSpeccy_Click(object sender, EventArgs e)
        {
            TestTransformSpectrum();
        }

        private void btnCGA_Click(object sender, EventArgs e)
        {
            TestTransformCGA();
        }

        private async void btnFast_Click(object sender, EventArgs e)
        {
            TestTransformFast();
        }

        private async void btnMedian_Click(object sender, EventArgs e)
        {
            TestTransformMedianCut();
        }

        private async void btnCluster_Click(object sender, EventArgs e)
        {
            TestTransformCluster();
        }

        private async void btnPalette_Click(object sender, EventArgs e)
        {
            TestTransformPalette();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (openLoadImage.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            try
            {
                var bmp = Bitmap.FromFile(openLoadImage.FileName);
                bitmapRenderIN.Image = bmp;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (bitmapRenderOUT.Image != null)
            {
                if (saveImageDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    bitmapRenderOUT.Image.Save(saveImageDialog.FileName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        private void oTimerLog_Tick(object sender, EventArgs e)
        {
            while (logBag.TryDequeue(out var s))
            {
                listBox1.Items.Insert(0, s);
            }
            if (listBox1.Items.Count > 100)
            {
                listBox1.Items.RemoveAt(99);
            }
        }



        private void cbQuantization_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearOldTransform();
        }

        private void cbAmigaMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearOldTransform();

        }

        private void cbC64VideoMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearOldTransform();

        }

        private void cbCPCVideoMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearOldTransform();

        }

        private void cbZXAutotuneMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearOldTransform();

        }

        private void cbZXPaletteMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearOldTransform();

        }

        private void cbCgaVideoMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearOldTransform();
        }
    }
}
