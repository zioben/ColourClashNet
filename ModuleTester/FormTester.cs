
using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;

namespace ModuleTester
{
    public partial class FormTester : Form
    {

        FormAdvancer oFormAdv;
        Dictionary<string, Image> oDict = new();

        public FormTester()
        {
            InitializeComponent();
            bitmapRender1.Control = pictureBox1;
            bitmapRender2.Control = pictureBox2;
            bitmapRender1.Image = ColourClashLib.Properties.Resources.BMP_RGB_Palette;//.BMP_1MColors;
            oFormAdv = new FormAdvancer();
            CreateCombo();
        }

        void CreateCombo()
        {
            oDict = new();
            oDict.Add("1M Colors", ColourClashLib.Properties.Resources.BMP_1MColors);
            oDict.Add("RGB Palette", ColourClashLib.Properties.Resources.BMP_RGB_Palette);
            oDict.Add("FADE Base", ColourClashLib.Properties.Resources.BMP_FadeTest);
            oDict.Add("GRANGER Charts", ColourClashLib.Properties.Resources.BMP_Granger_Chart);
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(oDict.Select(X => X.Key).ToArray());
        }


        async Task ProcessAsync(ColorTransformInterface oTrasf)
        {
            oFormAdv.Show();
            oFormAdv.Reset();
            RegisterEvents(oTrasf);
            bitmapRender2.Image = null;
            _ = Task.Run(async () =>
            {
                var cts = new CancellationTokenSource();
                var oData = ImageTools.ToMatrix(bitmapRender1.Image);
                await oTrasf.CreateAsync(oData, cts.Token);
                var ret = await oTrasf.ProcessColorsAsync(cts.Token);
                UnregisterEvents(oTrasf);
                Invoke(() =>
                {
                    oFormAdv.Hide();
                    bitmapRender2.Image = ImageTools.ToBitmap(ret.DataOut);
                    propertyGrid1.SelectedObject = oTrasf;
                    pictureBox1.Refresh();
                    pictureBox2.Refresh();
                });
            });
        }

        void RegisterEvents(ColorTransformInterface oTrasf)
        {
            oTrasf.Processing += OTrasf_Processing;
            oTrasf.ProcessPartial += OTrasf_ProcessPartial;
            oTrasf.Processed += OTrasf_Processed;
        }
        void UnregisterEvents(ColorTransformInterface oTrasf)
        {
            oTrasf.Processing -= OTrasf_Processing;
            oTrasf.ProcessPartial -= OTrasf_ProcessPartial;
            oTrasf.Processed -= OTrasf_Processed;
        }

        private void OTrasf_Processed(object? sender, ColorTransformEventArgs e)
        {

        }

        private void OTrasf_ProcessPartial(object? sender, ColorTransformEventArgs e)
        {
            oFormAdv.AppendData(e);
        }

        private void OTrasf_Processing(object? sender, ColorTransformEventArgs e)
        {
            oFormAdv.AppendData(e);
        }


        async Task TestTransformID()
        {
            ColourClashNet.Color.Transformation.ColorTransformIdentity oTrasf = new();
            oTrasf.SetProperty(ColourClashNet.Color.ColorTransformProperties.Dithering_Model, ColorDithering.None);
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
            var txt = comboBox1.Text.ToString();
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
            //await TestTransformCluster();
            //

            //await TestTransformFast();
            //await TestTransformMedianCut();
            //await TestTransformPalette();


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
    }
}
