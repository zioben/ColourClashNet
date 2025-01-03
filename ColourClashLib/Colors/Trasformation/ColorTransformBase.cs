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
        protected int FixedColors => FixedColorPalette?.Count ?? 0;

        //---------------- Transformation properties------------------------------
        public ColorHistogram Histogram { get; protected set; } = new ColorHistogram();
        public ColorPalette Palette { get; set; } = new ColorPalette();
        public int Colors => Palette?.Count ?? 0;
        public ColorTransformationMap ColorTransformationMapper { get; protected set; } = new  ColorTransformationMap();

        //---------------- Useful objects ------------------------------
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.RGB;
        public DitherInterface? Dithering { get; set; } = null;
        protected bool BypassDithering { get; set; }
        protected virtual void CreateTrasformationMap() { }

        private object locker = new object();   

        private CancellationTokenSource cancToken;

        public bool TransformAbort()
        {
            lock (locker)
            {
                if (cancToken == null)
                {
                    return false;
                }
                cancToken.Cancel();
                return true;
            }
        }

        protected virtual int[,]? ExecuteTransform(int[,]? oDataSource, CancellationToken oToken)
        {
            return ExecuteStdTransform(oDataSource,this, oToken);
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

        public ColorTransformResults TransformAndDither(int[,]? oDataSource) => TransformAndDitherAsync(oDataSource).Result;

        public async Task<ColorTransformResults> TransformAndDitherAsync(int[,]? oDataSource)
        {
            return await Task.Run(() =>
            {
                var oRet = new ColorTransformResults()
                {
                    DataSource = oDataSource,
                };
                lock (locker)
                {
                    if (cancToken != null)
                    {
                        oRet.AddMessage($"{Name} : Processing already running");
                        return oRet;
                    }
                    cancToken = new CancellationTokenSource();
                }
                try
                {
                    if (oDataSource == null)
                    {
                        oRet.AddMessage($"{Name} : DataSource Null");
                        return oRet;
                    }

                    oRet.DataTemp = ExecuteTransform(oDataSource, cancToken.Token);
                    if (oRet.DataTemp == null)
                    {
                        oRet.AddMessage($"{Name} : Transformation error");
                        return oRet;
                    }
                    if (Dithering == null || BypassDithering)
                    {
                        oRet.DataOut = oRet.DataTemp;
                        oRet.Valid = true;
                        oRet.AddMessage($"{Name} : Valid");
                        return oRet;
                    }
                    var oh = new HashSet<int>();
                    foreach (var rgb in oRet.DataTemp)
                    {
                        oh.Add(rgb);
                    }
                    if (oh.Count >= 256)
                    {
                        oRet.AddMessage($"{Name} : Processing Completed");
                        oRet.Valid = true;
                        return oRet;
                    }
                    oRet.DataOut = Dithering.Dither(oDataSource, oRet.DataTemp, Palette, ColorDistanceEvaluationMode, cancToken.Token);
                    if (oRet.DataOut == null)
                    {
                        oRet.AddMessage($"{Name} : Dithering error");
                        return oRet;
                    }
                    oRet.Valid = true;
                    oRet.AddMessage($"{Name} : Processing Completed");
                    return oRet;
                }
                catch (ThreadInterruptedException exTh)
                {
                    oRet.AddMessage($"{Name} : Processing Interupted");
                    return oRet;
                }
                catch (Exception ex)
                {
                    oRet.AddMessage($"{Name} : Exception Raised : {ex.Message}");
                    oRet.Exception = ex;
                    return oRet;
                }
                finally
                {
                    cancToken = null;
                }
            });
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
