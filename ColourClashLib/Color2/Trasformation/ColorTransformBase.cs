using ColourClashLib.Color.Trasformation;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Abstract class to handle std color transformations
    /// </summary>
    public abstract partial class ColorTransformBase : ColorTransformInterface
    {
        static string sC = nameof(ColorTransformBase);

        #region events

        public event EventHandler Creating;
        public event EventHandler Created;
        public event EventHandler<ColorTransformEventArgs> Processing;
        public event EventHandler<ColorTransformEventArgs> ProcessAdvance;
        public event EventHandler<ColorTransformEventArgs> ProcessPartial;
        public event EventHandler<ColorTransformEventArgs> Processed;

        #endregion

        #region Property/Fields

        //-------------------------------------------------------------------------------------------------------------------------------
        public ColorTransformType Type { get; protected init; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; protected init; }

        //-------------------------------------------------------------------------------------------------------------------------------

        protected DataContainer fixedDataContainer = new DataContainer();
        protected DataContainer sourceDataContainer = new DataContainer();
        protected DataContainer processedDataContainer = new DataContainer();
        protected DataContainer outputDataContainer = new DataContainer();

        public int[,]? SourceData => sourceDataContainer.Data;
        public int[,]? ProcessedData => processedDataContainer.Data;
        public int[,]? OutputData => outputDataContainer.Data;


        public Palette FixedPalette
        {
            get => fixedDataContainer.ColorPalette;
            set => fixedDataContainer.ColorPalette = value;
        }

        public int FixedColors => FixedPalette.Count;

        public Palette SourcePalette
        {
            get => sourceDataContainer.ColorPalette;
//            set => inputData.ColorPalette = value;
        }

        public Histogram SourceHistogram
        {
            get => sourceDataContainer.ColorHistogram;
            //set
            //{
            //    inputData.ColorHistogram = value;
            //    inputData.ColorPalette = value.ToColorPalette();
            //}
        }

        public int SourceColors => SourcePalette.Count;

        public Histogram OutputHistogramX
        {
            get => outputDataContainer.ColorHistogram;
//            set => outputData.SetColorHistogramAsync(value,null).GetAwaiter().GetResult();
        }

        public Palette OutputPaletteX
        {
            get => processedDataContainer.ColorPalette;
//            set => processedData.ColorPalette = value;
        }

        public int OutputColorsX => OutputPaletteX.Count;

        //-------------------------------------------------------------------------------------------------------------------------------
        public ColorTransformationMap TransformationMap { get; protected set; } = new ColorTransformationMap();
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.RGB;

        //-------------------------------------------------------------------------------------------------------------------------------
        public bool BypassDithering { get; set; }

        public ColorDithering DitheringType { get; set; }

        #endregion

        #region abstract Methods

        protected async virtual Task<bool> CreateTrasformationMapAsync( CancellationToken? oToken)
        {
            return await Task.FromResult(true);
        }


        #endregion



        #region Create/Destroy

        void Reset()
        {
            sourceDataContainer.Reset();           
            processedDataContainer.Reset();   
            outputDataContainer.Reset();
            //only Set() can handle this. do not uncomment!
            //fixedDataContainer.Reset();
        }

        void CreateStart()
        {
            Reset();
            Creating?.Invoke(this, EventArgs.Empty);
        }
        ColorTransformBase CreateEnd()
        {
            Created?.Invoke(this, EventArgs.Empty);
            return this;
        }

        public async Task<ColorTransformInterface> CreateAsync(int[,]? oDataSource, CancellationToken? oToken)
        {
            string sM = nameof(CreateAsync);
            try
            {
                CreateStart();
                await sourceDataContainer.SetColorHistogramAsync(oDataSource, oToken);
                return CreateEnd();
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return this;
            }
        }

        public async Task<ColorTransformInterface> CreateAsync(Histogram oColorHistogramSource, CancellationToken? oToken)
        {
            string sM = nameof(CreateAsync);
            try
            {
                CreateStart();
                await sourceDataContainer.SetColorHistogramAsync(oColorHistogramSource, oToken);
                return CreateEnd();
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return this;
            }
        }

        public async Task<ColorTransformInterface> CreateAsync(Palette? oColorPaletteSource, CancellationToken? oToken)
        {
            string sM = nameof(CreateAsync);
            try
            {
                CreateStart();
                await sourceDataContainer.SetColorPaletteAsync(oColorPaletteSource);
                return CreateEnd();
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return this;
            }
        }

        //public ColorTransformInterface Create(int[,]? oDataSource)
        //{
        //    var cts = new CancellationTokenSource();    
        //    return CreateAsync(oDataSource,cts.Token).GetAwaiter().GetResult();
        //}

        //public ColorTransformInterface Create(Histogram? oColorHistogramSource)
        //{
        //    var cts = new CancellationTokenSource();
        //    return CreateAsync(oColorHistogramSource, cts.Token).GetAwaiter().GetResult();
        //}

        //public ColorTransformInterface Create(Palette? oColorPaletteSource)
        //{
        //    var cts = new CancellationTokenSource();
        //    return CreateAsync(oColorPaletteSource, cts.Token).GetAwaiter().GetResult();
        //}

        #endregion


        public virtual ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            switch (eProperty)
            {
                case ColorTransformProperties.ColorDistanceEvaluationMode:
                    if (Enum.TryParse<ColorDistanceEvaluationMode>(oValue?.ToString(), out var eMode))
                    {
                        ColorDistanceEvaluationMode = eMode;
                    }
                    break;
                case ColorTransformProperties.Fixed_Palette:
                    {
                        if (oValue is List<int> oPalette)
                        {
                            FixedPalette = Palette.CreateColorPalette(oPalette);
                        }
                        else if (oValue is Palette oPal)
                        {
                            FixedPalette = oPal;
                        }
                        else
                        {
                            FixedPalette = new Palette();
                        }
                    }
                    break;
                case ColorTransformProperties.Dithering_Model:
                    {
                        DitheringType = ColorDithering.None;
                        if (Enum.TryParse<ColorDithering>(oValue?.ToString(), true, out var eRes))
                        {
                            DitheringType = eRes;
                        }
                    }
                    break;
                default:
                    break;
            }
            return this;
        }

        internal ColorTransformEventArgs CreateTransformEventArgs(CancellationTokenSource oTokenSource, ColorTransformResults oResult)
            => new ColorTransformEventArgs()
            {
                ColorTransformInterface = this,
                CancellationTokenSource = oTokenSource,
                ProcessingResults = oResult,
                TransformationMap = this.TransformationMap  
            };


        protected abstract Task<ColorTransformResults> ExecuteTransformAsync( CancellationToken? oToken);

        //protected async Task<ColorTransformResults> ExecuteTransformAsync(int[,]? oDataSource, CancellationToken oToken)
        //{
        //    return await Task.Run(()=>new ColorTransformResults());
        //}

        #region Processing

        public async Task<ColorTransformResults> ProcessColorsAsync( CancellationToken? oTokenA)
        {
            string sM = nameof(CreateAsync);
            var oRet = new ColorTransformResults();
            try
            {
                if (SourceData == null)
                {
                    LogMan.Error(sC, sM, $"{Type} : sourceData Null");
                    return oRet;
                }

                //await CreateAsync(oDataSource,oTokenA);

                var cts = oTokenA != null ? CancellationTokenSource.CreateLinkedTokenSource(oTokenA.Value) : new CancellationTokenSource();
                var oToken= cts.Token;

                TransformationMap.Reset();
                if (!await CreateTrasformationMapAsync( oToken))
                {
                    LogMan.Error(sC, sM, $"{Type} : CreateTrasformationMapAsync Error");
                    return oRet;
                }
                Processing?.Invoke(this, CreateTransformEventArgs(cts, oRet));

                // Execute color reduction
                oRet = await ExecuteTransformAsync( oToken);
                if (!oRet.Valid)
                {
                    LogMan.Error(sC, sM, $"{Type} :  Transformation error");
                    return oRet;
                }
                {
                    var oHash = new HashSet<int>();
                    foreach (var rgb in oRet.DataOut)
                    {
                        if (rgb > 0)
                        {
                            oHash.Add(rgb);
                        }
                    }
                    await processedDataContainer.SetColorHistogramAsync(oRet.DataOut, oToken);
                    if (BypassDithering || oHash.Count > 256 || DitheringType == ColorDithering.None)
                    {
                        LogMan.Message(sC, sM, $"{Type} : Processing Completed - No dithering");
                        oRet.DataOut = oRet.DataProcessed.Clone() as int[,];
                        oRet.Valid = true;
                    }
                    else
                    {
                        var Dithering = DitherBase.CreateDitherInterface(DitheringType);                       
                        oRet.DataOut = await Dithering.DitherAsync( SourceData, processedDataContainer.Data, processedDataContainer.ColorPalette, ColorDistanceEvaluationMode, oToken);
                        if (oRet.DataOut == null)
                        {
                            LogMan.Error(sC, sM, $"{Type} :  Dithering error");
                        }
                        else
                        {
                            LogMan.Message(sC, sM, $"{Type} : Processing Completed with dithering");
                        }
                    }
                    await outputDataContainer.SetColorHistogramAsync(oRet.DataOut, oToken);
                    oRet.Valid = oRet.DataOut != null;  
                    Processed?.Invoke(this, CreateTransformEventArgs(cts, oRet));
                    return oRet;
                }
            }
            catch (ThreadInterruptedException exTh)
            {
                LogMan.Error(sC, sM, $"{Type} : Processing Interrupted");
                oRet.Valid = false;
                oRet.Message = exTh.Message;
                oRet.Exception = exTh;
                return oRet;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                oRet.Valid = false;
                oRet.Exception = ex;
                return oRet;
            }
        }

       

        //public ColorTransformResults ProcessColors(int[,]? oDataSource)
        //{
        //    var cts = new CancellationTokenSource();
        //    return ProcessColorsAsync(oDataSource, cts).GetAwaiter().GetResult();
        //}

        public async Task AbortProcessingAsync(CancellationTokenSource oTokenSource)
        {
            if (oTokenSource != null)
            {
                await oTokenSource.CancelAsync();
            }
        }
        public void AbortProcessing(CancellationTokenSource oTokenSource)
        {
            oTokenSource?.Cancel();
        }

        #endregion

    }
}

//    public abstract partial class ColorTransformBaseOLD : ColorTransformInterface
//    {

//        //---------------- Base description ---------------------------------
//        public ColorTransformType Type { get; protected init; }
//        public String Name { get; set; } = "";
//        public string Description { get; protected set; } = "";

//        //---------------- Source properties --------------------------------------
//        //public Histogram InputHistogram { get; protected set; } = new Histogram();
//        public Palette InputFixedColorPalette { get; protected set; } = new Palette();
//        protected int InputFixedColors => InputFixedColorPalette?.Count ?? 0;

//        //---------------- Transformation properties------------------------------
//        public Histogram OutputHistogram { get; protected set; } = new Histogram();
//        public Palette OutputPalette { get; set; } = new Palette();
//        public int OutputColors => OutputPalette?.Count ?? 0;
//        public ColorTransformationMap ColorTransformationMapper { get; protected set; } = new ColorTransformationMap();

//        //---------------- Useful objects ------------------------------
//        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.RGB;
//        public DitherInterface? Dithering { get; set; } = null;
//        protected bool BypassDithering { get; set; }
//        protected virtual void CreateTrasformationMap() { }

//        private object locker = new object();

//        private CancellationTokenSource? cancToken = null;

//        public bool ProcessAbort()
//        {
//            lock (locker)
//            {
//                if (cancToken == null)
//                {
//                    return false;
//                }
//                cancToken.Cancel();
//                return true;
//            }
//        }

//        protected virtual int[,]? ExecuteTransform(int[,]? oDataSource, CancellationToken oToken)
//        {
//            return ExecuteStdTransform(oDataSource, this, oToken);
//        }

//        void Reset()
//        {
//            OutputPalette.Reset();
//            OutputHistogram.Reset();
//            ColorTransformationMapper.Reset();
//        }


//        public ColorTransformInterface? Create(int[,]? oDataSource, Palette? oFixedColorPalette)
//        {
//            Reset();
//            if (oDataSource == null)
//            {
//                return null;
//            }
//            InputFixedColorPalette = oFixedColorPalette ?? new Palette();
//            OutputHistogram.Create(oDataSource);
//            OutputPalette = OutputHistogram.ToColorPalette();
//            CreateTrasformationMap();
//            return this;
//        }

//        public ColorTransformInterface? Create(Histogram oSourceColorHistogram, Palette? oFixedColorPalette)
//        {
//            Reset();
//            if (oSourceColorHistogram == null)
//            {
//                return null;
//            }
//            InputFixedColorPalette = oFixedColorPalette ?? new Palette();
//            foreach (var kvp in oSourceColorHistogram.rgbHistogram)
//            {
//                OutputHistogram.rgbHistogram.Add(kvp.Key, kvp.Value);
//            }
//            OutputPalette = OutputHistogram.ToColorPalette();
//            CreateTrasformationMap();
//            return this;
//        }

//        public ColorTransformInterface? Create(Palette oSourceColorPalette, Palette? oFixedColorPalette)
//        {
//            Reset();
//            if (oSourceColorPalette == null)
//            {
//                return null;
//            }
//            InputFixedColorPalette = oFixedColorPalette ?? new Palette();
//            foreach (var rgb in oSourceColorPalette.rgbPalette)
//            {
//                OutputHistogram.rgbHistogram.Add(rgb, 0);
//            }
//            OutputPalette = OutputHistogram.ToColorPalette();
//            CreateTrasformationMap();

          
//            return this;
//        }


//        public ColorTransformResults ProcessColors(int[,]? oDataSource)
//        {
//            string sMethod = nameof(ProcessColors);
//            var oRet = new ColorTransformResults()
//            {
//                DataSource = oDataSource,
//            };
//            lock (locker)
//            {
//                if (cancToken != null)
//                {
//                    string sMSG = $"{Type} : Processing already running";   
//                    LogMan.Error(sClass, sMethod, sMSG);
//                    oRet.AddMessage(sMSG);
//                    oRet.Valid = false;
//                    return oRet;
//                }
//                else
//                {
//                    LogMan.Debug(sClass, sMethod, $"{Type} : Generating internal CancellationTokenSource");
//                    cancToken = new CancellationTokenSource();
//                }
//            }
//            try
//            {
//                if (oDataSource == null)
//                {
//                    oRet.AddMessage($"{Type} : DataSource Null");
//                    return oRet;
//                }

//                // Execute color reduction
//                oRet.DataTemp = ExecuteTransform(oDataSource, cancToken.Token);
//                if (oRet.DataTemp == null)
//                {
//                    oRet.AddMessage($"{Type} : Transformation error");
//                    return oRet;
//                }
//                var oHash = new HashSet<int>();
//                foreach (var rgb in oRet.DataTemp)
//                {
//                    if (rgb < 0)
//                    {
//                        continue;
//                    }
//                    oHash.Add(rgb);
//                }
//                if (Dithering == null || BypassDithering || oHash.Count > 256)
//                {
//                    oRet.AddMessage($"{Type} : Processing Completed - No dithering");
//                    oRet.DataOut = oRet.DataTemp.Clone() as int[,];
//                    oRet.Valid = true;
//                    return oRet;
//                }
//                oRet.DataOut = Dithering.Dither(oDataSource, oRet.DataTemp, OutputPalette, ColorDistanceEvaluationMode, cancToken.Token);
//                if (oRet.DataOut == null)
//                {
//                    oRet.AddMessage($"{Type} : Dithering error");
//                    return oRet;
//                }
//                oRet.Valid = true;
//                oRet.AddMessage($"{Type} : Processing Completed");
//                return oRet;
//            }
//            catch (ThreadInterruptedException exTh)
//            {
//                oRet.AddMessage($"{Type} : Processing Interupted");
//                return oRet;
//            }
//            catch (Exception ex)
//            {
//                oRet.AddMessage($"{Type} : Exception Raised : {ex.Message}");
//                oRet.Exception = ex;
//                return oRet;
//            }
//            finally
//            {
//                cancToken = null;
//            }
//        }

//        public async Task<ColorTransformResults> ProcessColorsAsync(int[,]? oDataSource)
//        {
//            return await Task.Run(() => ProcessColors(oDataSource));
//        }

//        static public double EvaluateError(int[,]? oDataA, int[,]? oDataB, ColorDistanceEvaluationMode eMode)
//        {
//            if (oDataA == null || oDataB == null)
//            {
//                return double.NaN;
//            }
//            int r1 = oDataA.GetLength(0);
//            int c1 = oDataA.GetLength(1);
//            int r2 = oDataB.GetLength(0);
//            int c2 = oDataB.GetLength(1);
//            if (r1 != r2 || c1 != c2 || r1 == 0 || r2 == 0 )
//            {
//                return double.NaN;
//            }
//            double err = 0;
//            for (int r = 0; r < r1; r++)
//            {
//                for (int c = 0; c < c1; c++)
//                {
//                    var rgb1 = oDataA[r, c];
//                    var rgb2 = oDataB[r, c];
//                    if (rgb1 >= 0 && rgb2 >= 0)
//                    {
//                        err += ColorIntExt.Distance(rgb1, rgb2, eMode);
//                    }
//                }
//            }
//            err /= (3 * r1 * c1);
//           // err *= 100;
//            return err;
//        }


//        public virtual ColorTransformInterface? SetProperty(ColorTransformProperties eProperty, object oValue)
//        {
//            switch (eProperty)
//            {
//                case ColorTransformProperties.ColorDistanceEvaluationMode:
//                    if (Enum.TryParse<ColorDistanceEvaluationMode>(oValue?.ToString(), out var eMode))
//                    {
//                        ColorDistanceEvaluationMode = eMode;
//                        return this;
//                    }
//                    break;
//                case ColorTransformProperties.Output_Palette:
//                    {
//                        if (oValue is List<int> oPalette)
//                        {
//                            OutputPalette = Palette.CreateColorPalette(oPalette);
//                        }
//                        else if (oValue is Palette oPal)
//                        {
//                            OutputPalette = oPal;
//                        }
//                        else
//                        {
//                            OutputPalette = new Palette();
//                        }
//                        return this;
//                    }
//                    break;
//                default:
//                    break;
//            }
//            return null;
//        }

//        public virtual ColorTransformInterface? SetDithering(DitherInterface oDithering)
//        {
//            Dithering = oDithering;
//            return this;
//        }
//    }
//}
