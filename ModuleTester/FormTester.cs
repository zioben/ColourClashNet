
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
            bitmapRender1.Control = pictureBox1;
            bitmapRender2.Control = pictureBox2;
            CreateCombo();
            LogMan.OnLogMessage += LogMan_OnLogMessage;
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
            cbDithering.SelectedIndex = 0;


            cbPreset.SelectedIndex = 1;
        }


        async Task ProcessAsync(ColorTransformInterface oTrasf)
        {
            bitmapRender2.Image = null;
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
            oTrasf.SetProperty(ColorTransformProperties.ColorDistanceEvaluationMode, eColor);
            oTrasf.SetProperty(ColorTransformProperties.Dithering_Type, eDither);
            oTrasf.SetProperty(ColorTransformProperties.Dithering_Strength, 1);
            var oData = ImageTools.GdiImageToImageData(bitmapRender1.Image as Bitmap);
            _ = Task.Run(async () =>
            {
                var cts = new CancellationTokenSource();
                ProcessingForm.CreateProcessingForm(oTrasf);
                oTrasf.Create(oData);
                var ret = await oTrasf.ProcessColorsAsync(cts.Token);
                Invoke(() =>
                {
                    bitmapRender2.Image = ImageTools.ImageDataToGdiImage(ret.DataOut);
                    propertyGrid1.SelectedObject = oTrasf;
                    pictureBox1.Refresh();
                    pictureBox2.Refresh();
                });
            });
        }


        async Task TestTransformID()
        {
            ColourClashNet.Color.Transformation.ColorTransformIdentity oTrasf = new();
            oTrasf.SetProperty(ColourClashNet.Color.ColorTransformProperties.Dithering_Type, ColorDithering.None);
            oTrasf.SetProperty(ColorTransformProperties.MaxColorsWanted, 16);
            await ProcessAsync(oTrasf);
        }
        async Task TestTransformQuantizer()
        {
            ColourClashNet.Color.Transformation.ColorTransformQuantization oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.QuantizationMode, ColorQuantizationMode.RGB222);
            await ProcessAsync(oTrasf);
        }

        async Task TestTransformBkgRemover()
        {
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
            await ProcessAsync(oTrasf);
        }

        async Task TestTransformAmiga()
        {
            ColourClashNet.Color.Transformation.ColorTransformReductionAmiga oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.Amiga_VideoMode, ColorTransformReductionAmiga.EnumAMigaVideoMode.Ham6);
            oTrasf.SetProperty(ColorTransformProperties.Amiga_HamColorReductionMode, ColorTransformReductionAmiga.EnumHamFirstColorReductionMode.Fast);
            await ProcessAsync(oTrasf);
        }

        async Task TestTransformLumSat()
        {
            ColourClashNet.Color.Transformation.ColorTransformLumSat oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.HsvBrightnessMultFactor, 2);
            oTrasf.SetProperty(ColorTransformProperties.HsvHueShift, 180);
            oTrasf.SetProperty(ColorTransformProperties.HsvSaturationMultFactor, 2);
            await ProcessAsync(oTrasf);
        }

        async Task TestTransformC64()
        {
            ColourClashNet.Color.Transformation.ColorTransformReductionC64 oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.C64_VideoMode, ColorTransformReductionC64.C64VideoMode.Multicolor);
            await ProcessAsync(oTrasf);
        }

        async Task TestTransformCluster()
        {
            ColourClashNet.Color.Transformation.ColorTransformReductionCluster oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.ClusterTrainingLoop, 10);
            oTrasf.SetProperty(ColorTransformProperties.UseColorMean, true);
            oTrasf.SetProperty(ColorTransformProperties.MaxColorsWanted, 16);
            await ProcessAsync(oTrasf);
        }

        async Task TestTransformCPC()
        {
            ColourClashNet.Color.Transformation.ColorTransformReductionCPC oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.CPC_VideoMode, ColorTransformReductionCPC.CPCVideoMode.Mode0);
            await ProcessAsync(oTrasf);
        }

        async Task TestTransformEGA()
        {
            ColourClashNet.Color.Transformation.ColorTransformReductionEGA oTrasf = new();
            await ProcessAsync(oTrasf);
        }

        async Task TestTransformFast()
        {
            ColourClashNet.Color.Transformation.ColorTransformReductionFast oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.MaxColorsWanted, 16);
            await ProcessAsync(oTrasf);
        }

        async Task TestTransformMedianCut()
        {
            ColourClashNet.Color.Transformation.ColorTransformReductionMedianCut oTrasf = new();
            oTrasf.SetProperty(ColorTransformProperties.MaxColorsWanted, 16);
            oTrasf.SetProperty(ColorTransformProperties.UseColorMean, true);
            await ProcessAsync(oTrasf);
        }

        async Task TestTransformSpectrum()
        {
            bool bAtuotune = true;
            ColourClashNet.Color.Transformation.ColorTransformReductionZxSpectrumV2 oTrasf = new();
            if (bAtuotune)
            {
                oTrasf.SetProperty(ColorTransformProperties.Zx_ColL_Seed, 0x0000);
                oTrasf.SetProperty(ColorTransformProperties.Zx_ColH_Seed, 0x00FF);
            }
            else
            {
                oTrasf.SetProperty(ColorTransformProperties.Zx_ColL_Seed, 0x00C0);
                oTrasf.SetProperty(ColorTransformProperties.Zx_ColH_Seed, 0x00FF);
            }
            oTrasf.SetProperty(ColorTransformProperties.Zx_Autotune, bAtuotune);
            oTrasf.SetProperty(ColorTransformProperties.Zx_DitherHighColorImage, true);
            oTrasf.SetProperty(ColorTransformProperties.Zx_IncludeBlackInHighColorImage, true);
            oTrasf.SetProperty(ColorTransformProperties.Zx_PaletteMode, ColorTransformReductionZxSpectrum.ZxPaletteMode.Both);
            await ProcessAsync(oTrasf);
        }

        async Task TestTransformPalette()
        {
            ColourClashNet.Color.Transformation.ColorTransformReductionPalette oTrasf = new();
            List<int> oList = new List<int>();
            for (int i = 0; i < 256; i += 16)
            {
                oList.Add(ColorIntExt.FromRGB(i, i, i));
            }
            oTrasf.SetProperty(ColorTransformProperties.Fixed_Palette, oList);
            await ProcessAsync(oTrasf);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var txt = cbPreset.Text.ToString();
            if (oDict.ContainsKey(txt))
            {
                bitmapRender1.Image = oDict[txt];
            }
        }


        private async void butReprocess(object sender, EventArgs e)
        {
            var oTrasf = propertyGrid1.SelectedObject as ColorTransformInterface;
            if (oTrasf != null)
            {
                await ProcessAsync(oTrasf);
            }
        }



        private async void butID(object sender, EventArgs e)
        {
            await TestTransformID();
        }

        private async void btnQuantizer_Click(object sender, EventArgs e)
        {
            await TestTransformQuantizer();
        }

        private async void btnBkgRem_Click(object sender, EventArgs e)
        {
            await TestTransformBkgRemover();
        }

        private async void btnAmiga_Click(object sender, EventArgs e)
        {
            await TestTransformAmiga();
        }

        private async void btnLumSat_Click(object sender, EventArgs e)
        {
            await TestTransformLumSat();
        }

        private async void btnC64_Click(object sender, EventArgs e)
        {
            await TestTransformC64();
        }

        private async void btnEGA_Click(object sender, EventArgs e)
        {
            await TestTransformEGA();
        }

        private async void btnAmstrad_Click(object sender, EventArgs e)
        {
            await TestTransformCPC();
        }

        private async void btnSpeccy_Click(object sender, EventArgs e)
        {
            await TestTransformSpectrum();
        }

        private void btnCGA_Click(object sender, EventArgs e)
        {

        }

        private async void btnFast_Click(object sender, EventArgs e)
        {
            await TestTransformFast();
        }

        private async void btnMedian_Click(object sender, EventArgs e)
        {
            await TestTransformMedianCut();
        }

        private async void btnCluster_Click(object sender, EventArgs e)
        {
            await TestTransformCluster();
        }

        private async void btnPalette_Click(object sender, EventArgs e)
        {
            await TestTransformPalette();
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
                bitmapRender1.Image = oBmp;
            }
            catch (Exception ex)
            {
            }
        }

       
        private void oTimerLog_Tick(object sender, EventArgs e)
        {
            while (oLogBag.TryDequeue( out var s ) )
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
