using ColourClashNet.Colors;
using ColourClashNet.Colors.Transformation;
using ColourClashNet.Controls;
using ColourClashNet.ImageTools;
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
            oColorTransformer.BackgroundColorList = GetBkgColors();
            oColorTransformer.ColorQuantizationMode = ColorQuantizationMode.RGB888;//GetQuantizationMode();
            oColorTransformer.DitheringAlgorithm = ColorDithering.FloydSteinberg;
            pbBkColor.BackColor = Color.Transparent;
            InitMenu();
            CreateComboBox(cbC64VideoMode, Enum.GetNames(typeof(ColorTransformReductionC64.C64VideoMode)).ToList());
            CreateComboBox(cbCpcVideoMode, Enum.GetNames(typeof(ColorTransformReductionCPC.CPCVideoMode)).ToList());
            CreateComboBox(cbAmigaVideoMode, Enum.GetNames(typeof(ColorTransformReductionAmiga.EnumAMigaVideoMode)).ToList());
        }


        List<System.Drawing.Color> lBkgColor = new List<System.Drawing.Color>();
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
            RebulidSetCheck(oColorTransformer.ColorQuantizationMode.ToString());
            RebulidSetCheck(oColorTransformer.ColorDistanceEvaluationMode.ToString());
            RebulidSetCheck(oColorTransformer.DitheringAlgorithm.ToString());
        }

        private void TsItem_Click(object? sender, EventArgs e)
        {
            var oTS = sender as ToolStripMenuItem;
            var oTag = oTS.Tag;
            if (oTag is ColorQuantizationMode)
            {
                oColorTransformer.ColorQuantizationMode = (ColorQuantizationMode)oTag;
                oColorTransformer.ProcessBase();
            }
            else if (oTag is ColorDistanceEvaluationMode)
            {
                oColorTransformer.ColorDistanceEvaluationMode = (ColorDistanceEvaluationMode)oTag;
            }
            else if (oTag is ColorDithering)
            {
                oColorTransformer.DitheringAlgorithm = (ColorDithering)oTag;
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
            };
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
                    };
                }
                pbBkColor.SizeMode = PictureBoxSizeMode.StretchImage;
                pbBkColor.Image = oBmp;
            }
        }

        void ShowImage()
        {
            oBitmapRenderSource.Image = oColorTransformer.ImageSource;
            oBitmapRenderDest.Image = oColorTransformer.ImageProcessed;
            Invalidate();
        }

        void RefreshData()
        {
            propertyGrid1.SelectedObject = oColorTransformer;
            ShowImage();
        }


        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var oBmp = Bitmap.FromFile(openFileDialog1.FileName) as Bitmap;
                oColorTransformer.BackgroundColorList = new List<int> { ColorIntExt.FromDrawingColor(pbBkColor.BackColor) };
                oColorTransformer.BackgroundColorReplacement = 0;
                //   oColorTransformer.ColorQuantizationMode = GetQuantizationMode();
                //   oColorTransformer.ColorDistanceEvaluationMode = GetColorDistanceMode();
                oColorTransformer.Create(oBmp);
                oBitmapRenderDest.OriginZero();
                oBitmapRenderSource.OriginZero();
            }
        }

        void Reprocess()
        {
            BuildBkgPalette();
            oColorTransformer.BackgroundColorList = new List<int> { ColorIntExt.FromDrawingColor(pbBkColor.BackColor) };
            oColorTransformer.BackgroundColorReplacement = 0;
            // oColorTransformer.ColorQuantizationMode = GetQuantizationMode();
            // oColorTransformer.ColorDistanceEvaluationMode = GetColorDistanceMode();
            oColorTransformer.ProcessBase();
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //if (!fullColorToolStripMenuItem.Checked)
            //    return;
            //if (e.Button == MouseButtons.Left)
            //{
            //    using (Bitmap bmp = new Bitmap(pictureBoxSrc.ClientSize.Width, pictureBoxSrc.Height))
            //    {
            //        pictureBoxSrc.DrawToBitmap(bmp, pictureBoxSrc.ClientRectangle);
            //        try
            //        {
            //            lBkgColor.Add(bmp.GetPixel(e.X, e.Y));
            //            Reprocess();
            //        }
            //        catch
            //        {
            //        }
            //    }
            //}
        }


        List<int> GetBkgColors()
        {
            var oRet = new List<int>();
            foreach (var item in lBkgColor)
                oRet.Add(ColorIntExt.FromDrawingColor(item));
            return oRet;
        }

        ImageWidthAlignMode GetImageWidthAlignMode()
        {
            return ImageWidthAlignMode.MultiplePixel16;
        }



        private void SetToControl()
        {
            oColorTransformer.ColorsMax = (int)nudColorsWanted.Value;
            oColorTransformer.ScanlineClustering = chkScanLineCluster.Checked;
            oColorTransformer.ScanlineColorsMax = (int)nudScanlineLineColors.Value;
            oColorTransformer.ScanlineSharedPalette = chkScanlineSharedPal.Checked;

            oColorTransformer.ClusteringTrainingLoop = (int)nudClusterLoop.Value;
            oColorTransformer.ClusteringUseMeanColor = true;
            oColorTransformer.DitheringStrenght = (double)nudDitheringStrenght.Value;
            oColorTransformer.SaturationEnhancement = (double)nudSat.Value;
            oColorTransformer.BrightnessEnhancement = (double)nudBright.Value;
            oColorTransformer.HueOffset = (double)nudHue.Value;
            oColorTransformer.C64ScreenMode = (ColorTransformReductionC64.C64VideoMode)Enum.Parse(typeof(ColorTransformReductionC64.C64VideoMode), cbC64VideoMode.SelectedItem.ToString());
            oColorTransformer.CPCScreenMode = (ColorTransformReductionCPC.CPCVideoMode)Enum.Parse(typeof(ColorTransformReductionCPC.CPCVideoMode), cbCpcVideoMode.SelectedItem.ToString());
            oColorTransformer.ZxEqColorLO = (int)nudZxColorLO.Value;
            oColorTransformer.ZxEqColorHI = (int)nudZxColorHI.Value;
            oColorTransformer.ZxEqBlackHI = chkZxBlackHI.Checked;
            oColorTransformer.ZxEqDitherHI = chkZxDitherHI.Checked;
            oColorTransformer.AmigaVideoMode = (ColorTransformReductionAmiga.EnumAMigaVideoMode)Enum.Parse(typeof(ColorTransformReductionAmiga.EnumAMigaVideoMode), cbAmigaVideoMode.SelectedItem.ToString());
        }

        private void btnReduceColors_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorTransformer.ColorTranform(ColorTransformType.ColorReductionMedianCut);
        }
        private void btnReduceColorsScanline_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorTransformer.ColorTranform(ColorTransformType.ColorReductionScanline);
        }

        private void btnReduceColorCluster_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorTransformer.ColorTranform(ColorTransformType.ColorReductionClustering);
        }

        private void pbBkColor_DoubleClick(object sender, EventArgs e)
        {
            if (lBkgColor.Count > 0)
            {
                lBkgColor.RemoveAt(lBkgColor.Count - 1);
                Reprocess();
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            oColorTransformer.Reset();
        }

        private void toolStripMenuItemSave_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageTools.ImageTools.Export(oColorTransformer.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageTools.ImageExportFormat.Png24);
            }
        }


        private void oColorTransformer_OnReset(object sender, EventArgs e)
        {
            RefreshData();
        }


        private void bitmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageTools.ImageTools.Export(oColorTransformer.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageTools.ImageExportFormat.Bmp24);
            }
        }

        private void pNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageTools.ImageTools.Export(oColorTransformer.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageTools.ImageExportFormat.Png24);
            }
        }

        private void bitmapIndexedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                oColorTransformer.WriteBitmapIndex(sfdExportImage.FileName, GetImageWidthAlignMode());
            }
        }
        private void bitplaneToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                oColorTransformer.WriteBitplane(sfdExportImage.FileName, GetImageWidthAlignMode(), false);
            }
        }

        private void oColorTransformer_OnProcess(object sender, ColorTransformer.EventArgsTransformation e)
        {
            RefreshData();
        }

        private void oColorTransformer_OnQuantize(object sender, ColorTransformer.EventArgsTransformation e)
        {
            RefreshData();
        }

        private void oColorTransformer_OnCreate(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void btnReduceColorsZx_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorTransformer.ColorTranform(ColorTransformType.ColorReductionZxSpectrum);
        }


        private void btnReduceColorsEga_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorTransformer.ColorTranform(ColorTransformType.ColorReductionEga);
        }

        private void BtnReduceColorsC64v1_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorTransformer.ColorTranform(ColorTransformType.ColorReductionCBM64);
        }

        private void btnReduceColorCPC_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorTransformer.ColorTranform(ColorTransformType.ColorReductionCPC);
        }

        private void btnChromaAdapt_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorTransformer.ColorTranform(ColorTransformType.ColorReductionSaturation);
        }

        private void btnReduceHam_Click(object sender, EventArgs e)
        {
            SetToControl();
            oColorTransformer.ColorTranform(ColorTransformType.ColorReductionHam);
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
            scLayout.Orientation = Orientation.Vertical;
        }

        private void rbLayoutH_CheckedChanged(object sender, EventArgs e)
        {
            scLayout.Orientation = Orientation.Horizontal;
        }

        private void scMain_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (scMain.Panel1.Width > Panel1MaxWidth)
            {
                scMain.SplitterDistance = Panel1MaxWidth;
            }
        }

        private void scMain_SizeChanged(object sender, EventArgs e)
        {
            if (scMain.Panel1.Width > Panel1MaxWidth)
            {
                scMain.SplitterDistance = Panel1MaxWidth;
            }
        }


    }
}
