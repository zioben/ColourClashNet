using ColourClashNet.Color;
using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Drawing;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using ModuleTester;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Components
{
    public partial class ColorManager : Component
    {

        static string sClass = nameof(ColorManager);

        public ColorManager()
        {
            InitializeComponent();
        }

        public ColorManager(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            Reset();
        }



        #region Properties


        //        public Dictionary<string, object> DataParameters = new Dictionary<string, object>();

        public ColorTransformConfig transformConfig { get; set; } = new ColorTransformConfig();

        ColorTransformInterface transformSource;
        ColorTransformInterface transformBkgRemover;
        ColorTransformInterface transformQuantizer;
        ColorTransformInterface transformProcessing;

        [Browsable(false)]
        public ImageData DataSourceX { get; set; }

        [Browsable(false)]
        public ImageData DataBkgRemoved { get; set; }

        [Browsable(false)]
        public ImageData DataQuantized { get; set; }

        [Browsable(false)]
        public ImageData DataProcessed { get; set; }

        public Image ImageSource { get; protected set; }
        public Image ImageBkgRemoved { get; protected set; }
        public Image ImageQuantized { get; protected set; }
        public Image ImageProcessed { get; protected set; }

        public int ImageSourceColors => transformSource?.ImageOutput?.Colors ?? 0;
        public int ImageBkgRemovedColors => transformBkgRemover?.ImageOutput?.Colors ?? 0;
        public int ImageQuantizedColors => transformQuantizer?.ImageOutput?.Colors ?? 0;
        public int ImageProcessedColors => transformProcessing?.ImageOutput?.Colors ?? 0;

        [Browsable(false)]
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
                transformConfig = new ColorTransformConfig();
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


        public bool Create(int[,] oData)
        {
            string sMethod = nameof(Create);
            Reset();
            if (oData == null)
            {
                LogMan.Error(sClass, sMethod, "No source data");
                return false;
            }
            try
            {
                DataSourceX = new ImageData().Create(oData);
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
        public bool Create(ImageData oImageData)
            => Create(oImageData?.GetMatrix());

        public bool PreProcess()
        {
            RemoveBkgAndQuantize(true);
            InvalidatePreProcess = false;
            return true;
        }

        public void Create(System.Drawing.Image oImage) => Create(ImageToolsGDI.GdiImageToMatrix(oImage as Bitmap));    


        ImageData RemoveBkgAndQuantize(bool bRaiseEvent )
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

                var noDitherConfig = transformConfig.Clone().WithDithering(ColorDithering.None, 1, ColorDitheringFx.None);

                LogMan.Trace(sClass, sMethod, "Process Bkg Remover");
                transformBkgRemover
                    .SetProperties(noDitherConfig)
                    .Create(DataSourceX);
                var DataBkgRemovedRes = transformBkgRemover.ProcessColors(cts.Token);
                DataBkgRemoved = DataBkgRemovedRes.DataOut;
                ImageBkgRemoved = ImageToolsGDI.ImageDataToGdiImage(DataBkgRemoved);

                LogMan.Trace(sClass, sMethod, "Process Quantizer");
                transformQuantizer
                    .SetProperties(noDitherConfig)
                    .Create(DataBkgRemoved);

                var DataQuantizedRes = transformQuantizer.ProcessColors(cts.Token);
                DataQuantized = DataQuantizedRes.DataOut;
                ImageQuantized = ImageToolsGDI.ImageDataToGdiImage(DataQuantized);

                LogMan.Trace(sClass, sMethod, "Cloning Quantizer Output");
                DataProcessed = new ImageData().Create(DataQuantized);
                ImageProcessed = ImageToolsGDI.ImageDataToGdiImage(DataProcessed);
                transformProcessing = transformQuantizer;
                        
                LogMan.Trace(sClass, sMethod, "Calling Event");
                if (bRaiseEvent)
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


        public ImageData ProcessColors(ColorTransformType eTrasformType)
        {
            string sMethod = nameof(ProcessColors);
            transformProcessing = null;
            try
            {
                OnPreProcess?.Invoke(this, new ColorManagerProcessEventArgs
                {
                    DataDest = DataProcessed,
                    DataSource = DataSourceX,
                    Transformation = transformSource
                });
                switch (eTrasformType)
                {
                    case ColorTransformType.ColorReductionFast:
                        {
                            transformProcessing = new ColorTransformReductionFast();
                        }
                        break;
                    case ColorTransformType.ColorReductionClustering:
                        {
                            transformProcessing = new ColorTransformReductionCluster();
                        }
                        break;
                    case ColorTransformType.ColorReductionScanline:
                        {
                            transformProcessing = new ColorTransformReductionScanLine();
                        }
                        break;
                    case ColorTransformType.ColorReductionZxSpectrum:
                        {
                            transformProcessing = new ColorTransformReductionZxSpectrum();
                        }
                        break;
                    case ColorTransformType.ColorReductionEga:
                        {
                            transformProcessing = new ColorTransformReductionEGA();
                        }
                        break;
                    case ColorTransformType.ColorReductionCBM64:
                        {
                            transformProcessing = new ColorTransformReductionC64();
                        }
                        break;

                    case ColorTransformType.ColorReductionCPC:
                        {
                            transformProcessing = new ColorTransformReductionCPC();
                        }
                        break;

                    case ColorTransformType.ColorReductionMedianCut:
                        {
                            transformProcessing = new ColorTransformReductionMedianCut();
                        }
                        break;
                    case ColorTransformType.ColorReductionSaturation:
                        {
                            transformProcessing = new ColorTransformLumSat();
                        }
                        break;
                    case ColorTransformType.ColorReductionHam:
                        {
                            var transf = new ColorTransformReductionAmiga();
                            //transf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            //transf.AmigaVideoMode = Config.AmigaScreenMode;
                            transformProcessing = transf;
                        }
                        break;
                    default:
                        LogMan.Error(sClass, sMethod, $"Transformation {eTrasformType} not implemented");
                        transformProcessing = null;
                        return null;
                }
                transformProcessing.SetProperties(transformConfig);
//                Config.SetProperties(transformProcessing);
                if (InvalidatePreProcess)
                {
                    PreProcess();
                }
                CancellationTokenSource cts = new CancellationTokenSource();
                ProcessingForm.CreateProcessingForm(transformProcessing, cts);
                transformProcessing.Create(DataQuantized);
                //transformProcessing.SetDithering(DitherBase.CreateDitherInterface(Config.DitheringAlgorithm, Config.DitheringStrenght));
                var DataProcessedRes = transformProcessing.ProcessColors(cts.Token);
                DataProcessed = DataProcessedRes.DataOut;
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

        public async Task<ImageData> ProcessColorsAsync(ColorTransformType eTrasformType)
            => await Task.Run(() => ProcessColors(eTrasformType));  

    }
}