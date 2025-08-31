using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Controls
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
        public int[,] DataSourceX { get; set; }

        [Browsable(false)]
        public int[,] DataBkgRemoved { get; set; }

        [Browsable(false)]
        public int[,] DataQuantized { get; set; }

        [Browsable(false)]
        public int[,] DataProcessed { get; set; }

        public Image ImageSource { get; protected set; }
        public Image ImageBkgRemoved { get; protected set; }
        public Image ImageQuantized { get; protected set; }
        public Image ImageProcessed { get; protected set; }

        public int ImageSourceColors => oTrasformSource?.OutputColors ?? 0;
        public int ImageBkgRemovedColors => oTrasformBkgRemover?.OutputColors ?? 0;
        public int ImageQuantizedColors => oTrasformQuantizer?.OutputColors ?? 0;
        public int ImageProcessedColors => oTrasformProcessing?.OutputColors ?? 0;

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
                DataSourceX = oData.Clone() as int[,];
                ImageSource = ImageTools.ToBitmap(DataSourceX);
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

        public void PreProcess()
        {
            RemoveBkgAndQuantize(true);
            InvalidatePreProcess = false;
        }

        public void Create(System.Drawing.Bitmap oImage) => Create(ImageTools.ToMatrix(oImage));    


        int[,] RemoveBkgAndQuantize(bool bRaiseEvent )
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
                oTrasformSource.Create(DataSourceX, null);
                oTrasformSource.ProcessColors(DataSourceX);

                LogMan.Trace(sClass, sMethod, "Process Bkg Remover");
                oTrasformBkgRemover.Create(oTrasformSource.OutputPalette, null);
                oTrasformBkgRemover.SetProperty(ColorTransformProperties.ColorBackgroundReplacement, Config.BackgroundColorReplacement);
                oTrasformBkgRemover.SetProperty(ColorTransformProperties.ColorBackgroundList, Config.BackgroundColorList);
                DataBkgRemoved = oTrasformBkgRemover.ProcessColors(DataSourceX).DataOut;
                ImageBkgRemoved = ImageTools.ToBitmap(DataBkgRemoved);

                LogMan.Trace(sClass, sMethod, "Process Quantizer");
                oTrasformQuantizer.Create(oTrasformBkgRemover.OutputPalette, null);
                oTrasformQuantizer.SetProperty(ColorTransformProperties.QuantizationMode, Config.ColorQuantizationMode);
                DataQuantized = oTrasformQuantizer.ProcessColors(DataBkgRemoved).DataOut;
                ImageQuantized = ImageTools.ToBitmap(DataQuantized);

                LogMan.Trace(sClass, sMethod, "Colen Quantizer Output");
                DataProcessed = DataQuantized.Clone() as int[,];
                ImageProcessed = ImageTools.ToBitmap(DataProcessed);
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


        public int[,] ProcessColors(ColorTransformType eTrasformType)
        {
            string sMethod = nameof(ProcessColors);
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
                            oTrasf.ColorsMaxWanted = Config.ColorsMax;
                            oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionClustering:
                        {
                            var oTrasf = new ColorTransformReductionCluster();
                            oTrasf.ColorsMaxWanted = Config.ColorsMax;
                            oTrasf.TrainingLoop = Config.ClusteringTrainingLoop;
                            oTrasf.UseClusterColorMean = Config.ClusteringUseMeanColor;
                            oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionScanline:
                        {
                            var oTrasf = new ColorTransformReductionScanLine();
                            oTrasf.ColorsMaxWanted = Config.ColorsMax;
                            oTrasf.LineReductionMaxColors = Config.ScanlineColorsMax;
                            oTrasf.LineReductionClustering = Config.ScanlineClustering;
                            oTrasf.CreateSharedPalette = Config.ScanlineSharedPalette;
                            oTrasf.UseColorMean = Config.ClusteringUseMeanColor;
                            oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionZxSpectrum:
                        {
                            var oTrasf = new ColorTransformReductionZxSpectrum();
                            oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasf.ColL = Config.ZxEqColorLO;
                            oTrasf.ColH = Config.ZxEqColorHI;
                            oTrasf.DitherHighColor = Config.ZxEqDitherHI;
                            oTrasf.IncludeBlackInHighColor = Config.ZxEqBlackHI;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionEga:
                        {
                            var oTrasf = new ColorTransformReductionEGA();
                            oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionCBM64:
                        {
                            var oTrasf = new ColorTransformReductionC64();
                            oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasf.VideoMode = Config.C64ScreenMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;

                    case ColorTransformType.ColorReductionCPC:
                        {
                            var oTrasf = new ColorTransformReductionCPC();
                            oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasf.VideoMode = Config.CPCScreenMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;

                    case ColorTransformType.ColorReductionMedianCut:
                        {
                            var oTrasf = new ColorTransformReductionMedianCut();
                            oTrasf.ColorsMaxWanted = Config.ColorsMax;
                            oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionSaturation:
                        {
                            var oTrasf = new ColorTransformLumSat();
                            oTrasf.SaturationMultFactor = Config.SaturationEnhancement;
                            oTrasf.BrightnessMultFactor = Config.BrightnessEnhancement;
                            oTrasf.HueShift = Config.HsvHueOffset;
                            oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    case ColorTransformType.ColorReductionHam:
                        {
                            var oTrasf = new ColorTransformReductionAmiga();
                            oTrasf.ColorDistanceEvaluationMode = Config.ColorDistanceEvaluationMode;
                            oTrasf.AmigaVideoMode = Config.AmigaVideoMode;
                            oTrasformProcessing = oTrasf;
                        }
                        break;
                    default:
                        LogMan.Error(sClass, sMethod, $"Transformation {eTrasformType} not implemented");
                        oTrasformProcessing = null;
                        return null;
                }
                if (InvalidatePreProcess)
                {
                    PreProcess();
                }
                oTrasformProcessing.Create(DataQuantized, null);
                oTrasformProcessing.SetDithering(DitherBase.CreateDitherInterface(Config.DitheringAlgorithm, Config.DitheringStrenght));
                DataProcessed = oTrasformProcessing.ProcessColors(DataQuantized).DataOut;
                ImageProcessed = ImageTools.ToBitmap(DataProcessed);
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



        //Bitmap CreateIndexedBitmap(ImageWidthAlignMode eWidthAlignMode)
        //{
        //    if (mDataProcessed == null)
        //        return null;
        //    var oTrLast = lTransform.LastOrDefault();
        //    if (oTrLast == null)
        //        return null;
        //    if (oTrLast is ColorTransformReductionScanLine)
        //    {
        //        var oTras = oTrLast as ColorTransformReductionScanLine;
        //        return null;// ImageTools.CreateIndexedBitmap(mDataProcessed, oTras.ColorListRow, ImageTools.ImageWidthAlignMode.MultiplePixel16);
        //    }
        //    else
        //    {
        //        HashSet<int> iSet = new HashSet<int>();
        //        int R = mDataProcessed.GetLength(0);
        //        int C = mDataProcessed.GetLength(1);
        //        for (int r = 0; r < R; r++)
        //        {
        //            for (int c = 0; c < C; c++)
        //            {
        //                iSet.Add(mDataProcessed[r, c]);
        //            }
        //        }
        //        if (iSet.Count > 256)
        //        {
        //            return null;
        //        }
        //        var lPalette = iSet.ToList();
        //        return null;// CreateIndexedBitmap(mDataProcessed, lPalette, eWidthAlignMode);
        //    }
        //}

        //public void WriteBitmapIndex(string sFileName, ImageWidthAlignMode eWidthAlignMode)
        //{
        //    var oBmp = CreateIndexedBitmap(eWidthAlignMode);
        //    oBmp?.Save(sFileName, ImageFormat.Bmp);
        //}
        //public void WritePngIndex(string sFileName, ImageWidthAlignMode eWidthAlignMode)
        //{
        //    var oBmp = CreateIndexedBitmap(eWidthAlignMode);
        //    oBmp?.Save(sFileName, ImageFormat.Png);
        //}

        //public void WriteBitmap(string sFileName)
        //{
        //    ImageProcessed?.Save(sFileName, ImageFormat.Bmp);
        //}
        //public void WritePng(string sFileName)
        //{
        //    ImageProcessed?.Save(sFileName, ImageFormat.Png);
        //}
        //public void WriteBitplane(string sFileName, ImageTools.ImageWidthAlignMode eWidthAlignMode, bool bInterleaveData)
        //{
        //    var oTrLast = lTransform.LastOrDefault();
        //    if (oTrLast == null)
        //        return;
        //    if (oTrLast is ColorTransformReductionScanLine)
        //    {
        //        var oTr = oTrLast as ColorTransformReductionScanLine;
        //        ImageTools.ImageTools.BitplaneWriteFile(sFileName, mDataProcessed, oTr.ColorListRow, eWidthAlignMode, bInterleaveData);
        //    }
        //    else
        //    {
        //        ImageTools.ImageTools.BitplaneWriteFile(sFileName, mDataProcessed, oTrLast.OutputPalette.ToList(), eWidthAlignMode, bInterleaveData);
        //    }
    }
}