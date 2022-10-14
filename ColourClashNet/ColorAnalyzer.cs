using ColourClashNet.Colors;
using ColourClashNet.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            oColorTransformer.ColorBackgroundList = GetBkgColors();
            oColorTransformer.ColorQuantizationMode = GetQuantizationMode();
            cbImage.SelectedIndex = 2;
        }


        List<System.Drawing.Color> lBkgColor = new List<System.Drawing.Color>();
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
                oColorTransformer.ColorBackgroundList = new List<ColorItem> { ColorItem.FromDrawingColor(pbBkColor.BackColor, ColorQuantizationMode.RGB888) };
                oColorTransformer.ColorBackgroundReplacement = new ColorItem(0, 0, 0);
                oColorTransformer.ColorQuantizationMode = GetQuantizationMode();
                oColorTransformer.ColorDistanceEvaluationMode = GetColorDistanceMode();
                oColorTransformer.Create(oBmp);
            }
        }

        void Reprocess()
        {
            BuildBkgPalette();
            oColorTransformer.ColorBackgroundList = new List<ColorItem> { ColorItem.FromDrawingColor(pbBkColor.BackColor, ColorQuantizationMode.RGB888) };
            oColorTransformer.ColorBackgroundReplacement = new ColorItem(0, 0, 0);
            oColorTransformer.ColorQuantizationMode = GetQuantizationMode();
            oColorTransformer.ColorDistanceEvaluationMode = GetColorDistanceMode();
            oColorTransformer.ProcessBase();
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!fullColorToolStripMenuItem.Checked)
                return;
            if (e.Button == MouseButtons.Left)
            {
                using (Bitmap bmp = new Bitmap(pictureBoxSrc.ClientSize.Width, pictureBoxSrc.Height))
                {
                    pictureBoxSrc.DrawToBitmap(bmp, pictureBoxSrc.ClientRectangle);
                    try
                    {
                        lBkgColor.Add(bmp.GetPixel(e.X, e.Y));
                        Reprocess();
                    }
                    catch
                    {
                    }
                }
            }
        }


        List<ColorItem> GetBkgColors()
        {
            var oRet = new List<ColorItem>();
            foreach (var item in lBkgColor)
                oRet.Add(ColorItem.FromDrawingColor(item, ColorQuantizationMode.RGB888));
            return oRet;
        }

        ColorQuantizationMode GetQuantizationMode()
        {
            if (fullColorToolStripMenuItem.Checked)
                return ColorQuantizationMode.RGB888;
            if (rGB3ToolStripMenuItem.Checked)
                return ColorQuantizationMode.RGB333;
            if (rGB4ToolStripMenuItem.Checked)
                return ColorQuantizationMode.RGB444;
            if (rGB5ToolStripMenuItem.Checked)
                return ColorQuantizationMode.RGB555;
            if (hiColorToolStripMenuItem.Checked)
                return ColorQuantizationMode.RGB565;
            return ColorQuantizationMode.RGB888;
        }

        ColorDistanceEvaluationMode GetColorDistanceMode()
        {
            if (rGBToolStripMenuItem.Checked)
                return ColorDistanceEvaluationMode.RGB;
            if( hSVToolStripMenuItem.Checked)
                return ColorDistanceEvaluationMode.HSV;
            if (aLLToolStripMenuItem.Checked)
                return ColorDistanceEvaluationMode.All;
            return ColorDistanceEvaluationMode.RGB;
        }

        void ResetTsColor(object sender, EventArgs e)
        {
            ToolStripMenuItem oItem = sender as ToolStripMenuItem;
            if ( oItem != fullColorToolStripMenuItem )
                fullColorToolStripMenuItem.Checked = false;
            if (oItem != rGB3ToolStripMenuItem)
                rGB3ToolStripMenuItem.Checked = false;
            if (oItem != rGB4ToolStripMenuItem)
                rGB4ToolStripMenuItem.Checked = false;
            if (oItem != rGB5ToolStripMenuItem)
                rGB5ToolStripMenuItem.Checked = false;
            if (oItem != hiColorToolStripMenuItem)
                rGB5ToolStripMenuItem.Checked = false;
            oItem.Checked = true;
            Reprocess();
        }

        void ResetTsColorDistance(object sender, EventArgs e)
        {
            ToolStripMenuItem oItem = sender as ToolStripMenuItem;
            if (oItem != rGBToolStripMenuItem)
                rGBToolStripMenuItem.Checked = false;
            if (oItem != hSVToolStripMenuItem)
                hSVToolStripMenuItem.Checked = false;
            if (oItem != aLLToolStripMenuItem)
                aLLToolStripMenuItem.Checked = false;
            oItem.Checked = true;

            oColorTransformer.ColorDistanceEvaluationMode = GetColorDistanceMode();
        }

        private void btnReduceColors_Click(object sender, EventArgs e)
        {
            int iColors = (int)numericUpDown1.Value;
            oColorTransformer.ReduceColorsQuantity(iColors);
        }
        private void btnReduceColorsScanline_Click(object sender, EventArgs e)
        {
            int iColors = (int)numericUpDown1.Value;
            bool bCluster = chkScanLineCluster.Checked;
            oColorTransformer.ReduceColorsScanLine(iColors, bCluster);
        }

        private void btnReduceColorCluster_Click(object sender, EventArgs e)
        {
            int iColors = (int)numericUpDown1.Value;
            int iLoop = (int)nudClusterLoop.Value;
            oColorTransformer.ReduceColorsClustering(iColors,iLoop);
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
                ImageTools.ImageExport.Export(oColorTransformer.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageTools.ImageExportformat.Bmp24, oColorTransformer.Palette,  ImageTools.WidthAlignMode.None);
            }
        }

        private void pNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageTools.ImageExport.Export(oColorTransformer.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageTools.ImageExportformat.Png24, oColorTransformer.Palette,  ImageTools.WidthAlignMode.None);
            }
        }

        private void bitmapIndexedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdExportImage.ShowDialog() == DialogResult.OK)
            {
                ImageTools.ImageExport.Export(oColorTransformer.ImageProcessed as Bitmap, sfdExportImage.FileName, ImageTools.ImageExportformat.BmpIndex, oColorTransformer.Palette,  ImageTools.WidthAlignMode.None);
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
    }
}
