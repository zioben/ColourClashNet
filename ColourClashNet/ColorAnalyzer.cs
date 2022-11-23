using ColourClashNet.Colors;
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
        public ColorAnalyzer()
        {
            InitializeComponent();
            oColorTransformer.BackgroundColorList = GetBkgColors();
            oColorTransformer.ColorQuantizationMode = ColorQuantizationMode.RGB888;//GetQuantizationMode();
            cbImage.SelectedIndex = 2;
            pbBkColor.BackColor = Color.Transparent;
            InitMenu();
        }


        List<System.Drawing.Color> lBkgColor = new List<System.Drawing.Color>();
        List<ToolStripMenuItem>lTsItems= new List<ToolStripMenuItem>();

        void RebulidSetCheck(string sItem)
        {
            var ts = lTsItems.FirstOrDefault(X => X.Tag.ToString() == sItem );
            if( ts != null)
                ts.Checked = true;  
        }

        void RebuildChecks()
        {
            lTsItems.ForEach(X=>X.Checked = false);
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


        void CreateMenuItem(ToolStripMenuItem oTsBase, object oItem )
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

        void InitMenu<T>(ToolStripMenuItem oTsBase) where T : System.Enum
        {
             var lColorMode = Enum.GetValues(typeof(T));
            foreach(var X in lColorMode )
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
                Bitmap oBmp = new Bitmap(lBkgColor.Count*16, 16);
                using (var g = Graphics.FromImage(oBmp))
                {
                    int index = 0;
                    for( int i = 0; i < lBkgColor.Count; i++ )
                    {
                        var item = lBkgColor[i];
                        using (var oBrush = new SolidBrush(item))
                        {
                            g.FillRectangle(oBrush, i*16, 0, 16, 16);
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
            var id = cbImage.SelectedIndex;
            oBitmapRenderSource.Image = oColorTransformer.ImageSource;
            switch (id)
            {
                case 1:
                    oBitmapRenderDest.Image = oColorTransformer.ImageQuantized;
                    break;
                default:
                    oBitmapRenderDest.Image = oColorTransformer.ImageProcessed; 
                    break;
            }
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

        //ColorQuantizationMode GetQuantizationMode()
        //{
        //    if (fullColorToolStripMenuItem.Checked)
        //        return ColorQuantizationMode.RGB888;
        //    if (rGB3ToolStripMenuItem.Checked)
        //        return ColorQuantizationMode.RGB333;
        //    if (rGB4ToolStripMenuItem.Checked)
        //        return ColorQuantizationMode.RGB444;
        //    if (rGB5ToolStripMenuItem.Checked)
        //        return ColorQuantizationMode.RGB555;
        //    if (hiColorToolStripMenuItem.Checked)
        //        return ColorQuantizationMode.RGB565;
        //    return ColorQuantizationMode.RGB888;
        //}

        //ColorDistanceEvaluationMode GetColorDistanceMode()
        //{
        //    if (rGBToolStripMenuItem.Checked)
        //        return ColorDistanceEvaluationMode.RGB;
        //    if( hSVToolStripMenuItem.Checked)
        //        return ColorDistanceEvaluationMode.HSV;
        //    if (aLLToolStripMenuItem.Checked)
        //        return ColorDistanceEvaluationMode.All;
        //    return ColorDistanceEvaluationMode.RGB;
        //}

        //void ResetTsColor(object sender, EventArgs e)
        //{
        //    ToolStripMenuItem oItem = sender as ToolStripMenuItem;
        //    if ( oItem != fullColorToolStripMenuItem )
        //        fullColorToolStripMenuItem.Checked = false;
        //    if (oItem != rGB3ToolStripMenuItem)
        //        rGB3ToolStripMenuItem.Checked = false;
        //    if (oItem != rGB4ToolStripMenuItem)
        //        rGB4ToolStripMenuItem.Checked = false;
        //    if (oItem != rGB5ToolStripMenuItem)
        //        rGB5ToolStripMenuItem.Checked = false;
        //    if (oItem != hiColorToolStripMenuItem)
        //        rGB5ToolStripMenuItem.Checked = false;
        //    oItem.Checked = true;
        //    Reprocess();
        //}

        //void ResetTsColorDistance(object sender, EventArgs e)
        //{
        //    ToolStripMenuItem oItem = sender as ToolStripMenuItem;
        //    if (oItem != rGBToolStripMenuItem)
        //        rGBToolStripMenuItem.Checked = false;
        //    if (oItem != hSVToolStripMenuItem)
        //        hSVToolStripMenuItem.Checked = false;
        //    if (oItem != aLLToolStripMenuItem)
        //        aLLToolStripMenuItem.Checked = false;
        //    oItem.Checked = true;

        //    oColorTransformer.ColorDistanceEvaluationMode = GetColorDistanceMode();
        //}

        private void btnReduceColors_Click(object sender, EventArgs e)
        {
            oColorTransformer.ColorsMax = (int)numericUpDown1.Value;
            oColorTransformer.ColorTranform(ColorTransform.ColorReductionMedianCut);
        }
        private void btnReduceColorsScanline_Click(object sender, EventArgs e)
        {
            oColorTransformer.ColorsMax = (int)numericUpDown1.Value;
            oColorTransformer.ScanlineClustering = chkScanLineCluster.Checked;
            oColorTransformer.ColorTranform(ColorTransform.ColorReductionScanline);
        }

        private void btnReduceColorCluster_Click(object sender, EventArgs e)
        {
            oColorTransformer.ColorsMax = (int)numericUpDown1.Value;
            oColorTransformer.ClusteringTrainingLoop = (int)nudClusterLoop.Value;
            oColorTransformer.ClusteringUseMeanColor = true;
            oColorTransformer.ColorTranform(ColorTransform.ColorReductionClustering);
        }

        private void cbImage_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowImage();
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

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

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

        private void button1_Click(object sender, EventArgs e)
        {
            oColorTransformer.ZxEqColorLO = (int)numericUpDownZXL.Value;
            oColorTransformer.ZxEqColorHI = (int)numericUpDownZXH.Value;
            oColorTransformer.ColorTranform( ColorTransform.ColorReductionZxSpectrum );
        }

        private void ColorAnalyzer_Load(object sender, EventArgs e)
        {
            oColorTransformer.DitheringAlgorithm = ColorDithering.FloydSteinberg;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            oColorTransformer.ColorTranform(ColorTransform.ColorReductionEga);
        }
    }
}
