using ColourClashNet.Color;
using ColourClashNet.Color.Trasformation;
using ColourClashNet.Color.Dithering;
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
using ColourClashNet.Log;

namespace ColourClashNet.Color.Transformation
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
        //public Histogram InputHistogram { get; protected set; } = new Histogram();
        public Palette InputFixedColorPalette { get; protected set; } = new Palette();
        protected int InputFixedColors => InputFixedColorPalette?.Count ?? 0;

        //---------------- Transformation properties------------------------------
        public Histogram OutputHistogram { get; protected set; } = new Histogram();
        public Palette OutputPalette { get; set; } = new Palette();
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


        public ColorTransformInterface? Create(int[,]? oDataSource, Palette? oFixedColorPalette)
        {
            Reset();
            if (oDataSource == null)
            {
                return null;
            }
            InputFixedColorPalette = oFixedColorPalette ?? new Palette();
            OutputHistogram.Create(oDataSource);
            OutputPalette = OutputHistogram.ToColorPalette();
            CreateTrasformationMap();
            return this;
        }

        public ColorTransformInterface? Create(Histogram oSourceColorHistogram, Palette? oFixedColorPalette)
        {
            Reset();
            if (oSourceColorHistogram == null)
            {
                return null;
            }
            InputFixedColorPalette = oFixedColorPalette ?? new Palette();
            foreach (var kvp in oSourceColorHistogram.rgbHistogram)
            {
                OutputHistogram.rgbHistogram.Add(kvp.Key, kvp.Value);
            }
            OutputPalette = OutputHistogram.ToColorPalette();
            CreateTrasformationMap();
            return this;
        }

        public ColorTransformInterface? Create(Palette oSourceColorPalette, Palette? oFixedColorPalette)
        {
            Reset();
            if (oSourceColorPalette == null)
            {
                return null;
            }
            InputFixedColorPalette = oFixedColorPalette ?? new Palette();
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
            string sMethod = nameof(ProcessColors);
            var oRet = new ColorTransformResults()
            {
                DataSource = oDataSource,
            };
            lock (locker)
            {
                if (cancToken != null)
                {
                    string sMSG = $"{Type} : Processing already running";   
                    LogMan.Error(sClass, sMethod, sMSG);
                    oRet.AddMessage(sMSG);
                    oRet.Valid = false;
                    return oRet;
                }
                else
                {
                    LogMan.Debug(sClass, sMethod, $"{Type} : Generating internal CancellationTokenSource");
                    cancToken = new CancellationTokenSource();
                }
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
                var oHash = new HashSet<int>();
                foreach (var rgb in oRet.DataTemp)
                {
                    if (rgb < 0)
                    {
                        continue;
                    }
                    oHash.Add(rgb);
                }
                if (Dithering == null || BypassDithering || oHash.Count > 256)
                {
                    oRet.AddMessage($"{Type} : Processing Completed - No dithering");
                    oRet.DataOut = oRet.DataTemp.Clone() as int[,];
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
                            OutputPalette = Palette.CreateColorPalette(oPalette);
                        }
                        else if (oValue is Palette oPal)
                        {
                            OutputPalette = oPal;
                        }
                        else
                        {
                            OutputPalette = new Palette();
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
