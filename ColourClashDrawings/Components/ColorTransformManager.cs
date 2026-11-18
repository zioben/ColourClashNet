using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Drawing;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using ColourClashProcessing.Blob;
using ModuleTester;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ColourClashNet.Components
{
    public partial class ColorTransformManager : Component
    {

        static string sClass = nameof(ColorTransformManager);

        public ColorTransformManager()
        {
            InitializeComponent();
        }

        public ColorTransformManager(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            Reset();
        }



        #region Properties


        //        public Dictionary<string, object> DataParameters = new Dictionary<string, object>();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public ColorTransformConfig TransformConfig { get; set; } = new ColorTransformConfig();


        ColorTransformInterface transformSource;
        ColorTransformInterface transformBkgRemover;
        ColorTransformInterface transformQuantizer;
        ColorTransformInterface transformProcessing;

        [Browsable(false)]
        [DefaultValue(null)]
        public ImageData DataSourceX { get; set; }

        [Browsable(false)]
        [DefaultValue(null)]
        public ImageData DataBkgRemoved { get; set; }

        [Browsable(false)]
        [DefaultValue(null)]
        public ImageData DataQuantized { get; set; }

        [Browsable(false)]
        [DefaultValue(null)]
        public ImageData DataProcessed { get; set; }

        [DefaultValue(null)]
        public Image ImageSource { get; protected set; }

        [DefaultValue(null)]
        public Image ImageBkgRemoved { get; protected set; }

        [DefaultValue(null)]
        public Image ImageQuantized { get; protected set; }

        [DefaultValue(null)]
        public Image ImageProcessed { get; protected set; }

        public int ImageSourceColors => transformSource?.ImageOutput?.Colors ?? 0;
        public int ImageBkgRemovedColors => transformBkgRemover?.ImageOutput?.Colors ?? 0;
        public int ImageQuantizedColors => transformQuantizer?.ImageOutput?.Colors ?? 0;
        public int ImageProcessedColors => transformProcessing?.ImageOutput?.Colors ?? 0;

        [Browsable(false)]
        [DefaultValue(true)]
        public bool InvalidatePreProcess { get; set; } = true;


        #endregion

        #region Events

        public event EventHandler OnCreate;
        public event EventHandler OnReset;
        public event EventHandler<ColorManagerProcessEventArgs> OnPreProcess;
        public event EventHandler<ColorManagerProcessEventArgs> OnBkgRemoved;
        public event EventHandler<ColorManagerProcessEventArgs> OnQuantize;
        public event EventHandler<ColorManagerProcessEventArgs> OnProcess;

        #endregion


        public void Reset()
        {
            string sMethod = nameof(Reset);
            try
            {
                TransformConfig = new ColorTransformConfig();
                transformSource = new ColorTransformIdentity();
                transformBkgRemover = new ColorTransformBkgRemover();
                transformQuantizer = new ColorTransformQuantization();
                transformProcessing = null;
                //
                DataSourceX = null;
                ImageSource?.Dispose();
                ImageSource = null;
                //
                ResetProcessingData();
                OnReset?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
            }
        }

        void ResetProcessingData()
        {
            InvalidatePreProcess = true;
            //
            DataBkgRemoved = null;
            DataQuantized = null;
            DataProcessed = null;
            //
            ImageBkgRemoved?.Dispose();
            ImageBkgRemoved = null;
            ImageQuantized?.Dispose();
            ImageQuantized = null;
            ImageProcessed?.Dispose();
            ImageProcessed = null;
            //
        }


        public bool Create(int[,] data)
        {
            string sMethod = nameof(Create);
            Reset();
            if (data == null)
            {
                LogMan.Error(sClass, sMethod, "No source data");
                return false;
            }
            try
            {
                DataSourceX = new ImageData().Create(data);
                ImageSource = ImageToolsGDI.ImageDataToGdiImage(DataSourceX);
                OnCreate?.Invoke(this, EventArgs.Empty);
                PreProcess();
                return true;
            }
            catch (Exception ex)
            {
                Reset();
                return false;
            }
        }
        public bool Create(ImageData imageData)
            => Create(imageData?.GetMatrix());

        public bool PreProcess()
        {
            RemoveBkgAndQuantize(true);
            InvalidatePreProcess = false;
            return true;
        }

        public void Create(System.Drawing.Image image) => Create(ImageToolsGDI.GdiImageToMatrix(image as Bitmap));


        ImageData RemoveBkgAndQuantize(bool raiseEvent)
        {
            string sMethod = nameof(RemoveBkgAndQuantize);
            if (DataSourceX == null)
            {
                LogMan.Error(sClass, sMethod, "No source data");
                InvalidatePreProcess = true;
                return null;
            }
            try
            {
                ResetProcessingData();
                LogMan.Message(sClass, sMethod, "Starting Processing");
                LogMan.Trace(sClass, sMethod, "Process Identity Transformation");
                transformSource.CreateAndProcessColors(DataSourceX);

                CancellationTokenSource cts = new CancellationTokenSource();

                var noDitherConfig = TransformConfig.Clone().WithDithering(ColorDithering.None, 1, ColorDitheringFx.None);

                LogMan.Trace(sClass, sMethod, "Process Bkg Remover");
                transformBkgRemover
                    .SetProperties(noDitherConfig)
                    .Create(DataSourceX);
                var dataBkgRemovedRes = transformBkgRemover.ProcessColors(cts.Token);
                DataBkgRemoved = dataBkgRemovedRes.DataOut;
                ImageBkgRemoved = ImageToolsGDI.ImageDataToGdiImage(DataBkgRemoved);

                LogMan.Trace(sClass, sMethod, "Process Quantizer");
                transformQuantizer
                    .SetProperties(noDitherConfig)
                    .Create(DataBkgRemoved);

                var dataQuantizedRes = transformQuantizer.ProcessColors(cts.Token);
                DataQuantized = dataQuantizedRes.DataOut;
                ImageQuantized = ImageToolsGDI.ImageDataToGdiImage(DataQuantized);

                LogMan.Trace(sClass, sMethod, "Cloning Quantizer Output");
                DataProcessed = new ImageData().Create(DataQuantized);
                ImageProcessed = ImageToolsGDI.ImageDataToGdiImage(DataProcessed);
                transformProcessing = transformQuantizer;

                LogMan.Trace(sClass, sMethod, "Calling Event");
                if (raiseEvent)
                {
                    OnQuantize?.Invoke(this, new ColorManagerProcessEventArgs
                    {
                        DataDest = DataProcessed,
                        DataSource = DataSourceX,
                        Transformation = transformQuantizer
                    });
                }
                LogMan.Message(sClass, sMethod, "Process End");
                return DataProcessed;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
                InvalidatePreProcess = true;
                return null;
            }
        }


        public ImageData ProcessColors(ColorTransformType transformType)
        {
            string sMethod = nameof(ProcessColors);
            try
            {
                OnPreProcess?.Invoke(this, new ColorManagerProcessEventArgs
                {
                    DataDest = DataProcessed,
                    DataSource = DataSourceX,
                    Transformation = transformSource
                });

                if (InvalidatePreProcess)
                {
                    PreProcess();
                }
                transformProcessing = ColorTransformBase.CreateColorTransformInterface(transformType, TransformConfig);
                CancellationTokenSource cts = new CancellationTokenSource();
                ProcessingForm.CreateProcessingForm(transformProcessing, cts);
                transformProcessing.Create(DataQuantized);
                var dataProcessedRes = transformProcessing.ProcessColors(cts.Token);
                DataProcessed = dataProcessedRes.DataOut;
                ImageProcessed = ImageToolsGDI.ImageDataToGdiImage(DataProcessed);
                OnProcess?.Invoke(this, new ColorManagerProcessEventArgs
                {
                    DataDest = DataProcessed,
                    DataSource = DataSourceX,
                    Transformation = transformProcessing,
                });
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
                DataProcessed = null;
                ImageProcessed = null;
            }
            return DataProcessed;
        }

        public async Task<ImageData> ProcessColorsAsync(ColorTransformType transformType)
            => await Task.Run(() => ProcessColors(transformType));

        internal void InjectOutput(ImageData dataOut)
        {
            DataProcessed = dataOut;
            ImageProcessed = ImageToolsGDI.ImageDataToGdiImage(DataProcessed);
        }
    }
}