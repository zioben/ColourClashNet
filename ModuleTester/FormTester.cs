
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
            bitmapRenderIN.Control = pictureBox1;
            bitmapRenderOUT.Control = pictureBox2;
            CreateCombo();
            LogMan.OnLogMessage += LogMan_OnLogMessage;

            //  TestTransformSpectrum();
        }

        ConcurrentQueue<string> oLogBag = new ConcurrentQueue<string>();

        private void LogMan_OnLogMessage(object? sender, LogEventArgs e)
        {
            oLogBag.Enqueue(e.Message);
        }

        void CreateCombo()
        {
            oDict = new();
            oDict.Add("1M Colors", ColourClashLib.Properties.Resources.BMP_1MColors);
            oDict.Add("RGB Palette", ColourClashLib.Properties.Resources.BMP_RGB_Palette);
            oDict.Add("FADE Base", ColourClashLib.Properties.Resources.BMP_FadeTest);
            oDict.Add("GRANGER Charts", ColourClashLib.Properties.Resources.BMP_Granger_Chart);
            cbPreset.Items.Clear();
            cbPreset.Items.AddRange(oDict.Select(X => X.Key).ToArray());

            var items = Enum.GetNames<ColorDithering>().ToList();
            cbDithering.Items.Clear();
            foreach (var item in items)
            {
                cbDithering.Items.Add(item);
            }
            cbDithering.SelectedIndex = 0;

            items = Enum.GetNames<ColorDistanceEvaluationMode>().ToList();
            foreach (var item in items)
            {
                cbColorMode.Items.Add(item);
            }
            cbPreset.SelectedIndex = 1;
            cbColorMode.SelectedIndex = 0;
            cbDithering.SelectedIndex = 9;
        }

        ColorTransformInterface oOldTrasf = null;
        void Process(ColorTransformInterface oTrasf)
        {
            oOldTrasf = oTrasf;
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
            double ditherStrength = (double)numDitheringStrenght.Value / 100.0;
            oTrasf.SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, eColor);
            oTrasf.SetProperty(ColorTransformProperties.DitheringType, eDither);
            oTrasf.SetProperty(ColorTransformProperties.DitheringStrength, ditherStrength);
            var oImageData = ImageToolsGDI.GdiImageToImageData(bitmapRenderIN.Image as Bitmap);
            _ = Task.Run(async () =>
            {
                var cts = new CancellationTokenSource();
                ProcessingForm.CreateProcessingForm(oTrasf, cts);
                oTrasf.Create(oImageData);
                var ret = oTrasf.ProcessColors(cts.Token);
                Invoke(() =>
                {
                    bitmapRenderOUT.Image = ImageToolsGDI.ImageDataToGdiImage(ret.DataOut);
                    propertyGrid1.SelectedObject = oTrasf;
                    pictureBox1.Refresh();
                    pictureBox2.Refresh();
                });
            });
        }


        void TestTransformID()
        {
            if (oOldTrasf?.Type == ColorTransformType.ColorIdentity)
            {
                Process(oOldTrasf);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformIdentity oTrasf = new();
            oTrasf.SetProperty(ColourClashNet.Color.ColorTransformProperties.DitheringType, ColorDithering.None);
            oTrasf.SetProperty(ColorTransformProperties.MaxColorsWanted, 16);
            Process(oTrasf);
        }
        void TestTransformQuantizer()
        {
            if (oOldTrasf?.Type == ColorTransformType.ColorReductionQuantization)
            {
                Process(oOldTrasf);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformQuantization oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.QuantizationMode, ColorQuantizationMode.RGB222);
            Process(oTrasf);
        }

        void TestTransformBkgRemover()
        {
            if (oOldTrasf?.Type == ColorTransformType.ColorRemover)
            {
                Process(oOldTrasf);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformBkgRemover oTrasf = new();
            List<int> oList = new List<int>();
            for (int r = 0; r < 256; r++)
            {
                for (int g = 0; g < 256; g++)
                {
                    for (int b = 0; b < 128; b++)
                    {
                        oList.Add(ColorIntExt.FromRGB(r, g, b));
                    }
                }
            }
            oTrasf.SetProperty(ColorTransformProperties.ColorBackgroundList, oList);
            oTrasf.SetProperty(ColorTransformProperties.ColorBackgroundReplacement, 0);
            Process(oTrasf);
        }

        void TestTransformAmiga()
        {
            if (oOldTrasf?.Type == ColorTransformType.ColorReductionHam)
            {
                Process(oOldTrasf);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformReductionAmiga oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.AmigaVideoMode, ColorTransformReductionAmiga.EnumAmigaVideoMode.Ham6);
            oTrasf.SetProperty(ColorTransformProperties.AmigaHamColorProcessingMode, ColorTransformReductionAmiga.EnumHamColorProcessingMode.Fast);
            Process(oTrasf);
        }

        void TestTransformLumSat()
        {
            if (oOldTrasf?.Type == ColorTransformType.ColorReductionSaturation)
            {
                Process(oOldTrasf);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformLumSat oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.HsvBrightnessMultFactor, 2);
            oTrasf.SetProperty(ColorTransformProperties.HsvHueShift, 180);
            oTrasf.SetProperty(ColorTransformProperties.HsvSaturationMultFactor, 2);
            Process(oTrasf);
        }

        void TestTransformC64()
        {
            if (oOldTrasf?.Type == ColorTransformType.ColorReductionCBM64)
            {
                Process(oOldTrasf);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformReductionC64 oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.C64VideoMode, ColorTransformReductionC64.C64VideoMode.BitmapHiRes);
            Process(oTrasf);
        }

        void TestTransformCluster()
        {
            if (oOldTrasf?.Type == ColorTransformType.ColorReductionClustering)
            {
                Process(oOldTrasf);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformReductionCluster oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.ClusterTrainingLoop, 10);
            oTrasf.SetProperty(ColorTransformProperties.UseColorMean, true);
            oTrasf.SetProperty(ColorTransformProperties.MaxColorsWanted, 16);
            Process(oTrasf);
        }

        void TestTransformCPC()
        {
            if (oOldTrasf?.Type == ColorTransformType.ColorReductionCPC)
            {
                Process(oOldTrasf);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformReductionCPC oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.CPCVideoMode, ColorTransformReductionCPC.CPCVideoMode.Mode0);
            Process(oTrasf);
        }

        void TestTransformEGA()
        {
            if (oOldTrasf?.Type == ColorTransformType.ColorReductionEga)
            {
                Process(oOldTrasf);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformReductionEGA oTrasf = new();
            Process(oTrasf);
        }

        void TestTransformFast()
        {
            if (oOldTrasf?.Type == ColorTransformType.ColorReductionFast)
            {
                Process(oOldTrasf);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformReductionFast oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.MaxColorsWanted, 16);
            Process(oTrasf);
        }

        void TestTransformMedianCut()
        {
            if (oOldTrasf?.Type == ColorTransformType.ColorReductionMedianCut)
            {
                Process(oOldTrasf);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformReductionMedianCut oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.MaxColorsWanted, 16);
            oTrasf.SetProperty(ColorTransformProperties.UseColorMean, true);
            Process(oTrasf);
        }

        void TestTransformSpectrum()
        {
            if (oOldTrasf?.Type == ColorTransformType.ColorReductionZxSpectrum)
            {
                Process(oOldTrasf);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformReductionZxSpectrum oTrasf = new();

            var autoTune = ColorTransformReductionZxSpectrum.ZxAutotuneMode.Fast;
            if (autoTune == ColorTransformReductionZxSpectrum.ZxAutotuneMode.None)
            {
                oTrasf.SetProperty(ColorTransformProperties.ZxColLSeed, 128);
                oTrasf.SetProperty(ColorTransformProperties.ZxColHSeed, 256);
            }
            else
            {
                oTrasf.SetProperty(ColorTransformProperties.ZxColLSeed, 0x00C0);
                oTrasf.SetProperty(ColorTransformProperties.ZxColHSeed, 0x00FF);
            }
            oTrasf.SetProperty(ColorTransformProperties.ZxAutotuneMode, autoTune);
            oTrasf.SetProperty(ColorTransformProperties.ZxDitherHighColorImage, true);
            oTrasf.SetProperty(ColorTransformProperties.ZxIncludeBlackInHighColorImage, true);
            oTrasf.SetProperty(ColorTransformProperties.ZxPaletteMode, ColorTransformReductionZxSpectrum.ZxPaletteMode.Both);
            Process(oTrasf);
        }

        void TestTransformPalette()
        {
            if (oOldTrasf?.Type == ColorTransformType.ColorReductionGenericPalette)
            {
                Process(oOldTrasf);
                return;
            }
            ColourClashNet.Color.Transformation.ColorTransformReductionPalette oTrasf = new();
            List<int> oList = new List<int>();
            for (int i = 0; i < 256; i += 16)
            {
                oList.Add(ColorIntExt.FromRGB(i, i, i));
            }
            oTrasf.SetProperty(ColorTransformProperties.PriorityPalette, oList);
            Process(oTrasf);
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
            var oTrasf = propertyGrid1.SelectedObject as ColorTransformInterface;
            if (oTrasf != null)
            {
                Process(oTrasf);
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
                if( saveImageDialog.ShowDialog() != DialogResult.OK  )
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

      
    }
}
