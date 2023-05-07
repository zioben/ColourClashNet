using ColourClashLib.Color;
using ColourClashNet.Colors.Dithering;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace ColourClashNet.Colors.Transformation
{
    public abstract partial class ColorTransformBase : ColorTransformInterface
    {
        //---------------- Base description ---------------------------------
        public ColorTransformType Name { get; protected init; }
        public string Description { get; protected set; } = "";

        //---------------- Source properties --------------------------------------
        public ColorHistogram SourceColorHistogram { get; protected set; } = new ColorHistogram();
        public ColorPalette FixedColorPalette { get; protected set; } = new ColorPalette();
        protected int FixedColors => FixedColorPalette?.Colors ?? 0;

        //---------------- Transformation properties------------------------------
        public ColorHistogram Histogram { get; protected set; } = new ColorHistogram();
        public ColorPalette Palette { get; set; } = new ColorPalette();
        public int Colors => Palette?.Colors ?? 0;
        public ColorTransformationMap ColorTransformationMapper { get; protected set; } = new  ColorTransformationMap();

        //---------------- Useful objects ------------------------------
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.RGB;
        public DitherInterface? Dithering { get; set; } = null;
        protected bool BypassDithering { get; set; }
        protected abstract void CreateTrasformationMap();

        protected virtual int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            return ExecuteStdTransform(oDataSource,this);
        }

        void Reset()
        {
            Palette.Reset();
            Histogram.Reset();
            ColorTransformationMapper.Reset();
        }


        public ColorTransformInterface? Create(int[,]? oDataSource, ColorPalette? oFixedColorPalette )
        {
            Reset();
            if (oDataSource == null)
            {
                return null;
            }
            FixedColorPalette = oFixedColorPalette;
            Histogram.Create(oDataSource);
            Palette = Histogram.ToColorPalette();
            CreateTrasformationMap();
            return this;    
        }

        public ColorTransformInterface? Create(ColorHistogram oSourceColorHistogram, ColorPalette? oFixedColorPalette)
        {
            Reset();
            if (oSourceColorHistogram == null )
            {
                return null;
            }
            FixedColorPalette = oFixedColorPalette ?? new ColorPalette();
            foreach (var kvp in oSourceColorHistogram.rgbHistogram)
            {
                Histogram.rgbHistogram.Add(kvp.Key, kvp.Value);
            }
            Palette = Histogram.ToColorPalette();
            CreateTrasformationMap();
            return this;    
        }

        public ColorTransformInterface? Create(ColorPalette oSourceColorPalette, ColorPalette? oFixedColorPalette)
        {
            Reset();
            if (oSourceColorPalette == null)
            {
                return null;
            }
            FixedColorPalette = oFixedColorPalette;
            foreach (var rgb in oSourceColorPalette.rgbPalette)
            {
                Histogram.rgbHistogram.Add(rgb, 0);
            }
            Palette = Histogram.ToColorPalette();
            CreateTrasformationMap();
            return this;
        }

        public int[,]? TransformAndDither(int[,]? oDataSource)
        {
            if (oDataSource == null)
            {
                return null;
            }
            var oDataTrasf = ExecuteTransform(oDataSource);
            if (oDataTrasf == null || Dithering == null || BypassDithering)
            {
                return oDataTrasf;
            }
            var oh = new HashSet<int>();
            foreach (var rgb in oDataTrasf)
            {
                oh.Add(rgb);
            }
            if( oh.Count >= 256 ) 
            {
                return oDataTrasf;
            }
            var oProcDither = Dithering.Dither(oDataSource, oDataTrasf, Palette, ColorDistanceEvaluationMode);
            return oProcDither;
        }

        static public double Error(int[,]? oDataA, int[,]? oDataB, ColorDistanceEvaluationMode eMode)
        {
            if (oDataA == null || oDataB == null)
            {
                return double.NaN;
            }
            int r1 = oDataA.GetLength(0);
            int c1 = oDataA.GetLength(1);
            int r2 = oDataB.GetLength(0);
            int c2 = oDataB.GetLength(1);
            if (r1 != r2 || c1 != c2)
            {
                return double.NaN;
            }
            double err = 0;
            for( int r=0; r<r1; r++ ) 
            {
                for (int c = 0; c < c1; c++)
                {
                    var rgb1 = oDataA[r, c];
                    var rgb2 = oDataB[r, c];
                    if( rgb1 >=0 && rgb2 >=0 ) 
                    {
                        err += ColorIntExt.Distance(rgb1, rgb2, eMode);
                    }
                }
            }
            return err; 
        }


        protected int[,] HalveHorizontalRes(int[,]? oTmpDataSource)
        {
            if (oTmpDataSource == null)
                return null;
            var R = oTmpDataSource.GetLength(0);
            var C = oTmpDataSource.GetLength(1);
            var oRet = new int[R, (C + 1) / 2];
            Parallel.For(0, R, r =>
            {
                for (int c = 0, co = 0; c < C; c += 2, co++)
                {
                    if (c < C - 1)
                    {
                        var a = oTmpDataSource[r, c];
                        var b = oTmpDataSource[r, c + 1];
                        oRet[r, co] = ColorIntExt.GetColorMean(a, b);
                    }
                }
            });
            return oRet;
        }


        protected int[,] DoubleHorizontalRes(int[,]? oTmpDataSource)
        {
            if (oTmpDataSource == null)
                return null;
            var R = oTmpDataSource.GetLength(0);
            var C = oTmpDataSource.GetLength(1);
            var oRet = new int[R, C * 2];
            Parallel.For(0, R, r =>
            {
                for (int c = 0, co = 0; c < C; c++)
                {
                    var a = oTmpDataSource[r, c];
                    oRet[r, co++] = a;
                    oRet[r, co++] = a;
                }
            });
            return oRet;
        }

        public virtual ColorTransformInterface? SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            switch (eProperty)
            {
                case ColorTransformProperties.ColorDistanceEvaluationMode:
                    if (Enum.TryParse<ColorDistanceEvaluationMode>(oValue?.ToString(), out var eMode))
                    {
                        ColorDistanceEvaluationMode = eMode;
                        return this;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

    }
}
