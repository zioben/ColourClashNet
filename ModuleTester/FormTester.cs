
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

        void ClearOldTransform() => oldTransform = null;

        ColorTransformInterface oldTransform = null;
        ColorTransformConfig oldConfig = null;
        /// <summary>
        /// Applies the common UI settings (distance, dithering) to <paramref name="cfg"/> and runs
        /// <paramref name="transform"/> on the input image in background, with a progress form.
        /// The transformation is remembered so that the "reprocess" button can run it again.
        /// </summary>
        void Process(ColorTransformInterface transform, ColorTransformConfig cfg)
        {
            oldTransform = transform;
            oldConfig = cfg;
            bitmapRenderOUT.Image = null;
            var dither = ColorDithering.None;
            if (!Enum.TryParse<ColorDithering>(cbDithering.SelectedItem?.ToString(), out dither))
            {
                dither = ColorDithering.None;
            }
            var color = ColorDistanceEvaluationMode.RGB;
            if (!Enum.TryParse<ColorDistanceEvaluationMode>(cbColorMode.SelectedItem?.ToString(), out color))
            {
                color = ColorDistanceEvaluationMode.RGB;
            }
            cfg.WithColorDistanceEvaluationMode(color)
            .WithDithering(dither, (double)numDitheringStrenght.Value / 100.0, ColorDitheringFx.None);
            transform.SetProperties(cfg);
            var imageData = ImageToolsGDI.GdiImageToImageData(bitmapRenderIN.Image as Bitmap);
            _ = Task.Run(async () =>
            {
                var cts = new CancellationTokenSource();
                ProcessingForm.CreateProcessingForm(transform, cts);
                transform.Create(imageData);
                var result = transform.ProcessColors(cts.Token);
                Invoke(() =>
                {
                    bitmapRenderOUT.Image = ImageToolsGDI.ImageDataToGdiImage(result.DataOut);
                    propertyGrid1.SelectedObject = cfg;
                    propertyGrid2.SelectedObject = transform;
                    pbIN.Refresh();
                    pbOUT.Refresh();
                });
            });
        }


        void TestTransformID()
        {
            if (oldTransform?.Type == ColorTransformType.ColorIdentity)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformIdentity transform = new();
            Process(transform, new ColorTransformConfig());
        }
        void TestTransformQuantizer()
        {
            if (oldTransform?.Type == ColorTransformType.ColorReductionQuantization)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformQuantization transform = new();
            var cfg = new ColorTransformConfig()
                .WithQuantizationMode((ColorQuantizationMode)Enum.Parse(typeof(ColorQuantizationMode), cbQuantization.SelectedItem?.ToString()));
            Process(transform, cfg);
        }

        void TestTransformBkgRemover()
        {
            if (oldTransform?.Type == ColorTransformType.ColorRemover)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformBkgRemover transform = new();
            Palette bkgPalette = new Palette().Create();
            List<int> list = new List<int>();
            for (int r = 0; r < 256; r++)
            {
                for (int g = 0; g < 256; g++)
                {
                    for (int b = 0; b < 128; b++)
                    {
                        bkgPalette.Add(ColorIntExt.FromRGB(r, g, b));
                    }
                }
            }
            var cfg = new ColorTransformConfig()
                .WithBackgroundColorReplacement(bkgPalette, 0);
            Process(transform, cfg);
        }

        void TestTransformAmiga()
        {
            if (oldTransform?.Type == ColorTransformType.ColorReductionHam)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformCbmAmiga transform = new();
            var cfg = new ColorTransformConfig()
                .WithAmigaScreenMode(
                    (ColorTransformCbmAmiga.EnumAmigaVideoMode)Enum.Parse(typeof(ColorTransformCbmAmiga.EnumAmigaVideoMode), cbAmigaMode.SelectedItem?.ToString()),
                     ColorTransformType.ColorReductionClustering,
                     ColorSelectionMode.EvaluateColorMean);
            //                .WithClustering(16, 10, ColorSelectionMode.EvaluateColorMean);
            Process(transform, cfg);
        }

        void TestTransformLumSat()
        {
            if (oldTransform?.Type == ColorTransformType.ColorReductionSaturation)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformLumSat transform = new();
            var cfg = new ColorTransformConfig()
                .WithHSV((double)numLumH.Value, (double)numLumS.Value / 100, (double)numLumV.Value / 100);
            Process(transform, cfg);
        }

        void TestTransformC64()
        {
            if (oldTransform?.Type == ColorTransformType.ColorReductionCBM64)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformCbm64 transform = new();
            var cfg = new ColorTransformConfig()
                .WithC64ScreenMode(
                (ColorTransformCbm64.C64VideoMode) Enum.Parse(typeof(ColorTransformCbm64.C64VideoMode), cbC64VideoMode.SelectedItem?.ToString()),
                ColorTransformCbm64.C64DitheringMode.PreDitherImage,
                false);
            Process(transform, cfg);
        }

        void TestTransformCluster()
        {
            if (oldTransform?.Type == ColorTransformType.ColorReductionClustering)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformClustering transform = new();
            var cfg = new ColorTransformConfig()
                .WithClustering(16, 10, ColorSelectionMode.EvaluateColorMean);
            Process(transform, cfg);
        }


        void TestTransformAmigaEHB()
        {
            if (oldTransform?.Type == ColorTransformType.ColorReductionHam)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformCbmAmiga transform = new();
            var cfg = new ColorTransformConfig()
                .WithAmigaScreenMode(ColorTransformCbmAmiga.EnumAmigaVideoMode.Ham6,
                ColorTransformType.ColorReductionClustering,
                ColorSelectionMode.EvaluateColorMean);
            Process(transform, cfg);
        }

        void TestTransformCPC()
        {
            if (oldTransform?.Type == ColorTransformType.ColorReductionCPC)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColorTransformAmstradCPC transform = new();
            var cfg = new ColorTransformConfig()
                .WithCpcVideoMode((ColorTransformAmstradCPC.CPCVideoMode) Enum.Parse(typeof(ColorTransformAmstradCPC.CPCVideoMode), cbCPCVideoMode.SelectedItem?.ToString()));
            Process(transform, cfg);
        }

        void TestTransformEGA()
        {
            if (oldTransform?.Type == ColorTransformType.ColorReductionEGA)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformEGA transform = new();
            Process(transform, new ColorTransformConfig());
        }

        void TestTransformCGA()
        {
            if (oldTransform?.Type == ColorTransformType.ColorReductionCGA)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformCGA transform = new();
            var cfg = new ColorTransformConfig()
                .WithCGAScreenMode((ColorTransformCGA.CGAVideoMode)Enum.Parse(typeof(ColorTransformCGA.CGAVideoMode), cbCgaVideoMode.SelectedItem?.ToString()),
                    ColorTransformCGA.CGACompositeOutputMode.ProcessedImage); 
            Process(transform,cfg);
        }

        void TestTransformFast()
        {
            if (oldTransform?.Type == ColorTransformType.ColorReductionFast)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformFast transform = new();
            var cfg = new ColorTransformConfig()
                .WithFastReduction(16);
            Process(transform, cfg);
        }

        void TestTransformMedianCut()
        {
            if (oldTransform?.Type == ColorTransformType.ColorReductionMedianCut)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformMedianCut transform = new();
            var cfg = new ColorTransformConfig()
               .WithMedianCut(16);
            Process(transform, cfg);
        }

        void TestTransformSpectrum()
        {
            if (oldTransform?.Type == ColorTransformType.ColorReductionZxSpectrum)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformZxSpectrum transform = new();
            var cfg = new ColorTransformConfig()
                .WithZxScreenMode((ColorTransformZxSpectrum.ZxPaletteMode) Enum.Parse(typeof(ColorTransformZxSpectrum.ZxPaletteMode), cbZXPaletteMode.SelectedItem?.ToString()), 128, 256)
                .WithZxProcessing((ColorTransformZxSpectrum.ZxAutotuneMode) Enum.Parse(typeof(ColorTransformZxSpectrum.ZxAutotuneMode), cbZXAutotuneMode.SelectedItem?.ToString()), true, true, true, false);
            Process(transform, cfg);
        }

        void TestTransformPalette()
        {
            if (oldTransform?.Type == ColorTransformType.ColorReductionGenericPalette)
            {
                Process(oldTransform, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformFixedPalette transform = new();
            var palette = new Palette().Create();
            for (int i = 0; i < 256; i += 16)
            {
                palette.Add(ColorIntExt.FromRGB(i, i, i));
            }
            var cfg = new ColorTransformConfig()
                .WithReferencePalette(palette);
            Process(transform, cfg);
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
            var transform = propertyGrid2.SelectedObject as ColorTransformInterface;
            if (transform != null)
            {
                Process(oldTransform, oldConfig);
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
