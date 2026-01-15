using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Drawing;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
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

        public ColorManagerConfig Config { get; private set; }

        ColorTransformInterface oTrasformSource;
        ColorTransformInterface oTrasformBkgRemover;
        ColorTransformInterface oTrasformQuantizer;
        ColorTransformInterface oTrasformProcessing;

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

        public int ImageSourceColors => oTrasformSource?.OutputData?.Colors ?? 0;
        public int ImageBkgRemovedColors => oTrasformBkgRemover?.OutputData?.Colors ?? 0;
        public int ImageQuantizedColors => oTrasformQuantizer?.OutputData?.Colors ?? 0;
        public int ImageProcessedColors => oTrasformProcessing?.OutputData?.Colors ?? 0;

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
                Config = new ColorManagerConfig();
                //DataParameters = new Dictionary<string, object>();
                oTrasformSource = new ColorTransformIdentity();
                oTrasformBkgRemover = new ColorTransformBkgRemover();
                oTrasformQuantizer = new ColorTransformQuantization();
                oTrasformProcessing = null;
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
            => Create(oImageData?.Data);

        public async Task PreProcess()
        {
            await RemoveBkgAndQuantizeAsync(true);
            InvalidatePreProcess = false;
        }

        public void Create(System.Drawing.Image oImage) => Create(ImageToolsGDI.GdiImageToMatrix(oImage as Bitmap));    


        async Task<ImageData> RemoveBkgAndQuantizeAsync(bool bRaiseEvent )
        {
            string sMethod = nameof(RemoveBkgAndQuantizeAsync);
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
                oTrasformSource.Create(DataSourceX);

                await oTrasformSource.ProcessColorsAsync(CancellationToken.None);

                CancellationTokenSource cts = new CancellationTokenSource();

                LogMan.Trace(sClass, sMethod, "Process Bkg Remover");
                oTrasformBkgRemover
                    .Create(DataSourceX)
                    .SetProperty(ColorTransformProperties.ColorBackgroundReplacement, Config.BackgroundColorReplacement)
                    .SetProperty(ColorTransformProperties.ColorBackgroundList, Config.BackgroundColorList);                
                var DataBkgRemovedRes = (await oTrasformBkgRemover.ProcessColorsAsync(cts.Token));
                DataBkgRemoved = DataBkgRemovedRes.DataOut;
                ImageBkgRemoved = ImageToolsGDI.ImageDataToGdiImage(DataBkgRemoved);

                LogMan.Trace(sClass, sMethod, "Process Quantizer");
                oTrasformQuantizer
                    .Create(DataBkgRemoved)
                    .SetProperty(ColorTransformProperties.QuantizationMode, Config.ColorQuantizationMode);
                var DataQuantizedRes = (await oTrasformQuantizer.ProcessColorsAsync(cts.Token));
                DataQuantized = DataQuantizedRes.DataOut;
                ImageQuantized = ImageToolsGDI.ImageDataToGdiImage(DataQuantized);

                LogMan.Trace(sClass, sMethod, "Cloning Quantizer Output");
                DataProcessed = new ImageData().Create(DataQuantized);
                ImageProcessed = ImageToolsGDI.ImageDataToGdiImage(DataProcessed);
                oTrasformProcessing = oTrasformQuantizer;
                        
                LogMan.Trace(sClass, sMethod, "Calling Event");
                if (bRaiseEvent)
                {
                    OnQuantize?.Invoke(this, new ColorManagerProcessEventArgs
                    {
                        DataDest = DataProcessed,
                        DataSource = DataSourceX,
                        Transformation = oTrasformQuantizer
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


        public async Task<ImageData> ProcessColorsAsync(ColorTransformType eTrasformType)
        {
            string sMethod = nameof(ProcessColorsAsync);
            oTrasformProcessing = null;
            try
            {
                OnPreProcess?.Invoke(this, new ColorManagerProcessEventArgs
                {
                    DataDest = DataProcessed,
                    DataSource = DataSourceX,
                    Transformation = oTrasformSource
                });
                switch (eTrasformType)
                {
                    case ColorTransformType.ColorReductionFast:
                        {
                            var oTrasf = new ColorTransformReductionFast();
                            //oTrasf.ColorsMaxWanted = Config.ColorsMax;
                            //oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionClustering:
                        {
                            var oTrasf = new ColorTransformReductionCluster();
                            //oTrasf.ColorsMaxWanted = Config.ColorsMax;
                            //oTrasf.TrainingLoop = Config.ClusteringTrainingLoop;
                            //oTrasf.UseClusterColorMean = Config.ClusteringUseMeanColor;
                            //oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionScanline:
                        {
                            var oTrasf = new ColorTransformReductionScanLine();
                            //oTrasf.ColorsMaxWanted = Config.ColorsMax;
                            //oTrasf.LineReductionMaxColors = Config.ScanlineColorsMax;
                            //oTrasf.LineReductionClustering = Config.ScanlineClustering;
                            //oTrasf.CreateSharedPalette = Config.ScanlineSharedPalette;
                            //oTrasf.UseColorMean = Config.ClusteringUseMeanColor;
                            //oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionZxSpectrum:
                        {
                            var oTrasf = new ColorTransformReductionZxSpectrum();
                            //oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            //oTrasf.ColL = Config.ZxEqColorLO;
                            //oTrasf.ColH = Config.ZxEqColorHI;
                            //oTrasf.AutoTune = Config.ZxEqAutotune;
                            //oTrasf.DitherHighColor = Config.ZxEqDitherHI;
                            //oTrasf.IncludeBlackInHighColor = Config.ZxEqBlackHI;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionEga:
                        {
                            var oTrasf = new ColorTransformReductionEGA();
                            //oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionCBM64:
                        {
                            var oTrasf = new ColorTransformReductionC64();
                            //oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            //oTrasf.VideoMode = Config.C64ScreenMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;

                    case ColorTransformType.ColorReductionCPC:
                        {
                            var oTrasf = new ColorTransformReductionCPC();
                            //oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            //oTrasf.VideoMode = Config.CPCScreenMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;

                    case ColorTransformType.ColorReductionMedianCut:
                        {
                            var oTrasf = new ColorTransformReductionMedianCut();
                            //oTrasf.ColorsMaxWanted = Config.ColorsMax;
                            //oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionSaturation:
                        {
                            var oTrasf = new ColorTransformLumSat();
                            //oTrasf.SaturationMultFactor = Config.SaturationEnhancement;
                            //oTrasf.BrightnessMultFactor = Config.BrightnessEnhancement;
                            //oTrasf.HueShift = Config.HsvHueOffset;
                            //oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionHam:
                        {
                            var oTrasf = new ColorTransformReductionAmiga();
                            //oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            //oTrasf.AmigaVideoMode = Config.AmigaScreenMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    default:
                        LogMan.Error(sClass, sMethod, $"Transformation {eTrasformType} not implemented");
                        oTrasformProcessing = null;
                        return null;
                }
                Config.SetProperties(oTrasformProcessing);
                if (InvalidatePreProcess)
                {
                    PreProcess();
                }
                CancellationTokenSource cts = new CancellationTokenSource();
                oTrasformProcessing.Create(DataQuantized);
                //oTrasformProcessing.SetDithering(DitherBase.CreateDitherInterface(Config.DitheringAlgorithm, Config.DitheringStrenght));
                var DataProcessedRes = await oTrasformProcessing.ProcessColorsAsync(cts.Token);
                DataProcessed = DataProcessedRes.DataOut;
                ImageProcessed = ImageToolsGDI.ImageDataToGdiImage(DataProcessed);
                OnProcess?.Invoke(this, new ColorManagerProcessEventArgs
                {
                    DataDest = DataProcessed,
                    DataSource = DataSourceX,
                    Transformation = oTrasformProcessing,
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

    }
}