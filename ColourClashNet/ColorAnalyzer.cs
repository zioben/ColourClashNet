using ColourClashNet.Color;
using ColourClashNet.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            cbImage.SelectedIndex = 2;
            pbBkColor.BackColor = System.Drawing.Color.Transparent;
        }

        void ShowImage()
        {
            var id = cbImage.SelectedIndex;
            switch (id)
            {
                case 0:
                    oBitmapRender.Image = oColorTransformer.ImageSource;
                    break;
                case 1:
                    oBitmapRender.Image = oColorTransformer.ImageQuantized;
                    break;
                case 2:
                    oBitmapRender.Image = oColorTransformer.ImageProcessed;
                    break;
                default: break;
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
                oColorTransformer.Create(
                    oBmp, 
                    ColorItem.FromDrawingColor(pbBkColor.BackColor, ColorQuantizationMode.RGB888), 
                    new ColorItem(), 
                    ColorQuantizationMode.RGB888, 
                    GetColorDistanceMode());

                RefreshData();
            }
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                using (Bitmap bmp = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.Height))
                {
                    pictureBox1.DrawToBitmap(bmp, pictureBox1.ClientRectangle);
                    try
                    {
                        pbBkColor.BackColor = bmp.GetPixel(e.X, e.Y);
                    }
                    catch
                    {
                        pbBkColor.BackColor = System.Drawing.Color.Transparent;
                    }
                    bmp.Dispose();
                }
            }
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

            oColorTransformer.Quantize( GetQuantizationMode() );
            RefreshData();
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
            RefreshData();
        }
        private void btnReduceColorsScanline_Click(object sender, EventArgs e)
        {
            int iColors = (int)numericUpDown1.Value;
            bool bCluster = chkScanLineCluster.Checked;
            oColorTransformer.ReduceColorsScanLine(iColors, bCluster);
            RefreshData();
        }

        private void btnReduceColorCluster_Click(object sender, EventArgs e)
        {
            int iColors = (int)numericUpDown1.Value;
            int iLoop = (int)nudClusterLoop.Value;
            oColorTransformer.ReduceColorsClustering(iColors,iLoop);
            RefreshData();
        }

        private void cbImage_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowImage();
        }

        private void pbBkColor_DoubleClick(object sender, EventArgs e)
        {
            pbBkColor.BackColor = System.Drawing.Color.Transparent;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }
    }
}
