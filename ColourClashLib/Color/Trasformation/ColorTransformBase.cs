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
    /// <summary>
    /// Abstract class to handle std color transformations
    /// </summary>
    public abstract partial class ColorTransformBase : ColorTransformInterface
    {
        //---------------- Base description ---------------------------------
        public ColorTransformType Type { get; protected init; }
        public String Name { get; set; } = "";
        public string Description { get; protected set; } = "";

        //---------------- Source properties --------------------------------------
        public ColorHistogram InputHistogram { get; protected set; } = new ColorHistogram();
        public ColorPalette InputFixedColorPalette { get; protected set; } = new ColorPalette();
        protected int InputFixedColors => InputFixedColorPalette?.Count ?? 0;

        //---------------- Transformation properties------------------------------
        public ColorHistogram OutputHistogram { get; protected set; } = new ColorHistogram();
        public ColorPalette OutputPalette { get; set; } = new ColorPalette();
        public int OutputColors => OutputPalette?.Count ?? 0;
        public ColorTransformationMap ColorTransformationMapper { get; protected set; } = new ColorTransformationMap();

        //---------------- Useful objects ------------------------------
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.RGB;
        public DitherInterface? Dithering { get; set; } = null;
        protected bool BypassDithering { get; set; }
        protected virtual void CreateTrasformationMap() { }

        private object locker = new object();

        private CancellationTokenSource? cancToken = null;

        public bool ProcessAbort()
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
            return ExecuteStdTransform(oDataSource, this, oToken);
        }

        void Reset()
        {
            OutputPalette.Reset();
            OutputHistogram.Reset();
            ColorTransformationMapper.Reset();
        }


        public ColorTransformInterface? Create(int[,]? oDataSource, ColorPalette? oFixedColorPalette)
        {
            Reset();
            if (oDataSource == null)
            {
                return null;
            }
            InputFixedColorPalette = oFixedColorPalette ?? new ColorPalette();
            OutputHistogram.Create(oDataSource);
            OutputPalette = OutputHistogram.ToColorPalette();
            CreateTrasformationMap();
            return this;
        }

        public ColorTransformInterface? Create(ColorHistogram oSourceColorHistogram, ColorPalette? oFixedColorPalette)
        {
            Reset();
            if (oSourceColorHistogram == null)
            {
                return null;
            }
            InputFixedColorPalette = oFixedColorPalette ?? new ColorPalette();
            foreach (var kvp in oSourceColorHistogram.rgbHistogram)
            {
                OutputHistogram.rgbHistogram.Add(kvp.Key, kvp.Value);
            }
            OutputPalette = OutputHistogram.ToColorPalette();
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
            InputFixedColorPalette = oFixedColorPalette ?? new ColorPalette();
            foreach (var rgb in oSourceColorPalette.rgbPalette)
            {
                OutputHistogram.rgbHistogram.Add(rgb, 0);
            }
            OutputPalette = OutputHistogram.ToColorPalette();
            CreateTrasformationMap();

          
            return this;
        }


        public ColorTransformResults ProcessColors(int[,]? oDataSource)
        {
            var oRet = new ColorTransformResults()
            {
                DataSource = oDataSource,
            };
            lock (locker)
            {
                if (cancToken != null)
                {
                    oRet.AddMessage($"{Type} : Processing already running");
                    return oRet;
                }
                cancToken = new CancellationTokenSource();
            }
            try
            {
                if (oDataSource == null)
                {
                    oRet.AddMessage($"{Type} : DataSource Null");
                    return oRet;
                }

                // Execute color reduction
                oRet.DataTemp = ExecuteTransform(oDataSource, cancToken.Token);
                if (oRet.DataTemp == null)
                {
                    oRet.AddMessage($"{Type} : Transformation error");
                    return oRet;
                }
                if (Dithering == null || BypassDithering)
                {
                    oRet.DataOut = oRet.DataTemp;
                    oRet.Valid = true;
                    oRet.AddMessage($"{Type} : Valid");
                    return oRet;
                }
                var oHash = new HashSet<int>();
                foreach (var rgb in oRet.DataTemp)
                {
                    oHash.Add(rgb);
                }
                if (oHash.Count >= 256)
                {
                    oRet.AddMessage($"{Type} : Processing Completed");
                    oRet.Valid = true;
                    return oRet;
                }
                oRet.DataOut = Dithering.Dither(oDataSource, oRet.DataTemp, OutputPalette, ColorDistanceEvaluationMode, cancToken.Token);
                if (oRet.DataOut == null)
                {
                    oRet.AddMessage($"{Type} : Dithering error");
                    return oRet;
                }
                oRet.Valid = true;
                oRet.AddMessage($"{Type} : Processing Completed");
                return oRet;
            }
            catch (ThreadInterruptedException exTh)
            {
                oRet.AddMessage($"{Type} : Processing Interupted");
                return oRet;
            }
            catch (Exception ex)
            {
                oRet.AddMessage($"{Type} : Exception Raised : {ex.Message}");
                oRet.Exception = ex;
                return oRet;
            }
            finally
            {
                cancToken = null;
            }
        }

        public async Task<ColorTransformResults> ProcessColorsAsync(int[,]? oDataSource)
        {
            return await Task.Run(() => ProcessColors(oDataSource));
        }

        static public double EvaluateError(int[,]? oDataA, int[,]? oDataB, ColorDistanceEvaluationMode eMode)
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
            for (int r = 0; r < r1; r++)
            {
                for (int c = 0; c < c1; c++)
                {
                    var rgb1 = oDataA[r, c];
                    var rgb2 = oDataB[r, c];
                    if (rgb1 >= 0 && rgb2 >= 0)
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
                case ColorTransformProperties.Output_Palette:
                    {
                        if (oValue is List<int> oPalette)
                        {
                            OutputPalette = ColorPalette.FromList(oPalette);
                        }
                        else if (oValue is ColorPalette oPal)
                        {
                            OutputPalette = oPal;
                        }
                        else
                        {
                            OutputPalette = new ColorPalette();
                        }
                        return this;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

        public virtual ColorTransformInterface? SetDithering(DitherInterface oDithering)
        {
            Dithering = oDithering;
            return this;
        }
    }
}
