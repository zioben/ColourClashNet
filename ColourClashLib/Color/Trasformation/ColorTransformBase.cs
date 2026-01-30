using ColourClashNet.Color.Dithering;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing.Imaging;
using System.Globalization;
using System.Threading.Tasks;
using static ColourClashNet.Color.Transformation.ColorTransformReductionAmiga;
using static ColourClashNet.Color.Transformation.ColorTransformReductionC64;
using static ColourClashNet.Color.Transformation.ColorTransformReductionCPC;
using static ColourClashNet.Color.Transformation.ColorTransformReductionZxSpectrum;

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

        //-------------------------------------------------------------------------------------------------------------------------------
        public ColorTransformType Type { get; protected init; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; protected init; } = "";

        //-------------------------------------------------------------------------------------------------------------------------------

        public ImageData SourceData { get; protected set; } = new ImageData();
        public ImageData OutputData { get; protected set; } = new ImageData();

        //-------------------------------------------------------------------------------------------------------------------------------
        public Palette FixedPalette { get; internal protected set; } = new Palette();
        public int FixedColors => FixedPalette?.Count ?? 0;
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

        protected virtual ColorTransformResult CreateTransformationMap(CancellationToken oToken = default)
        {
            string sM = nameof(CreateTransformationMap);
            LogMan.Debug(sM, sM, $"{Type} : Default {nameof(CreateTransformationMap)} called - no processing");
            return ColorTransformResult.CreateValidResult();
        }


        #endregion

        #region Create/Destroy

        Chrono m_chrono = new Chrono();
        public double ProcessingTimeMilliseconds => m_chrono.ElapsedMilliseconds;

        public virtual void Reset()
        {
            SourceData.Reset();
            OutputData.Reset();
        }


        protected bool ProcessPartialEventRegistered => ProcessPartial != null;
        protected void RaiseProcessPartialEvent(ColorProcessingEventArgs oArgs) => ProcessPartial?.Invoke(this, oArgs);

        public ColorTransformInterface Create(ImageData image)
        {
            string sM = nameof(Create);
            
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if(!image.IsValid)
                throw new InvalidDataException(nameof(image));

            bool bentered = false;
            try
            {
                bentered = semaphore.Wait(1000);
                if (!bentered)
                    throw new TimeoutException($"{sC}.{sM} : Cannot create");
                Reset();
                try
                {
                    Creating?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception exEvent)
                {
                    LogMan.Exception(sC, sM, $"{Type} : Error in {nameof(Creating)} event", exEvent);
                }

                SourceData.Create(image);
                
                try
                {
                    Created?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception exEvent)
                {
                    LogMan.Exception(sC, sM, $"{Type} : Error in {nameof(Created)} event", exEvent);
                }
                return this;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return null;
            }
            finally
            {
                if (bentered)
                    semaphore.Release();
            }
        }

        #endregion


        protected T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            else if (value.CompareTo(max) > 0) return max;
            else return value;
        }

        protected double ToDouble(object value)
        {
            if (value is IConvertible c)
                return c.ToDouble(CultureInfo.InvariantCulture);
            throw new ArgumentException($"Invalid value type for conversion to double: {value?.GetType().Name}");
        }

        protected bool ToBool(object value)
        {
            if (value is IConvertible c)
                return c.ToBoolean(CultureInfo.InvariantCulture);
            throw new ArgumentException($"Invalid value type for conversion to boolean: {value?.GetType().Name}");
        }
        protected int ToInt(object value)
        {
            if (value is IConvertible c)
                return c.ToInt32(CultureInfo.InvariantCulture);
            throw new ArgumentException($"Invalid value type for conversion to boolean: {value?.GetType().Name}");
        }

        protected T ToEnum<T>(object value) where T : struct, Enum
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (Enum.TryParse<T>(value.ToString(), out var eVal))
                return eVal;

            throw new ArgumentException($"Invalid value for enum conversion to {typeof(T).Name}: {value}", nameof(value));
        }

        protected double Clamp(object value, double min, double max)
        {
            return Math.Min(Math.Max(ToDouble(value), min), max);
        }

        protected internal virtual ColorTransformInterface SetProperty(ColorTransformProperties propertyName, object value)
        {
            string sM = nameof(SetProperty);
            switch (propertyName)
            {
                case ColorTransformProperties.ColorDistanceEvaluationMode:
                    ColorDistanceEvaluationMode = ToEnum<ColorDistanceEvaluationMode>(value);
                    break;
                case ColorTransformProperties.Fixed_Palette:
                    if (value is IEnumerable<int> palette1)
                        FixedPalette = new Palette().Create(palette1);
                    if (value is List<int> palette2)
                        FixedPalette = new Palette().Create(palette2);
                    else if (value is Palette palette3)
                        FixedPalette = new Palette().Create(palette3);
                    else
                        throw new ArgumentException($"{Type} : Invalid value for {propertyName} ");
                    break;
                case ColorTransformProperties.Dithering_Type:
                    DitheringType = ToEnum<ColorDithering>(value);
                    break;
                case ColorTransformProperties.Dithering_Strength:
                    DitheringStrength = Clamp(value, 0.0, 1.0);
                    break;
                default:
                    break;
            }
            return this;
        }

        public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, int value)
            => SetProperty(propertyName, (object)value);
        public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, double value)
            => SetProperty(propertyName, (object)value);
        public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, Palette value)
            => SetProperty(propertyName, (object)value);
        public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, List<int> value)
            => SetProperty(propertyName, (object)value);
        public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, IEnumerable<int> value)
            => SetProperty(propertyName, (object)value);
        public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, ColorDistanceEvaluationMode value)
            => SetProperty(propertyName, (object)value);
        public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, string value)
            => SetProperty(propertyName, (object)value);
        public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, decimal value)
            => SetProperty(propertyName, (object)value);
        public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, ColorDithering value)
            => SetProperty(propertyName, (object)value);
        public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, Boolean value)
            => SetProperty(propertyName, (object)value);
        //public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, ColorQuantizationMode value)
        //    => SetProperty(propertyName, (object)value);
        //public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, EnumAmigaVideoMode value)
        //    => SetProperty(propertyName, (object)value);
        //public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, EnumHamColorProcessingMode value)
        //    => SetProperty(propertyName, (object)value);
        //public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, C64VideoMode value)
        //    => SetProperty(propertyName, (object)value);
        //public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, CPCVideoMode value)
        //    => SetProperty(propertyName, (object)value);
        //public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, ZxPaletteMode value)
        //    => SetProperty(propertyName, (object)value);
        public ColorTransformInterface SetProperty(ColorTransformProperties propertyName, Enum value)
            => SetProperty(propertyName, (object)value);

        internal ColorProcessingEventArgs CreateTransformEventArgs(CancellationTokenSource tokenSource, ColorTransformResult result)
            => new ColorProcessingEventArgs()
            {
                ColorTransformInterface = this,
                CancellationTokenSource = tokenSource,
                ProcessingResults = result,
                TransformationMap = this.TransformationMap
            };

        internal ColorProcessingEventArgs CreateTransformEventArgs(CancellationTokenSource tokenSource)
            => CreateTransformEventArgs(tokenSource, null);


        protected virtual ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {

            string sM = nameof(ExecuteTransform);
            try
            {
                LogMan.Debug(sC, sM, "Executing Default Transformation ");
                var oProcessed = TransformationMap.Transform(SourceData, token);
                if (oProcessed.IsValid)
                {
                    return ColorTransformResult.CreateValidResult(SourceData, oProcessed);
                }
                else
                {
                    return ColorTransformResult.CreateErrorResult(SourceData, oProcessed, $"{Type} : Transformation produced invalid data");
                }
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, Name, ex);
                return new();
            }
        }



        #region Processing



        public ColorTransformResult ProcessColors( CancellationToken token = default)
        {
            string sM = nameof(ProcessColors);
            bool entered = false;

            try
            {
                entered = semaphore.Wait(1000, token);
                if (!entered)
                {
                    throw new TimeoutException($"{sC}.{sM} : Cannot process");
                }

                m_chrono.Start();

                LogMan.Debug(sC, sM, $"{Type} : Processing started");
                TransformationError = double.NaN;

                if (SourceData == null || !SourceData.IsValid)
                {
                    LogMan.Error(sC, sM, $"{Type} : InputData null or invalid");
                    return ColorTransformResult.CreateErrorResult($"{Type} : Invalid input data");
                }

                // Reset transformation map
                TransformationMap.Reset();

                // Notify processing, allowing client to handle cancellation token
                var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

                try
                {
                    Processing?.Invoke(this, CreateTransformEventArgs(cts, null));
                }
                catch (Exception exEvent)
                {
                    LogMan.Exception(sC, sM, $"{Type} : Error in {nameof(Processing)} event", exEvent);
                }



                // Creating Transformation Map
                var oMapRes = CreateTransformationMap(token);
                if (!oMapRes.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"{Type} : {nameof(CreateTransformationMap)} Error");
                    return oMapRes;
                }

                // Execute color reduction
                var oTransfRes = ExecuteTransform(token);
                var oRetRes = new ColorTransformResult();
                if (!oTransfRes.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"{Type} : {nameof(ExecuteTransform)} error");
                }
                else
                {

                    if (BypassDithering || DitheringType == ColorDithering.None)
                    {
                        OutputData.Create(oTransfRes.DataOut);
                        oRetRes = ColorTransformResult.CreateValidResult(SourceData, OutputData);
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
                            oRetRes = ColorTransformResult.CreateValidResult(SourceData, OutputData);
                        }
                        else
                        {
                            var oDithering = DitherBase.CreateDitherInterface(DitheringType, DitheringStrength);
                            var oImageDataDither = oDithering.Dither(SourceData, oProcessedData, ColorDistanceEvaluationMode, token);
                            if (oImageDataDither == null)
                            {
                                LogMan.Error(sC, sM, $"{Type} :  Dithering error");
                                oRetRes = ColorTransformResult.CreateErrorResult(SourceData, oProcessedData, $"{Type} : Dithering error");
                            }
                            else
                            {
                                OutputData.Create(oImageDataDither);
                                LogMan.Debug(sC, sM, $"{Type} : Processing Completed with dithering");
                                oRetRes = ColorTransformResult.CreateValidResult(SourceData, OutputData);
                            }
                        }

                    }

                    TransformationError = RecalcTransformationError(SourceData, token);
                }
                m_chrono.Update();
                try
                {
                    Processed?.Invoke(this, CreateTransformEventArgs(cts, oRetRes));
                }
                catch (Exception exEvent)
                {
                    LogMan.Exception(sC, sM, $"{Type} : Error in {nameof(Processed)} event", exEvent);
                }
                return oRetRes;
            }
            catch (ThreadInterruptedException exTh)
            {
                LogMan.Error(sC, sM, $"{Type} : Processing Interrupted");
                return ColorTransformResult.CreateErrorResult("Processed interrupted");
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return ColorTransformResult.CreateErrorResult(ex);
            }
            finally
            {

                if (entered)
                    semaphore.Release();
            }
        }


        public void AbortProcessing(CancellationTokenSource tokenSource)
        {
            tokenSource?.Cancel();
        }

        public double RecalcTransformationError(ImageData referenceImage, CancellationToken token = default)
            => TransformationError = ColorIntExt.EvaluateError(referenceImage, OutputData, ColorDistanceEvaluationMode, token);

        public ColorTransformResult CreateAndProcessColors(ImageData oDataSource, CancellationToken token = default)
            => Create(oDataSource).ProcessColors(token);

        //public async Task<ColorTransformResults> Create(ImageData oDataSource, CancellationToken token = default)
        //   => await Task.Run<ColorTransformResults>(() => Create(oDataSource, token));
        //public async Task<ColorTransformResults> ProcessColorsAsync(CancellationToken token = default)
        //   => await Task.Run<ColorTransformResults>(() => ProcessColors(token));

        public async Task<ColorTransformResult> CreateAndProcessColorsAsync(ImageData oDataSource, CancellationToken token = default)
           => await Task.Run<ColorTransformResult>((Func<ColorTransformResult>)(() => this.CreateAndProcessColors(oDataSource, token)));

        public async Task AbortProcessingAsync(CancellationTokenSource tokenSource)
           => await Task.Run(() => AbortProcessing(tokenSource));

        #endregion

    }
}
