using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ColourClashNet.Colors
{
    public static class ColorIntExt
    {
        public static System.Drawing.Color ToDrawingColor(this int i)
        {
            if (i < 0)
                return System.Drawing.Color.Transparent;
            unchecked
            {
                return System.Drawing.Color.FromArgb(i | (int)0xFF_00_00_00);
            }
        }

        public static int ToR(this int i)
        {
            return i >= 0 ? (i >> 16) & 0x00ff : -1;
        }
        public static int ToG(this int i)
        {
            return i >= 0 ? (i >> 8) & 0x00ff : -1;
        }
        public static int ToB(this int i)
        {
            return i >= 0 ? (i >> 0) & 0x00ff : -1;
        }


        public static double Distance(this int i,  int j, ColorDistanceEvaluationMode eMode)
        {
            if (i < 0 || j < 0)
                return double.PositiveInfinity;
            int R = i.ToR() - j.ToR();
            int G = i.ToG() - j.ToG();
            int B = i.ToB() - j.ToB();
            return R * R + G * G + B * B;
        }

        public static int FromDrawingColor(System.Drawing.Color oColor)
        {
            if (oColor == System.Drawing.Color.Transparent)
                return -1;
            return (oColor.R << 24) | (oColor.G << 16) | (oColor.B);
        }

        

    }
}
