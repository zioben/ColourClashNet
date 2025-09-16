using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Controls;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColourClashNet
{
    public partial class ColorAnalyzer : UserControl
    {

        int Panel1MaxWidth = 400;


        public ColorAnalyzer()
        {
            InitializeComponent();
            oColorManager.Config.BackgroundColorList = GetBkgColors();
            oColorManager.Config.ColorQuantizationMode = ColorQuantizationMode.RGB888;//GetQuantizationMode();
            oColorManager.Config.DitheringAlgorithm = ColorDithering.FloydSteinberg;
            pbBkColor.BackColor = ColorDefaults.DefaultBkgColor;
            InitMenu();
            CreateComboBox(cbC64VideoMode, Enum.GetNames(typeof(ColorTransformReductionC64.C64VideoMode)).ToList());
            CreateComboBox(cbCpcVideoMode, Enum.GetNames(typeof(ColorTransformReductionCPC.CPCVideoMode)).ToList());
            CreateComboBox(cbAmigaVideoMode, Enum.GetNames(typeof(ColorTransformReductionAmiga.EnumAMigaVideoMode)).ToList());
            oBitmapRenderSource.ColorAdded += (s, e) => { BuildBkgPalette(); };
            oBitmapRenderSource.ColorRemoved += (s, e) => { BuildBkgPalette(); };
            oColorManager.OnQuantize += (s, e) => { RefreshData(); };
            oColorManager.OnProcess += (s, e) => { RefreshData(); };
        }


        List<ToolStripMenuItem> lTsItems = new List<ToolStripMenuItem>();

        void RebulidSetCheck(string sItem)
        {
            var ts = lTsItems.FirstOrDefault(X => X.Tag.ToString() == sItem);
            if (ts != null)
                ts.Checked = true;
        }

        void RebuildChecks()
        {
            lTsItems.ForEach(X => X.Checked = false);
            RebulidSetCheck(oColorManager.Config.ColorQuantizationMode.ToString());
            RebulidSetCheck(oColorManager.Config.ColorDistanceEvaluationMode.ToString());
            RebulidSetCheck(oColorManager.Config.DitheringAlgorithm.ToString());
        }

        private void TsItem_Click(object? sender, EventArgs e)
        {
            var oTS = sender as ToolStripMenuItem;
            var oTag = oTS.Tag;
            if (oTag is ColorQuantizationMode)
            {
                oColorManager.Config.ColorQuantizationMode = (ColorQuantizationMode)oTag;
                oColorManager.PreProcess();
            }
            else if (oTag is ColorDistanceEvaluationMode)
            {
                oColorManager.Config.ColorDistanceEvaluationMode = (ColorDistanceEvaluationMode)oTag;
            }
            else if (oTag is ColorDithering)
            {
                oColorManager.Config.DitheringAlgorithm = (ColorDithering)oTag;
            }
            RebuildChecks();
        }


        void CreateMenuItem(ToolStripMenuItem oTsBase, object oItem)
        {
            if (oItem.ToString() == "Unknown")
                return;
            var tsItem = new System.Windows.Forms.ToolStripMenuItem();
            tsItem.Name = oItem.ToString();
            tsItem.Size = new System.Drawing.Size(180, 22);
            tsItem.Text = oItem.ToString();
            tsItem.CheckOnClick = true;
            tsItem.Tag = oItem;
            tsItem.Click += TsItem_Click;
            oTsBase.DropDownItems.Add(tsItem);
            lTsItems.Add(tsItem);
        }

        void CreateComboBox(ComboBox ocb, List<string> lItems)
        {
            ocb.Items.Clear();
            lItems.ForEach(X => ocb.Items.Add(X));
            ocb.SelectedIndex = 0;
        }

        void InitMenu<T>(ToolStripMenuItem oTsBase) where T : System.Enum
        {
            var lColorMode = Enum.GetValues(typeof(T));
            foreach (var X in lColorMode)
            {
                CreateMenuItem(oTsBase, X);
            }
            ;
        }

        void InitMenu()
        {
            InitMenu<ColorQuantizationMode>(colorModeToolStripMenuItem);
            InitMenu<ColorDistanceEvaluationMode>(colorDistanceToolStripMenuItem);
            InitMenu<ColorDithering>(ditheringToolStripMenuItem);
            RebuildChecks();
        }

        void BuildBkgPalette()
        {
            pbBkColor.Image = null;
            var lBkgColor = oBitmapRenderSource.SelectedColors;
            if (lBkgColor.Count > 0)
            {
                Bitmap oBmp = new Bitmap(lBkgColor.Count * 16, 16);
                using (var g = Graphics.FromImage(oBmp))
                {
                    int index = 0;
                    for (int i = 0; i < lBkgColor.Count; i++)
                    {
                        var item = lBkgColor[i];
                        using (var oBrush = new SolidBrush(item))
                        {
                            g.FillRectangle(oBrush, i * 16, 0, 16, 16);
                        }
                        index++;
                    }
                    ;
                }
                pbBkColor.SizeMode = PictureBoxSizeMode.StretchImage;
                pbBkColor.Image = oBmp;
            }
            oColorManager.Config.BackgroundColorList = GetBkgColors();
            oColorManager.Config.BackgroundColorReplacement = ColorIntExt.FromDrawingColor(ColorDefaults.DefaultBkgColor);
            oColorManager.PreProcess();
        }

        void ShowImage()
        {
            oBitmapRenderSource.Image = oColorManager.ImageSource.Clone() as Image;
            oBitmapRenderDest.Image = oColorManager.ImageProcessed.Clone() as Image;
            Invalidate();
        }

        void RefreshData()
        {
            pgColorProcessor.SelectedObject = oColorManager;
            ShowImage();
        }


        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ofdSelectImage.ShowDialog() == DialogResult.OK)
            {
                var oBmp = Bitmap.FromFile(ofdSelectImage.FileName) as Bitmap;
                oBitmapRenderDest.ResetMouseSelectedColors();
                oBitmapRenderDest.OriginZero();
                oBitmapRenderSource.ResetMouseSelectedColors();
                oBitmapRenderSource.OriginZero();
                //
                oColorManager.Config.BackgroundColorList = GetBkgColors();
                oColorManager.Config.BackgroundColorReplacement = ColorIntExt.FromDrawingColor(ColorDefaults.DefaultBkgColor);
                oColorManager.Create(oBmp);
            }
        }


        List<int> GetBkgColors()
        {
            var oRet = new List<int>();
            foreach (var item in oBitmapRenderSource.SelectedColors)
                oRet.Add(ColorIntExt.FromDrawingColor(item));
            return oRet;
        }




        private void SetToControl()
        {
            oColorManager.Config.BackgroundColorList = GetBkgColors();
            oColorManager.Config.BackgroundColorReplacement = ColorIntExt.FromDrawingColor(ColorDefaults.DefaultBkgColor);

            oColorManager.Config.ColorsMax = (int)nudColorsWanted.Value;
            oColorManager.Config.ScanlineClustering = chkScanLineCluster.Checked;
            oColorManager.Config.ScanlineColorsMax = (int)nudScanlineLineColors.Value;
            oColorManager.Config.ScanlineSharedPalette = chkScanlineSharedPal.Checked;

            oColorManager.Config.ClusteringTrainingLoop = (int)nudClusterLoop.Value;
            oColorManager.Config.ClusteringUseMeanColor = true;
            oColorManager.Config.DitheringStrenght = (double)nudDitheringStrenght.Value;
            oColorManager.Config.SaturationEnhancement = (double)nudSat.Value;
            oColorManager.Config.BrightnessEnhancement = (double)nudBright.Value;
            oColorManager.Config.HsvHueOffset = (double)nudHue.Value;
            oColorManager.Config.C64ScreenMode = (ColorTransformReductionC64.C64VideoMode)Enum.Parse(typeof(ColorTransformReductionC64.C64VideoMode), cbC64VideoMode.SelectedItem.ToString());
            oColorManager.Config.CPCScreenMode = (ColorTransformReductionCPC.CPCVideoMode)Enum.Parse(typeof(ColorTransformReductionCPC.CPCVideoMode), cbCpcVideoMode.SelectedItem.ToString());
            oColorManager.Config.ZxEqColorLO = (int)nudZxColorLO.Value;
            oColorManager.Config.ZxEqColorHI = (int)nudZxColorHI.Value;
            oColorManager.Config.ZxEqBlackHI = chkZxBlackHI.Checked;
            oColorManager.Config.ZxEqDitherHI = chkZxDitherHI.Checked;
            oColorManager.Config.AmigaVideoMode = (ColorTransformReductionAmiga.EnumAMigaVideoMode)Enum.Parse(typeof(ColorTransformReductionAmiga.EnumAMigaVideoMode), cbAmigaVideoMode.SelectedItem.ToString());
        }

        private void btnReduceColors_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorManager.ProcessColors(ColorTransformType.ColorReductionMedianCut);
        }
        private void btnReduceColorsScanline_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorManager.ProcessColors(ColorTransformType.ColorReductionScanline);
        }

        private void btnReduceColorCluster_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorManager.ProcessColors(ColorTransformType.ColorReductionClustering);
        }

        private void pbBkColor_DoubleClick(object sender, EventArgs e)
        {
            //if (lBkgColor.Count > 0)
            //{
            //    lBkgColor.RemoveAt(lBkgColor.Count - 1);
            //    Reprocess();
            //}
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            oColorManager.Reset();
        }

        private void toolStripMenuItemSave_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageTools.Export(oColorManager.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageExportFormat.Png24);
            }
        }


        private void oColorManager_OnReset(object sender, EventArgs e)
        {
            RefreshData();
        }


        private void bitmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageTools.Export(oColorManager.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageExportFormat.Bmp24);
            }
        }

        private void pNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageTools.Export(oColorManager.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageExportFormat.Png24);
            }
        }

        private void bitmapIndexedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                // oColorManager.WriteBitmapIndex(sfdExportImage.FileName, ImageWidthAlignMode.MultiplePixel16);
            }
        }
        private void bitplaneToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                // oColorManager.WriteBitplane(sfdExportImage.FileName, ImageWidthAlignMode.MultiplePixel16, false);
            }
        }





        private void btnReduceColorsZx_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorManager.ProcessColors(ColorTransformType.ColorReductionZxSpectrum);
        }


        private void btnReduceColorsEga_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorManager.ProcessColors(ColorTransformType.ColorReductionEga);
        }

        private void BtnReduceColorsC64v1_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorManager.ProcessColors(ColorTransformType.ColorReductionCBM64);
        }

        private void btnReduceColorCPC_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorManager.ProcessColors(ColorTransformType.ColorReductionCPC);
        }

        private void btnChromaAdapt_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorManager.ProcessColors(ColorTransformType.ColorReductionSaturation);
        }

        private void btnReduceHam_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorManager.ProcessColors(ColorTransformType.ColorReductionHam);
        }

        bool bDitherStrenghtUpdating = false;
        private void tbDitherStrenght_Scroll(object sender, EventArgs e)
        {
            if (bDitherStrenghtUpdating)
                return;
            bDitherStrenghtUpdating = true;
            nudDitheringStrenght.Value = (decimal)tbDitherStrenght.Value / 100.0M;
            bDitherStrenghtUpdating = false;
        }

        private void nudDitheringStrenght_ValueChanged(object sender, EventArgs e)
        {
            if (bDitherStrenghtUpdating)
                return;
            bDitherStrenghtUpdating = true;
            tbDitherStrenght.Value = (int)Math.Max(0, Math.Min(100, nudDitheringStrenght.Value * 100));
            bDitherStrenghtUpdating = false;
        }

        private void gbLayoutV_CheckedChanged(object sender, EventArgs e)
        {
            splitRenderLayout.Orientation = Orientation.Vertical;
        }

        private void rbLayoutH_CheckedChanged(object sender, EventArgs e)
        {
            splitRenderLayout.Orientation = Orientation.Horizontal;
        }

        private void scMain_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (splitMain.Panel1.Width > Panel1MaxWidth)
            {
                splitMain.SplitterDistance = Panel1MaxWidth;
            }
        }

        private void scMain_SizeChanged(object sender, EventArgs e)
        {
            if (splitMain.Panel1.Width > Panel1MaxWidth)
            {
                splitMain.SplitterDistance = Panel1MaxWidth;
            }
        }

        private void oBitmapRenderSource_MouseMove(object sender, MouseEventArgs e)
        {
            var oPointC = oBitmapRenderSource.MouseControlCoordinatesX;
            tsMouseCoord.Text = $"X:{oPointC.X} Y:{oPointC.Y}";
            var oPointI = oBitmapRenderSource.MouseImageCoordinatesClip;
            tsImageCoord.Text = $"X:{oPointI.X:f1} Y:{oPointI.Y:f1}";
            tsColorData.BackColor = oBitmapRenderSource.MouseImageColor;
        }

    }
}
