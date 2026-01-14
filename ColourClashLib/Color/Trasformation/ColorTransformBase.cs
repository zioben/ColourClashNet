using ColourClashLib.Color.Trasformation;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Imaging;
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

        public string Description { get; protected init; } = "";

        //-------------------------------------------------------------------------------------------------------------------------------

        public ImageData InputData { get; protected set; } = new ImageData();
        protected ImageData MiddleData { get; set; } = new ImageData();
        public ImageData OutputData { get; protected set; } = new ImageData();

        public Palette FixedPalette { get; protected set; } = new Palette();
        public int FixedColors => FixedPalette?.Count??0;
        public bool FastPreview { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------
        public ColorTransformationMap TransformationMap { get; protected set; } = new ColorTransformationMap();
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.RGB;
        public double TransformationError { get; set; } = double.NaN;

        //-------------------------------------------------------------------------------------------------------------------------------
        public bool BypassDithering { get; set; }

        public ColorDithering DitheringType { get; set; }

        public double DitheringStrenght { get; set; } = 1.0;

        #endregion

        #region abstract Methods

        protected async virtual Task<ColorTransformResults> CreateTrasformationMapAsync( CancellationToken? oToken)
        {
            return await Task.FromResult(ColorTransformResults.CreateValidResult());
        }


        #endregion



        #region Create/Destroy

        void Reset()
        {
            InputData.Destroy();
            MiddleData.Destroy();
            OutputData.Destroy();
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

        protected bool ProcessPartialEventRegistered => ProcessPartial != null;
        protected void RaiseProcessPartialEvent( ColorTransformEventArgs oArgs ) => ProcessPartial?.Invoke(this, oArgs);

        //public async Task<ColorTransformInterface> CreateAsync(int[,]? oDataSource, CancellationToken oToken=default)
        //{
        //    string sM = nameof(CreateAsync);
        //    try
        //    {
        //        CreateStart();
        //        await  InputData.CreateAsync(oDataSource, oToken);
        //        return CreateEnd();
        //    }
        //    catch (Exception ex)
        //    {
        //        LogMan.Exception(sC, sM, ex);
        //        return this;
        //    }
        //}

        //public async Task<ColorTransformInterface> CreateAsync(Histogram oColorHistogramSource, CancellationToken oToken = default)
        //{
        //    string sM = nameof(CreateAsync);
        //    try
        //    {
        //        CreateStart();
        //        await InputData.CreateAsyncHistogramOnly(oColorHistogramSource, oToken);
        //        return CreateEnd();
        //    }
        //    catch (Exception ex)
        //    {
        //        LogMan.Exception(sC, sM, ex);
        //        return this;
        //    }
        //}

        public async Task<ColorTransformInterface> CreateAsync(ImageData? oImageData, CancellationToken oToken = default)
        {
            string sM = nameof(CreateAsync);
            try
            {
                CreateStart();
                await InputData.CreateAsync(oImageData?.Data, oToken);
                return CreateEnd();
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return this;
            }
        }


        //public async Task<ColorTransformInterface> CreateAsync(Palette? oColorPaletteSource, CancellationToken oToken = default)
        //{
        //    string sM = nameof(CreateAsync);
        //    try
        //    {
        //        CreateStart();
        //        await sourceDataContainer.SetColorPaletteAsyncX(oColorPaletteSource);
        //        return CreateEnd();
        //    }
        //    catch (Exception ex)
        //    {
        //        LogMan.Exception(sC, sM, ex);
        //        return this;
        //    }
        //}

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
                case ColorTransformProperties.Dithering_Type:
                    {
                        DitheringType = ColorDithering.None;
                        if (Enum.TryParse<ColorDithering>(oValue?.ToString(), true, out var eRes))
                        {
                            DitheringType = eRes;
                        }
                    }
                    break;
                case ColorTransformProperties.Dithering_Strength:
                    {
                        DitheringStrenght = 1.0;
                        if (double.TryParse(oValue?.ToString(), out var dStrenght))
                        {
                            DitheringStrenght = dStrenght;
                        }
                    }
                    break;
                default:
                    break;
            }
            return this;
        }

        internal ColorTransformEventArgs CreateTransformEventArgs(CancellationTokenSource oTokenSource, ColorTransformResults? oResult)
            => new ColorTransformEventArgs()
            {
                ColorTransformInterface = this,
                CancellationTokenSource = oTokenSource,
                ProcessingResults = oResult,
                TransformationMap = this.TransformationMap  
            };

        internal ColorTransformEventArgs CreateTransformEventArgs(CancellationTokenSource oTokenSource)
            => CreateTransformEventArgs(oTokenSource, null);


        protected async virtual Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken? oToken)
        {
            string sM = nameof(ExecuteTransformAsync);
            try
            {
                LogMan.Message(sC, sM, "Executing Default Transformation ");
                var oProcessed = await TransformationMap.TransformAsync(InputData, oToken);

                if (oProcessed != null)
                {
                    return ColorTransformResults.CreateValidResult(InputData, oProcessed);
                }
                else
                {
                    return new();
                }
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, Name, ex);
                return new();
            }
        }

       

        #region Processing

        object locker = new object();

        public async Task<ColorTransformResults> ProcessColorsAsync( CancellationToken? oTokenA)
        {
            string sM = nameof(CreateAsync);
            try
            {
                TransformationError = double.NaN;
                if (SourceData == null)
                {
                    LogMan.Error(sC, sM, $"{Type} : sourceData Null");
                    return new();
                }

                //await CreateAsync(oDataSource,oTokenA);

                var cts = oTokenA != null ? CancellationTokenSource.CreateLinkedTokenSource(oTokenA.Value) : new CancellationTokenSource();
                var oToken= cts.Token;
                TransformationMap.Reset();

                Processing?.Invoke(this, CreateTransformEventArgs(cts, null));

                var oMapRes = await CreateTrasformationMapAsync(oToken);
                if( !oMapRes.ProcessingValid )
                {
                    LogMan.Error(sC, sM, $"{Type} : CreateTrasformationMapAsync Error");
                    return oMapRes;
                }

                // Execute color reduction
                var oTransfRes = await ExecuteTransformAsync( oToken);
                if (!oTransfRes.ProcessingValid)
                {
                    LogMan.Error(sC, sM, $"{Type} :  Transformation error");
                    return oTransfRes;
                }

                var processedDataContainer = new DataContainer();
                await processedDataContainer.SetDataAsync(oTransfRes.DataOut,oToken);
                var oHash = new HashSet<int>();
                foreach (var rgb in processedDataContainer.ColorPalette.rgbPalette)
                {
                    if (rgb >= 0)
                    {
                        oHash.Add(rgb);
                    }
                }
                var oRetRes = new ColorTransformResults();
                if (BypassDithering || oHash.Count > 512 || DitheringType == ColorDithering.None)
                {
                    //if(BypassDithering)
                    //    LogMan.Message(sC, sM, $"{Name} : Processing Completed - {DitheringType} : Bypass dithering");
                    //else (DitheringType == ColorDithering.None)
                    //    LogMan.Message(sC, sM, $"{Name} : Processing Completed - {DitheringType} : No dithering");
                    if(oHash.Count > 512)
                        LogMan.Warning(sC, sM, $"{Name} : Processing Completed - {DitheringType} : Too many input colors : {oHash.Count}");

                    outputDataContainer = processedDataContainer;
                    oRetRes = ColorTransformResults.CreateValidResult(SourceData, OutputData);
                }
                else
                {
                    var oDithering = DitherBase.CreateDitherInterface(DitheringType, DitheringStrenght );     
                    var oDitheringOut = await oDithering.DitherAsync(SourceData, processedDataContainer.Data, processedDataContainer.ColorPalette, ColorDistanceEvaluationMode, oToken);
                    if (oDitheringOut == null)
                    {
                        LogMan.Error(sC, sM, $"{Type} :  Dithering error");
                        oRetRes = new ColorTransformResults()
                        {
                            Message = "Dithering Error"
                        };
                    }
                    else
                    {
                        await outputDataContainer.SetDataAsync(oDitheringOut, oToken);
                        LogMan.Message(sC, sM, $"{Type} : Processing Completed with dithering");
                        oRetRes = ColorTransformResults.CreateValidResult(SourceData, OutputData);
                    }
                }

                TransformationError = await ColorIntExt.EvaluateErrorAsync(SourceData, OutputData, ColorDistanceEvaluationMode, oToken);

                Processed?.Invoke(this, CreateTransformEventArgs(cts, oRetRes));
                return oRetRes;
            }
            catch (ThreadInterruptedException exTh)
            {
                LogMan.Error(sC, sM, $"{Type} : Processing Interrupted");
                return new ColorTransformResults()
                {
                    Message = "Processing Interrupted",
                    Exception = exTh,
                };
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return new ColorTransformResults()
                {
                    Message = ex.Message,
                    Exception = ex,
                };
            }
        }

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


        public async Task RecalcTransformationErrorAsync(int[,] oSourceImage, CancellationToken? oToken)
        {
            TransformationError =double.NaN;
            if (OutputData != null)
            {
                TransformationError = await ColorIntExt.EvaluateErrorAsync(oSourceImage, OutputData, ColorDistanceEvaluationMode, oToken);
            }
        }


        #endregion

    }
}
