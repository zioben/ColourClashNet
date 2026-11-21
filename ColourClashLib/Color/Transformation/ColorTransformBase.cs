using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Tile;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Threading.Tasks;


namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Base class of all color transformations.
    /// <para/>
    /// <see cref="ProcessColors"/> implements the processing pipeline:
    /// <see cref="CreateTransformationMap"/> (build the color map), <see cref="ExecuteTransform"/> (apply it),
    /// optional dithering against <see cref="ImageReference"/>, then the error evaluation.
    /// Derived classes override the first two steps; both run under a semaphore, so one instance processes one image at a time.
    /// </summary>
    public abstract partial class ColorTransformBase : ColorTransformInterface
    {
        static string sC = nameof(ColorTransformBase);

        object locker = new object();

        #region events

        /// <inheritdoc/>
        public event EventHandler? Creating;
        /// <inheritdoc/>
        public event EventHandler? Created;
        /// <inheritdoc/>
        public event EventHandler<ColorProcessingEventArgs>? Processing;
        /// <inheritdoc/>
        public event EventHandler<ColorProcessingEventArgs>? ProcessAdvance;
        /// <inheritdoc/>
        public event EventHandler<ColorProcessingEventArgs>? ProcessPartial;
        /// <inheritdoc/>
        public event EventHandler<ColorProcessingEventArgs>? Processed;

        #endregion

        #region Property/Fields

        private readonly SemaphoreSlim semaphore = new(1, 1);

        /// <summary>
        /// Returns the current options (typed in <see cref="ColorTransformBase{TOptions}"/>).
        /// </summary>
        protected abstract TransformOptions GetOptions();

        /// <summary>
        /// Replaces the current options.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="options"/> is not of the type expected by the transformation.</exception>
        protected abstract void SetOptionsCore(TransformOptions options);

        /// <inheritdoc/>
        public TransformOptions Options => GetOptions();

        /// <inheritdoc/>
        public ColorTransformInterface SetOptions(TransformOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            SetOptionsCore(options);
            return this;
        }

        /// <summary>
        /// Colors chosen by the user (<see cref="TransformOptions.UserPalette"/>) as a <see cref="Palette"/>.
        /// </summary>
        protected Palette UserPalette => new Palette().Create(GetOptions().UserPalette ?? Array.Empty<int>());

        //-------------------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public ColorTransformType Type { get; protected init; }

        /// <inheritdoc/>
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string Description { get; protected init; } = "";

        //-------------------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public ImageData ImageReference { get; protected set; } = new ImageData();
        /// <inheritdoc/>
        public ImageData ImageSource { get; protected set; } = new ImageData();
        /// <inheritdoc/>
        public ImageData ImageOutput { get; protected set; } = new ImageData();

        //-------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Enables the intermediate result events.
        /// </summary>
        public bool EnablePreviewEvents { get; set; } = true;

        //-------------------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public ColorTransformationMap TransformationMap { get; protected set; } = new ColorTransformationMap();
        
        /// <inheritdoc/>
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode => GetOptions().DistanceMode;

        /// <inheritdoc/>
        public double TransformationError { get; set; } = double.NaN;

        //-------------------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public bool BypassDithering { get; set; }

        /// <summary>
        /// Dithering settings of the current options.
        /// </summary>
        protected DitherOptions DitheringOptions => GetOptions().Dithering ?? DitherOptions.None;

        #endregion

        #region abstract Methods

        /// <summary>
        /// Pipeline step 1: builds <see cref="TransformationMap"/> from <see cref="ImageSource"/>.
        /// <para/>
        /// The default implementation does nothing; override it in map based transformations.
        /// </summary>
        protected virtual ColorTransformResult CreateTransformationMap(CancellationToken token = default)
        {
            string sM = nameof(CreateTransformationMap);
            LogMan.Debug(sC, sM, $"{Type} : Default {nameof(CreateTransformationMap)} called - no processing");
            return ColorTransformResult.CreateValidResult();
        }


        #endregion

        #region Create/Destroy

        StopWatch chrono = new StopWatch();
        /// <summary>
        /// Duration of the last <see cref="ProcessColors"/> call, in milliseconds.
        /// </summary>
        public double ProcessingTimeMilliseconds => chrono.ElapsedMilliseconds;

        /// <summary>
        /// Clears the reference, source and output images.
        /// </summary>
        public virtual void Reset()
        {
            ImageReference.Reset();
            ImageSource.Reset();
            ImageOutput.Reset();
        }


        /// <summary>
        /// True when someone listens to <see cref="ProcessPartial"/>: allows skipping the creation of intermediate images.
        /// </summary>
        protected bool ProcessPartialEventRegistered => ProcessPartial != null;
        /// <summary>
        /// Raises <see cref="ProcessPartial"/>.
        /// </summary>
        protected void RaiseProcessPartialEvent(ColorProcessingEventArgs args) => ProcessPartial?.Invoke(this, args);

        /// <summary>
        /// Raises <see cref="ProcessPartial"/> on behalf of a nested step (child transformation or tile manager).
        /// <para/>
        /// The event data is copied and re-issued by this instance, so subscribers of the outermost transformation
        /// receive the progress of every nested level. Nothing is allocated when nobody is listening.
        /// </summary>
        /// <param name="sender">Nested step that raised the event</param>
        /// <param name="args">Event data of the nested step</param>
        protected void ForwardNestedEvent(object? sender, ColorProcessingEventArgs args)
        {
            if (ProcessPartial == null || args == null)
                return;
            RaiseProcessPartialEvent(new ColorProcessingEventArgs()
            {
                ColorTransformInterface = this,
                ProcessingResults = args.ProcessingResults,
                CancellationTokenSource = args.CancellationTokenSource,
                TransformationMap = args.TransformationMap,
                CompletedPercent = args.CompletedPercent,
            });
        }

        /// <summary>
        /// Subscribes to the <see cref="ProcessPartial"/> and <see cref="ProcessAdvance"/> events of a nested
        /// transformation and forwards them through <see cref="ForwardNestedEvent"/>.
        /// </summary>
        /// <typeparam name="T">Nested transformation type</typeparam>
        /// <param name="child">Nested transformation</param>
        /// <returns><paramref name="child"/>, to allow fluent use.</returns>
        protected T ForwardEventsFrom<T>(T child) where T : ColorTransformInterface
        {
            if (child != null)
            {
                child.ProcessPartial += ForwardNestedEvent;
                child.ProcessAdvance += ForwardNestedEvent;
            }
            return child;
        }

        /// <summary>
        /// Subscribes to the progress event of a <see cref="TileManager"/> and forwards it through <see cref="ForwardNestedEvent"/>.
        /// </summary>
        /// <param name="tileManager">Tile manager used by this transformation</param>
        /// <returns><paramref name="tileManager"/>, to allow fluent use.</returns>
        protected TileManager ForwardEventsFrom(TileManager tileManager)
        {
            if (tileManager != null)
            {
                tileManager.ProcessPartial += ForwardNestedEvent;
            }
            return tileManager;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">An image is invalid.</exception>
        public ColorTransformInterface Create( ImageData sourceImage, ImageData referenceImage)
        {
            string sM = nameof(Create);

            if(referenceImage != null )
                ImageData.AssertValid(referenceImage);
            ImageData.AssertValid(sourceImage);

            bool entered = false;
            try
            {
                entered = semaphore.Wait(1000);
                if (!entered)
                    throw new TimeoutException($"{sC}.{sM} : Cannot create");

                // A previous Create without reference image aliased ImageReference to ImageSource:
                // detach them, otherwise creating the reference would overwrite the source too.
                if (ReferenceEquals(ImageReference, ImageSource))
                    ImageReference = new ImageData();

                Reset();
                try
                {
                    Creating?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception exEvent)
                {
                    LogMan.Exception(sC, sM, $"{Type} : Error in {nameof(Creating)} event", exEvent);
                }

                ImageSource.Create(sourceImage);

                if (referenceImage == null)
                    ImageReference = ImageSource;
                else
                    ImageReference.Create(referenceImage);

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
                if (entered)
                    semaphore.Release();
            }
        }
        /// <inheritdoc/>
        public ColorTransformInterface Create(ImageData sourceImage)
           => Create(sourceImage, null);

        #endregion

        

        /// <summary>
        /// Builds the event data for the processing events.
        /// </summary>
        internal ColorProcessingEventArgs CreateTransformEventArgs(CancellationTokenSource tokenSource, ColorTransformResult result)
            => new ColorProcessingEventArgs()
            {
                ColorTransformInterface = this,
                CancellationTokenSource = tokenSource,
                ProcessingResults = result,
                TransformationMap = this.TransformationMap
            };

        /// <summary>
        /// Builds the event data for the processing events, without result.
        /// </summary>
        internal ColorProcessingEventArgs CreateTransformEventArgs(CancellationTokenSource tokenSource)
            => CreateTransformEventArgs(tokenSource, null);


        /// <summary>
        /// Pipeline step 2: produces the color-reduced image.
        /// <para/>
        /// The default implementation applies <see cref="TransformationMap"/> to <see cref="ImageSource"/>.
        /// </summary>
        protected virtual ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {

            string sM = nameof(ExecuteTransform);
            try
            {
                LogMan.Debug(sC, sM, "Executing Default Transformation ");
                var processedImage = TransformationMap.Transform(ImageSource, token);
                ImageData.AssertValid(processedImage);
                return ColorTransformResult.CreateValidResult(ImageSource, processedImage);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, Name, ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : transformation map not applicable", ex);
            }
        }



        #region Processing



        /// <summary>
        /// Runs the full pipeline: transformation map, color reduction, dithering and error evaluation.
        /// <para/>
        /// Dithering is skipped when bypassed, disabled, or when the reduced image still has more than 256 colors.
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>The processing result; <see cref="ImageOutput"/> holds the final image on success.</returns>
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

                chrono.Start();

                LogMan.Debug(sC, sM, $"{Type} : Processing started");
                TransformationError = double.NaN;

                ImageData.AssertValid(ImageSource);
                ImageData.AssertValid(ImageReference);
                
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



                ColorTransformResult result;

                // Validate the options before doing any work
                var validationErrors = GetOptions()?.Validate().ToList() ?? new List<string> { "options are missing" };

                ColorTransformResult? mapResult = null;
                ColorTransformResult? transformResult = null;
                if (validationErrors.Count > 0)
                {
                    LogMan.Error(sC, sM, $"{Type} : invalid options : {string.Join("; ", validationErrors)}");
                    result = ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : invalid options : {string.Join("; ", validationErrors)}");
                }
                // Creating Transformation Map
                else if ((mapResult = CreateTransformationMap(token)) == null || !mapResult.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"{Type} : {nameof(CreateTransformationMap)} Error");
                    result = ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : {nameof(CreateTransformationMap)} failed", mapResult);
                }
                else if ((transformResult = ExecuteTransform(token)) == null || !transformResult.IsSuccess || transformResult.DataOut == null)
                {
                    // Execute color reduction failed: propagate the nested error (message, output and exception)
                    LogMan.Error(sC, sM, $"{Type} : {nameof(ExecuteTransform)} error");
                    result = ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : {nameof(ExecuteTransform)} failed", transformResult);
                }
                else
                {

                    if (BypassDithering || DitheringOptions.DitheringType == ColorDithering.None)
                    {
                        ImageOutput.Create(transformResult.DataOut);
                        result = ColorTransformResult.CreateValidResult(ImageSource, ImageOutput);
                    }
                    else
                    {

                        var processedImage = transformResult.DataOut;

                        // Dithering needs a small output palette: count the real colors of the reduced image
                        var colorSet = new HashSet<int>();
                        foreach (var rgb in processedImage.ColorPalette.ToList())
                        {
                            if (rgb.IsColor())
                            {
                                colorSet.Add(rgb);
                            }
                        }

                        if (colorSet.Count > 256)
                        {
                            LogMan.Warning(sC, sM, $"{Name} : Processing Completed - {DitheringOptions} : Too many colors to apply a dither : {colorSet.Count}");
                            ImageOutput.Create(processedImage);
                            result = ColorTransformResult.CreateValidResult(ImageSource, ImageOutput);
                        }
                        else
                        {
                            var dithering = DitherBase.CreateDitherInterface(DitheringOptions);
                            var imageDataDither = dithering.Dither(ImageReference, processedImage, ColorDistanceEvaluationMode, token);
                            if (imageDataDither.IsSuccess)
                            {
                                ImageOutput.Create(imageDataDither.DataOut);
                                LogMan.Debug(sC, sM, $"{Type} : Processing Completed with dithering");
                                result = ColorTransformResult.CreateValidResult(ImageSource, ImageOutput);
                            }
                            else
                            {
                                LogMan.Error(sC, sM, $"{Type} :  Dithering error");
                                result = ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, processedImage, $"{Type} : dithering error : {imageDataDither.Message}", imageDataDither.Exception);
                            }
                        }

                    }

                    TransformationError = RecalcTransformationError(ImageSource, token);
                }
                chrono.Update();
                try
                {
                    Processed?.Invoke(this, CreateTransformEventArgs(cts, result));
                }
                catch (Exception exEvent)
                {
                    LogMan.Exception(sC, sM, $"{Type} : Error in {nameof(Processed)} event", exEvent);
                }
                return result;
            }
            catch (ThreadInterruptedException exInterrupted)
            {
                LogMan.Error(sC, sM, $"{Type} : Processing Interrupted");
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : processing interrupted", exInterrupted);
            }
            catch (OperationCanceledException exCanceled)
            {
                LogMan.Warning(sC, sM, $"{Type} : Processing cancelled");
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : processing cancelled", exCanceled);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : processing failed", ex);
            }
            finally
            {

                if (entered)
                    semaphore.Release();
            }
        }


        /// <inheritdoc/>
        public void AbortProcessing(CancellationTokenSource tokenSource)
        {
            tokenSource?.Cancel();
        }

        /// <summary>
        /// Recalculates <see cref="TransformationError"/> between <paramref name="referenceImage"/> and <see cref="ImageOutput"/>.
        /// </summary>
        public double RecalcTransformationError(ImageData referenceImage, CancellationToken token = default)
            => TransformationError = ColorIntExt.EvaluateError(referenceImage, ImageOutput, ColorDistanceEvaluationMode, token);

        /// <inheritdoc/>
        public ColorTransformResult CreateAndProcessColors(ImageData sourceImage, ImageData referenceImage,  CancellationToken token = default)
        {
            string sM = nameof(CreateAndProcessColors);
            try
            {
                // Create returns null on failure: report it as an error result instead of throwing NullReferenceException
                return Create(sourceImage, referenceImage)?.ProcessColors(token)
                    ?? ColorTransformResult.CreateErrorResult(sC, sM, sourceImage, null, $"{Type} : {nameof(Create)} failed");
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, sourceImage, null, $"{Type} : processing failed", ex);
            }
        }

        /// <inheritdoc/>
        public ColorTransformResult CreateAndProcessColors(ImageData sourceImage, CancellationToken token = default)
            => CreateAndProcessColors(sourceImage, null, token);

        //public async Task<ColorTransformResults> Create(ImageData oDataSource, CancellationToken token = default)
        //   => await Task.Run<ColorTransformResults>(() => Create(oDataSource, token));
        //public async Task<ColorTransformResults> ProcessColorsAsync(CancellationToken token = default)
        //   => await Task.Run<ColorTransformResults>(() => ProcessColors(token));

        /// <inheritdoc/>
        public async Task<ColorTransformResult> CreateAndProcessColorsAsync(ImageData sourceImage, ImageData referenceImage, CancellationToken token = default)
           => await Task.Run<ColorTransformResult>((Func<ColorTransformResult>)(() => this.CreateAndProcessColors(sourceImage, referenceImage, token)));
        /// <summary>
        /// Loads the source image and runs the transformation on a worker thread.
        /// </summary>
        public async Task<ColorTransformResult> CreateAndProcessColorsAsync(ImageData sourceImage,  CancellationToken token = default)
           => await Task.Run<ColorTransformResult>((Func<ColorTransformResult>)(() => this.CreateAndProcessColors(sourceImage, token)));

        /// <inheritdoc/>
        public async Task AbortProcessingAsync(CancellationTokenSource tokenSource)
           => await Task.Run(() => AbortProcessing(tokenSource));

        #endregion

    }
}
