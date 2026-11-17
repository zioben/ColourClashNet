
using ColourClashLib.Color.Transformation;
using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ModuleTester
{
    public partial class FormTester : Form
    {
        Dictionary<string, Image> oDict = new();

        public FormTester()
        {
            InitializeComponent();
            bitmapRenderIN.Control = pbIN;
            bitmapRenderOUT.Control = pbOUT;
            CreateCombos();
            LogMan.OnLogMessage += LogMan_OnLogMessage;

            //  TestTransformSpectrum();
        }

        ConcurrentQueue<string> oLogBag = new ConcurrentQueue<string>();

        private void LogMan_OnLogMessage(object? sender, LogEventArgs e)
        {
            oLogBag.Enqueue(e.Message);
        }
        void CreateCombo(List<string> stringList, ComboBox cb, int? defaultIndex = null)
        {
            cb.Items.Clear();
            if (stringList == null)
            {
                return;
            }
            foreach (var item in stringList)
            {
                cb.Items.Add(item);
            }
            if (defaultIndex.HasValue && defaultIndex.Value >= 0 && defaultIndex.Value < cb.Items.Count)
            {
                cb.SelectedIndex = defaultIndex.Value;
            }
        }

        void CreateCombo(Type enumType, ComboBox cb, int? defaultIndex = null)
            => CreateCombo(Enum.GetNames(enumType).ToList(), cb, defaultIndex);

        void CreateCombos()
        {
            oDict = new();
            oDict.Add("1M Colors", ColourClashDrawings.Properties.Resources.BMP_1Mcolors);
            oDict.Add("RGB Palette", ColourClashDrawings.Properties.Resources.BMP_RGB_Palette);
            oDict.Add("FADE Base", ColourClashDrawings.Properties.Resources.BMP_FadeTest);
            oDict.Add("GRANGER Charts", ColourClashDrawings.Properties.Resources.BMP_Granger_Chart);

            CreateCombo(oDict.Select(X => X.Key).ToList(), cbPreset, 1);
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

        void ClearOldTransform() => oldTrasf = null;

        ColorTransformInterface oldTrasf = null;
        ColorTransformConfig oldConfig = null;
        void Process(ColorTransformInterface transf, ColorTransformConfig cfg)
        {
            oldTrasf = transf;
            oldConfig = cfg;
            bitmapRenderOUT.Image = null;
            var eDither = ColorDithering.None;
            if (!Enum.TryParse<ColorDithering>(cbDithering.SelectedItem?.ToString(), out eDither))
            {
                eDither = ColorDithering.None;
            }
            var eColor = ColorDistanceEvaluationMode.RGB;
            if (!Enum.TryParse<ColorDistanceEvaluationMode>(cbColorMode.SelectedItem?.ToString(), out eColor))
            {
                eColor = ColorDistanceEvaluationMode.RGB;
            }
            cfg.WithColorDistanceEvaluationMode(eColor)
            //.WithQuantizationMode()
            .WithDithering(eDither, (double)numDitheringStrenght.Value / 100.0, ColorDitheringFx.None);
            transf.SetProperties(cfg);
            var oImageData = ImageToolsGDI.GdiImageToImageData(bitmapRenderIN.Image as Bitmap);
            _ = Task.Run(async () =>
            {
                var cts = new CancellationTokenSource();
                ProcessingForm.CreateProcessingForm(transf, cts);
                transf.Create(oImageData);
                var ret = transf.ProcessColors(cts.Token);
                Invoke(() =>
                {
                    bitmapRenderOUT.Image = ImageToolsGDI.ImageDataToGdiImage(ret.DataOut);
                    propertyGrid1.SelectedObject = cfg;
                    propertyGrid2.SelectedObject = transf;
                    pbIN.Refresh();
                    pbOUT.Refresh();
                });
            });
        }


        void TestTransformID()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorIdentity)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformIdentity transf = new();
            // transf.SetProperty(ColourClashNet.Color.ColorTransformProperties.DitheringType, ColorDithering.None);
            // transf.SetProperty(ColorTransformProperties.MaxColorsWanted, 16);
            Process(transf, new ColorTransformConfig());
        }
        void TestTransformQuantizer()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorReductionQuantization)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformQuantization transf = new();
            var cfg = new ColorTransformConfig()
                .WithQuantizationMode((ColorQuantizationMode)Enum.Parse(typeof(ColorQuantizationMode), cbQuantization.SelectedItem?.ToString()));
            Process(transf, cfg);
        }

        void TestTransformBkgRemover()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorRemover)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformBkgRemover transf = new();
            Palette pal = new Palette().Create();
            List<int> oList = new List<int>();
            for (int r = 0; r < 256; r++)
            {
                for (int g = 0; g < 256; g++)
                {
                    for (int b = 0; b < 128; b++)
                    {
                        pal.Add(ColorIntExt.FromRGB(r, g, b));
                    }
                }
            }
            var cfg = new ColorTransformConfig()
                .WithBackgroundColorReplacement(pal, 0);
            Process(transf, cfg);
        }

        void TestTransformAmiga()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorReductionHam)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformCbmAmiga transf = new();
            var cfg = new ColorTransformConfig()
                .WithAmigaScreenMode(
                    (ColorTransformCbmAmiga.EnumAmigaVideoMode)Enum.Parse(typeof(ColorTransformCbmAmiga.EnumAmigaVideoMode), cbAmigaMode.SelectedItem?.ToString()),
                     ColorTransformType.ColorReductionClustering,
                     ColorSelectionMode.EvaluateColorMean);
            //                .WithClustering(16, 10, ColorSelectionMode.EvaluateColorMean);
            Process(transf, cfg);
        }

        void TestTransformLumSat()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorReductionSaturation)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformLumSat transf = new();
            var cfg = new ColorTransformConfig()
                .WithHSV((double)numLumH.Value, (double)numLumS.Value / 100, (double)numLumV.Value / 100);
            Process(transf, cfg);
        }

        void TestTransformC64()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorReductionCBM64)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformCbm64 transf = new();
            var cfg = new ColorTransformConfig()
                .WithC64ScreenMode(
                (ColorTransformCbm64.C64VideoMode) Enum.Parse(typeof(ColorTransformCbm64.C64VideoMode), cbC64VideoMode.SelectedItem?.ToString()),
                ColorTransformCbm64.C64DitheringMode.PreDitherImage,
                false);
            Process(transf, cfg);
        }

        void TestTransformCluster()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorReductionClustering)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformClustering transf = new();
            var cfg = new ColorTransformConfig()
                .WithClustering(16, 10, ColorSelectionMode.EvaluateColorMean);
            Process(transf, cfg);
        }


        void TestTransformAmigaEHB()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorReductionHam)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformCbmAmiga transf = new();
            var cfg = new ColorTransformConfig()
                .WithAmigaScreenMode(ColorTransformCbmAmiga.EnumAmigaVideoMode.Ham6,
                ColorTransformType.ColorReductionClustering,
                ColorSelectionMode.EvaluateColorMean);
            Process(transf, cfg);
        }

        void TestTransformCPC()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorReductionCPC)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColorTransformAmstradCPC transf = new();
            var cfg = new ColorTransformConfig()
                .WithCpcVideoMode((ColorTransformAmstradCPC.CPCVideoMode) Enum.Parse(typeof(ColorTransformAmstradCPC.CPCVideoMode), cbCPCVideoMode.SelectedItem?.ToString()));
            Process(transf, cfg);
        }

        void TestTransformEGA()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorReductionEGA)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformEGA transf = new();
            Process(transf, new ColorTransformConfig());
        }

        void TestTransformCGA()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorReductionCGA)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformCGA transf = new();
            var cfg = new ColorTransformConfig()
                .WithCGAScreenMode((ColorTransformCGA.CGAVideoMode)Enum.Parse(typeof(ColorTransformCGA.CGAVideoMode), cbCgaVideoMode.SelectedItem?.ToString()),
                    ColorTransformCGA.CGACompositeOutputMode.ProcessedImage); 
            Process(transf,cfg);
        }

        void TestTransformFast()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorReductionFast)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformFast transf = new();
            var cfg = new ColorTransformConfig()
                .WithFastReduction(16);
            Process(transf, cfg);
        }

        void TestTransformMedianCut()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorReductionMedianCut)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformMedianCut transf = new();
            var cfg = new ColorTransformConfig()
               .WithMedianCut(16);
            Process(transf, cfg);
        }

        void TestTransformSpectrum()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorReductionZxSpectrum)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformZxSpectrum transf = new();
            var cfg = new ColorTransformConfig()
                .WithZxScreenMode((ColorTransformZxSpectrum.ZxPaletteMode) Enum.Parse(typeof(ColorTransformZxSpectrum.ZxPaletteMode), cbZXPaletteMode.SelectedItem?.ToString()), 128, 256)
                .WithZxProcessing((ColorTransformZxSpectrum.ZxAutotuneMode) Enum.Parse(typeof(ColorTransformZxSpectrum.ZxAutotuneMode), cbZXAutotuneMode.SelectedItem?.ToString()), true, true, true, false);
            Process(transf, cfg);
        }

        void TestTransformPalette()
        {
            if (oldTrasf?.Type == ColorTransformType.ColorReductionGenericPalette)
            {
                Process(oldTrasf, oldConfig);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformFixedPalette transf = new();
            var palette = new Palette().Create();
            for (int i = 0; i < 256; i += 16)
            {
                palette.Add(ColorIntExt.FromRGB(i, i, i));
            }
            var cfg = new ColorTransformConfig()
                .WithReferencePalette(palette);
            Process(transf, cfg);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var txt = cbPreset.Text.ToString();
            if (oDict.ContainsKey(txt))
            {
                bitmapRenderIN.Image = oDict[txt];
            }
        }


        private async void butReprocess(object sender, EventArgs e)
        {
            var transf = propertyGrid2.SelectedObject as ColorTransformInterface;
            if (transf != null)
            {
                Process(oldTrasf, oldConfig);
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
                var oBmp = Bitmap.FromFile(openLoadImage.FileName);
                bitmapRenderIN.Image = oBmp;
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
            while (oLogBag.TryDequeue(out var s))
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
