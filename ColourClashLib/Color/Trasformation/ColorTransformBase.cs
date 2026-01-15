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
        public event EventHandler<ColorProcessingEventArgs> Processing;
        public event EventHandler<ColorProcessingEventArgs> ProcessAdvance;
        public event EventHandler<ColorProcessingEventArgs> ProcessPartial;
        public event EventHandler<ColorProcessingEventArgs> Processed;

        #endregion

        #region Property/Fields

        //-------------------------------------------------------------------------------------------------------------------------------
        public ColorTransformType Type { get; protected init; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; protected init; } = "";

        //-------------------------------------------------------------------------------------------------------------------------------

        public ImageData SourceData { get; protected set; } = new ImageData();
       // protected ImageData MiddleData { get; set; } = new ImageData();
        public ImageData OutputData { get; protected set; } = new ImageData();

        //-------------------------------------------------------------------------------------------------------------------------------
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
            string sM = nameof(CreateTrasformationMapAsync);    
            LogMan.Debug(sM, sM, $"{Type} : Default CreateTrasformationMapAsync called");
            return await Task.FromResult(ColorTransformResults.CreateValidResult());
        }


        #endregion



        #region Create/Destroy

        void Reset()
        {
            SourceData.Reset();
        //    MiddleData.Reset();
            OutputData.Reset();
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
        protected void RaiseProcessPartialEvent( ColorProcessingEventArgs oArgs ) => ProcessPartial?.Invoke(this, oArgs);

        public ColorTransformInterface Create(int[,]? oDataSource)
        {
            string sM = nameof(Create);
            try
            {
                CreateStart();
                SourceData.Create(oDataSource);
                return CreateEnd();
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return this;
            }
        }


        public ColorTransformInterface Create(ImageData oImageData)
        {
            string sM = nameof(Create);
            try
            {
                CreateStart();
                SourceData.Create(oImageData?.Data);
                return CreateEnd();
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return this;
            }
        }

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
                            FixedPalette = Palette.CreatePalette(oPalette);
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

        internal ColorProcessingEventArgs CreateTransformEventArgs(CancellationTokenSource oTokenSource, ColorTransformResults? oResult)
            => new ColorProcessingEventArgs()
            {
                ColorTransformInterface = this,
                CancellationTokenSource = oTokenSource,
                ProcessingResults = oResult,
                TransformationMap = this.TransformationMap  
            };

        internal ColorProcessingEventArgs CreateTransformEventArgs(CancellationTokenSource oTokenSource)
            => CreateTransformEventArgs(oTokenSource, null);


        protected async virtual Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken oToken=default)
        {
            string sM = nameof(ExecuteTransformAsync);
            try
            {
                LogMan.Message(sC, sM, "Executing Default Transformation ");
                var oProcessed = await TransformationMap.TransformAsync(SourceData, oToken);

                if (oProcessed != null)
                {
                    return ColorTransformResults.CreateValidResult(SourceData, oProcessed);
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

        public async Task<ColorTransformResults> ProcessColorsAsync( CancellationToken oToken = default)
        {
            string sM = nameof(ProcessColorsAsync);
            try
            {
                TransformationError = double.NaN;

                if (SourceData == null || !SourceData.DataValid)
                {
                    LogMan.Error(sC, sM, $"{Type} : InputData null or invalid");
                    return ColorTransformResults.CreateErrorResult($"{Type} : Invalid input data");
                }

                // Reset transformation map
                TransformationMap.Reset();

                // Notify processing, allowing clinet to handle cancellation token
                var cts = CancellationTokenSource.CreateLinkedTokenSource(oToken);
                Processing?.Invoke(this, CreateTransformEventArgs(cts, null));

                // Creating Transformation Map
                var oMapRes = await CreateTrasformationMapAsync(oToken);
                if( !oMapRes.ProcessingValid )
                {
                    LogMan.Error(sC, sM, $"{Type} : {nameof(CreateTrasformationMapAsync)} Error");
                    return oMapRes;
                }

                // Execute color reduction
                var oTransfRes = await ExecuteTransformAsync( oToken);
                if (!oTransfRes.ProcessingValid)
                {
                    LogMan.Error(sC, sM, $"{Type} : {nameof(ExecuteTransformAsync)} error");
                    return oTransfRes;
                }

                var oMiddleData = oTransfRes.DataOut;
                var oHash = new HashSet<int>();
                foreach (var rgb in oMiddleData.ColorPalette.ToList())
                {
                    if (rgb.IsColor() )
                    {
                        oHash.Add(rgb);
                    }
                }
                var oRetRes = new ColorTransformResults();
                if (BypassDithering || oHash.Count > 512 || DitheringType == ColorDithering.None)
                {
                    if(BypassDithering)
                        LogMan.Trace(sC, sM, $"{Name} : Processing Completed - {DitheringType} : Bypass dithering");
                    else if (DitheringType == ColorDithering.None)
                        LogMan.Trace(sC, sM, $"{Name} : Processing Completed - {DitheringType} : No dithering");
                    else if (oHash.Count > 512)
                        LogMan.Warning(sC, sM, $"{Name} : Processing Completed - {DitheringType} : Too many input colors : {oHash.Count}");
                    OutputData.Create(oMiddleData);
                    oRetRes = ColorTransformResults.CreateValidResult(SourceData, OutputData);
                }
                else
                {
                    var oDithering = DitherBase.CreateDitherInterface(DitheringType, DitheringStrenght );     
                    var oImageDataDither = await oDithering.DitherAsync(SourceData, oMiddleData, ColorDistanceEvaluationMode, oToken);
                    if (oImageDataDither == null)
                    {
                        LogMan.Error(sC, sM, $"{Type} :  Dithering error");
                        oRetRes = ColorTransformResults.CreateErrorResult(SourceData, oMiddleData, $"{Type} : Dithering error");
                    }
                    else
                    {
                        OutputData.Create(oImageDataDither);
                        LogMan.Debug(sC, sM, $"{Type} : Processing Completed with dithering");
                        oRetRes = ColorTransformResults.CreateValidResult(SourceData, OutputData);
                    }
                }

                TransformationError = await ColorIntExt.EvaluateErrorAsync(SourceData.Data, OutputData.Data, ColorDistanceEvaluationMode, oToken);

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

        public async Task RecalcTransformationErrorAsync(int[,] oReferenceData, CancellationToken? oToken)
            => TransformationError = await ColorIntExt.EvaluateErrorAsync(oReferenceData, OutputData?.Data, ColorDistanceEvaluationMode, oToken);

        public async Task RecalcTransformationErrorAsync(ImageData oReferenceImage, CancellationToken? oToken)
            => TransformationError = await ColorIntExt.EvaluateErrorAsync(oReferenceImage?.Data, OutputData?.Data, ColorDistanceEvaluationMode, oToken);


        #endregion

    }
}
