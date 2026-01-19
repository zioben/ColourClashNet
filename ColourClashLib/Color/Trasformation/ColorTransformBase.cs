using ColourClashNet.Color.Dithering;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Abstract class to handle std color transformations
    /// </summary>
    public abstract partial class ColorTransformBase : ColorTransformInterface
    {
        static string sC = nameof(ColorTransformBase);

        #region events

        public event EventHandler? Creating;
        public event EventHandler? Created;
        public event EventHandler<ColorProcessingEventArgs>? Processing;
        public event EventHandler<ColorProcessingEventArgs>? ProcessAdvance;
        public event EventHandler<ColorProcessingEventArgs>? ProcessPartial;
        public event EventHandler<ColorProcessingEventArgs>? Processed;

        #endregion

        #region Property/Fields

        private readonly SemaphoreSlim semaphore = new(1, 1);
        public bool IsProcessing { get; private set; }

        //-------------------------------------------------------------------------------------------------------------------------------
        public ColorTransformType Type { get; protected init; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; protected init; } = "";

        //-------------------------------------------------------------------------------------------------------------------------------

        public ImageData SourceData { get; protected set; } = new ImageData();
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

        public double DitheringStrength { get; set; } = 1.0;

        #endregion

        #region abstract Methods

        protected virtual ColorTransformResults CreateTransformationMap( CancellationToken oToken=default )
        {
            string sM = nameof(CreateTransformationMap);    
            LogMan.Debug(sM, sM, $"{Type} : Default {nameof(CreateTransformationMap)} called - no processing");
            return ColorTransformResults.CreateValidResult();
        }


        #endregion



        #region Create/Destroy

        void Reset()
        {
            SourceData.Reset();
            OutputData.Reset();
        }

     
        protected bool ProcessPartialEventRegistered => ProcessPartial != null;
        protected void RaiseProcessPartialEvent( ColorProcessingEventArgs oArgs ) => ProcessPartial?.Invoke(this, oArgs);

        public ColorTransformInterface Create(ImageData image)
        {
            string sM = nameof(Create);
            try
            {
                if(!semaphore.Wait(1000))
                {
                    LogMan.Error(sC, sM, $"{sC}.{sM} : Cannot create");
                    return this;
                }
                Reset();
                Creating?.Invoke(this, EventArgs.Empty);
                SourceData.Create(image);
                return this;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return this;
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion


        public virtual ColorTransformInterface SetProperty(ColorTransformProperties propertyName, object value)
        {
            string sM = nameof(SetProperty);
            try
            {
                if(!semaphore.Wait(1000))
                {
                    LogMan.Error(sC, sM, $"{sC}.{sM} : Cannot set property {propertyName}");
                    return this;
                }
                switch (propertyName)
                {
                    case ColorTransformProperties.ColorDistanceEvaluationMode:
                        if (Enum.TryParse<ColorDistanceEvaluationMode>(value?.ToString(), out var eMode))
                        {
                            ColorDistanceEvaluationMode = eMode;
                        }
                        break;
                    case ColorTransformProperties.Fixed_Palette:
                        {
                            if (value is List<int> oPalette)
                            {
                                FixedPalette = Palette.CreatePalette(oPalette);
                            }
                            else if (value is Palette oPal)
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
                            if (Enum.TryParse<ColorDithering>(value?.ToString(), true, out var eRes))
                            {
                                DitheringType = eRes;
                            }
                        }
                        break;
                    case ColorTransformProperties.Dithering_Strength:
                        {
                            DitheringStrength = 1.0;
                            if (double.TryParse(value?.ToString(), out var dStrenght))
                            {
                                DitheringStrength = dStrenght;
                            }
                        }
                        break;
                    default:
                        break;
                }
                return this;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return this;
            }
            finally
            {
                semaphore.Release();
            }   
        }

        internal ColorProcessingEventArgs CreateTransformEventArgs(CancellationTokenSource tokenSource, ColorTransformResults result)
            => new ColorProcessingEventArgs()
            {
                ColorTransformInterface = this,
                CancellationTokenSource = tokenSource,
                ProcessingResults = result,
                TransformationMap = this.TransformationMap  
            };

        internal ColorProcessingEventArgs CreateTransformEventArgs(CancellationTokenSource tokenSource)
            => CreateTransformEventArgs(tokenSource, null);


        protected virtual ColorTransformResults ExecuteTransform(CancellationToken token=default)
        {
            string sM = nameof(ExecuteTransform);
            try
            {
                LogMan.Debug(sC, sM, "Executing Default Transformation ");
                var oProcessed = TransformationMap.Transform(SourceData, token);

                if (oProcessed.DataValid)
                {
                    return ColorTransformResults.CreateValidResult(SourceData, oProcessed);
                }
                else
                {
                    return ColorTransformResults.CreateErrorResult(SourceData, oProcessed, $"{Type} : Transformation produced invalid data");   
                }
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, Name, ex);
                return new();
            }
        }



        #region Processing



        public async Task<ColorTransformResults> ProcessColorsAsync(CancellationToken token = default)
        {
            string sM = nameof(ProcessColorsAsync);
            // Monitor.Enter(locker);
            try
            {
                await semaphore.WaitAsync(token);

                IsProcessing = true;

                LogMan.Debug(sC, sM, $"{Type} : Processing started");
                TransformationError = double.NaN;

                if (SourceData == null || !SourceData.DataValid)
                {
                    LogMan.Error(sC, sM, $"{Type} : InputData null or invalid");
                    return ColorTransformResults.CreateErrorResult($"{Type} : Invalid input data");
                }

                // Reset transformation map
                TransformationMap.Reset();

                // Notify processing, allowing clinet to handle cancellation token
                var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                Processing?.Invoke(this, CreateTransformEventArgs(cts, null));

                // Creating Transformation Map
                var oMapRes = CreateTransformationMap(token);
                if (!oMapRes.ProcessingValid)
                {
                    LogMan.Error(sC, sM, $"{Type} : {nameof(CreateTransformationMap)} Error");
                    return oMapRes;
                }

                // Execute color reduction
                var oTransfRes = ExecuteTransform(token);
                if (!oTransfRes.ProcessingValid)
                {
                    LogMan.Error(sC, sM, $"{Type} : {nameof(ExecuteTransform)} error");
                    return oTransfRes;
                }


                var oRetRes = new ColorTransformResults();
                if (BypassDithering || DitheringType == ColorDithering.None)
                {
                    OutputData.Create(oRetRes.DataOut);
                    oRetRes = ColorTransformResults.CreateValidResult(SourceData, OutputData);
                }
                else
                {

                    var oProcessedData = oTransfRes.DataOut;

                    var oHash = new HashSet<int>();
                    foreach (var rgb in oProcessedData.ColorPalette.ToList())
                    {
                        if (rgb.IsColor())
                        {
                            oHash.Add(rgb);
                        }
                    }

                    if (oHash.Count > 256)
                    {
                        LogMan.Warning(sC, sM, $"{Name} : Processing Completed - {DitheringType} : Too many colors to apply a dither : {oHash.Count}");
                        OutputData.Create(oProcessedData);
                        oRetRes = ColorTransformResults.CreateValidResult(SourceData, OutputData);
                    }
                    else
                    {
                        var oDithering = DitherBase.CreateDitherInterface(DitheringType, DitheringStrength);
                        var oImageDataDither = await oDithering.DitherAsync(SourceData, oProcessedData, ColorDistanceEvaluationMode, token);
                        if (oImageDataDither == null)
                        {
                            LogMan.Error(sC, sM, $"{Type} :  Dithering error");
                            oRetRes = ColorTransformResults.CreateErrorResult(SourceData, oProcessedData, $"{Type} : Dithering error");
                        }
                        else
                        {
                            OutputData.Create(oImageDataDither);
                            LogMan.Debug(sC, sM, $"{Type} : Processing Completed with dithering");
                            oRetRes = ColorTransformResults.CreateValidResult(SourceData, OutputData);
                        }
                    }

                }

                TransformationError = await RecalcTransformationErrorAsync(SourceData, token);

                Processed?.Invoke(this, CreateTransformEventArgs(cts, oRetRes));
                return oRetRes;
            }
            catch (ThreadInterruptedException exTh)
            {
                LogMan.Error(sC, sM, $"{Type} : Processing Interrupted");
                return ColorTransformResults.CreateErrorResult("Processed interrupted");
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return ColorTransformResults.CreateErrorResult(ex);
            }
            finally
            {
                IsProcessing = false;
                semaphore.Release();
            }       
        }

        public async Task AbortProcessingAsync(CancellationTokenSource tokenSource)
        {
            if (tokenSource != null)
            {
                await tokenSource.CancelAsync();
            }
        }
        public void AbortProcessing(CancellationTokenSource tokenSource)
        {
            tokenSource?.Cancel();
        }

        public async Task<double> RecalcTransformationErrorAsync(ImageData referenceImage, CancellationToken token = default)
            => TransformationError = await ColorIntExt.EvaluateErrorAsync(referenceImage, OutputData, ColorDistanceEvaluationMode, token);

        public async Task<ColorTransformResults> CreateAndProcess(ImageData oDataSource, CancellationToken token = default)
        {
            Create(oDataSource);
            return await ProcessColorsAsync(token);
        }

        #endregion

    }
}
