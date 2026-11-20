using ColourClashNet.Color;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace ColourClashNet.Controls
{
    public partial class PaletteViewer : UserControl
    {
        public PaletteViewer()
        {
            InitializeComponent();
        }

        List<Panel> panels = new List<Panel>();

        const int pw = 8;
        const int ph = 8;

        void SetImage(int[,] matrix)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            var image = pictureBox1.Image;
            pictureBox1.Image = null;
            image?.Dispose();
            if (matrix != null)
            {
                pictureBox1.Image = ImageToolsGDI.MatrixToGdiImage(matrix);
            }
        }

        public void ResetPalette()
        {
            var matrix = MatrixTools.CreateMatrix(pw, ph, ColorIntExt.FromRGB(0,0,0));
            SetImage(matrix);
        }   

        public void SetPalette(Palette palette)
        {
            if(palette == null)
            {
                ResetPalette();
                return;
            }
            int colors = Math.Min(256, palette.Count);
            int maxarea = 0;
            int rr = 0;
            int cc = 0;
            if (colors <= 256)
            {
                cc = 32;
                rr = colors / 32;
                if (colors % 8!=0) rr++;
            }
            if (colors <= 128)
            {
                cc = 16;
                rr = (colors / 16);
                if (colors % 16 != 0) rr++;
            }
            if (colors <= 16)
            {
                cc = 4;
                rr = (colors/4);
                if (colors % 4 != 0) rr++;
            }
            if (colors <= 4)
            {
                rr = 1;
                cc = colors;
            }
            var matrix = MatrixTools.CreateMatrix(pw * cc, ph * rr, ColorIntExt.FromRGB(0, 0, 0));
            var rgbList = palette.ToList();
            var i = 0;
            for (int r = 0; r < rr ; r++)
            {
                if (i >= colors)
                    break;
                for (int c = 0; c < cc; c++, i++)
                {
                    if (i >= colors)
                        break;
                    var m = MatrixTools.CreateMatrix(pw, ph, rgbList[i]);
                    MatrixTools.Blit(m, matrix, new Rectangle<int>(0, 0, pw, ph), c * pw, r * ph);
                }
            }
            SetImage(matrix);
        }

        public void SetPalette(List<System.Drawing.Color> colorList)
        {
            ResetPalette();
            if (colorList == null)
            {
                return;
            }
            var pal = new Palette().Create();
            for (int i = 0; i < Math.Min(256, colorList.Count); i++)
            {
                pal.Add(ColorIntExtGdi.FromDrawingColor(colorList[i]));
            }
            SetPalette(pal);
        }

    }
}
