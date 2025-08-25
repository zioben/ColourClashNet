using ColourClashNet.Colors;
using ColourClashNet.Colors.Dithering;
using ColourClashNet.Colors.Transformation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Controls
{
    public partial class ColorTransformer : Component
    {
        public class EventArgsTransformation : EventArgs
        {
            public ColorTransformInterface Transformation { get; internal set; }
            public int[,] DataSource { get; internal set; }
            public int[,] DataDest { get; internal set; }
        }

        public ColorTransformer()
        {
            InitializeComponent();
            RegisterEvents();
        }

        public ColorTransformer(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            RegisterEvents();
        }


        #region Data processed

        int[,] mDataSource = null;
        int[,] mDataBkgRemoved = null;
        int[,] mDataQuantized = null;
        int[,] mDataProcessed = null;

        #endregion

        #region Windows objects

        public Image ImageSource { get; private set; }
        public Image ImageBkgRemoved { get; private set; }
        public Image ImageQuantized { get; private set; }
        public Image ImageProcessed { get; private set; }

        #endregion

        #region Properties
        public int ImageSourceColors => oTrIdentity?.OutputColors ?? 0;
        public int ImageBkgRemovedColors => oTrBkgRemover?.OutputColors ?? 0;
        public int ImageQuantizedColors => oTrQuantization?.OutputColors ?? 0;
        public int ImageProcessedColors => lTransform.LastOrDefault()?.OutputColors ?? 0;
        public int ImageWidth => ImageSource?.Width ?? 0;
        public int ImageHeight => ImageSource?.Height ?? 0;

        int iColorsMax = 16;
        public int ColorsMax 
        {
            get { return iColorsMax; }
            set
            {
                iColorsMax = Math.Max(2, Math.Min(256, value));
            }
        }

        public double SaturationEnhancement { get; set; } = 1;
        public double BrightnessEnhancement { get; set; } = 1;
        public double HsvHueOffset { get; set; } = 0;

        int iTrainLoop = 30;
        public int ClusteringTrainingLoop
        {
            get
            {
                return iTrainLoop;
            }
            set
            {
                iTrainLoop = Math.Max(1, value);
            }
        }

        public bool ClusteringUseMeanColor { get; set; } = true;
        public bool ScanlineClustering { get; set; } = true;
        public int ScanlineColorsMax { get; set; } = 7;
        public bool ScanlineSharedPalette { get; set; } = true;

        public ColorTransformType ColorTransformAlgorithm { get; set; } = ColorTransformType.None;
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.RGB;
        public ColorQuantizationMode ColorQuantizationMode { get; set; } = ColorQuantizationMode.Unknown;
        public ColorDithering DitheringAlgorithm { get; set; } = ColorDithering.FloydSteinberg;
        public double DitheringStrenght { get; set; } = 1.0;
        public List<int> BackgroundColorList { get; set; } = new List<int>();
        public int BackgroundColorReplacement { get; set; } = 0;
        public int ZxEqColorLO { get; set; } = 0x80;
        public int ZxEqColorHI { get; set; } = 0xFF;
        public bool ZxEqBlackHI { get; set; } = true;
        public bool ZxEqDitherHI { get; set; } = true;
        public ColorTransformReductionAmiga.EnumAMigaVideoMode AmigaVideoMode { get; set; } = ColorTransformReductionAmiga.EnumAMigaVideoMode.Ham6;
        public ColorTransformReductionC64.C64VideoMode C64ScreenMode { get; set; } = ColorTransformReductionC64.C64VideoMode.Multicolor;
        public ColorTransformReductionCPC.CPCVideoMode CPCScreenMode { get; set; } = ColorTransformReductionCPC.CPCVideoMode.Mode0;

        #endregion

        #region Trasformations

        private ColorTransformIdentity oTrIdentity = new ColorTransformIdentity();
        private ColorTransformBkgRemover oTrBkgRemover = new ColorTransformBkgRemover();
        private ColorTransformQuantization oTrQuantization = new ColorTransformQuantization();
        private List<ColorTransformInterface> lTransform = new List<ColorTransformInterface>();

        #endregion


       // private ColorTransformInterface oTrLast;

        public event EventHandler OnReset;
        public event EventHandler OnCreate;
        public event EventHandler<EventArgsTransformation> OnQuantize;
        public event EventHandler<EventArgsTransformation> OnProcess;

        public void Reset()
        {
            ImageSource?.Dispose();
            ImageSource = null;
            ImageBkgRemoved?.Dispose();
            ImageBkgRemoved = null;
            ImageQuantized?.Dispose();
            ImageQuantized = null;
            ImageProcessed?.Dispose();
            ImageProcessed = null;

            mDataSource = null;
            mDataBkgRemoved = null;
            mDataQuantized = null;
            mDataProcessed = null;

            oTrIdentity = new ColorTransformIdentity();
            oTrBkgRemover = new ColorTransformBkgRemover(); 
            oTrQuantization = new ColorTransformQuantization();
            lTransform = new List<ColorTransformInterface>();

            OnReset?.Invoke(this, EventArgs.Empty);
        }


        #region conversion

        unsafe int[,] ToMatrix(Bitmap oBmp)
        {
            var m = new int[oBmp.Height, oBmp.Width];
            var oLock = oBmp.LockBits(new Rectangle(0, 0, oBmp.Width, oBmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            {
                byte* ptr = (byte*)oLock.Scan0.ToPointer();
                for (int y = 0; y < oBmp.Height; y++)
                {
                    int yoff = oLock.Stride * y;
                    int* ptrRow = (int*)(oLock.Scan0+yoff);
                    for (int x = 0; x < ImageWidth; x++)
                    {
                        m[y, x] = ptrRow[x] & 0x00FFFFFF;
                    }
                }
                oBmp.UnlockBits(oLock);
            }
            return m;
        }

        unsafe Bitmap ToBitmap(int[,] m)
        {
            if (m == null)
                return null;
            var R = m.GetLength(0);
            var C = m.GetLength(1);
            var oBmp = new Bitmap(C, R, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            var oLock = oBmp.LockBits(new Rectangle(0, 0, oBmp.Width, oBmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            {
                byte* ptr = (byte*)oLock.Scan0.ToPointer();
                for (int y = 0; y < oBmp.Height; y++)
                {
                    int yoff = oLock.Stride * y;
                    int* ptrRow = (int*)(oLock.Scan0 + yoff);
                    for (int x = 0; x < oBmp.Width; x++)
                    {
                        ptrRow[x] = m[y, x] >= 0 ? m[y, x] : BackgroundColorReplacement;
                    }
                }
                oBmp.UnlockBits(oLock);
            }
            return oBmp;
        }

        ColorPalette ToPalette(List<int> lPalette)
        {
            using (var oBmp = new Bitmap(16, 16, PixelFormat.Format8bppIndexed))
            {
                var loop = Math.Min(lPalette.Count, oBmp.Palette.Entries.Length);
                for (int i = 0; i < loop; i++)
                {
                    oBmp.Palette.Entries[i] = lPalette[i].ToDrawingColor();
                }
                return oBmp.Palette;
            }
        }

        ColorPalette ToPalette(Dictionary<int, int> dictTrasf)
        {
            if (dictTrasf == null)
                return null;
            List<int> lCol = dictTrasf.Where(X => X.Value >= 0).Select(X => X.Value).ToList().Distinct().ToList();
            return ToPalette(lCol); 
        }


        #endregion

        #region Event Handling

        void RegisterEvents()
        {
            OnProcess += ColorTransformer_OnProcess;
        }

     

        private void ColorTransformer_OnProcess(object? sender, EventArgsTransformation e)
        {
            ToPalette(lTransform.LastOrDefault()?.ColorTransformationMapper.rgbTransformationMap.Select(X=>X.Value).ToList());
            RebuildImageOutput();
        }

        #endregion

        public void ProcessBase()
        {
            if (mDataSource == null)
                return;
            Quantize();
            mDataProcessed = mDataQuantized.Clone() as int[,];
            OnProcess?.Invoke(this, new EventArgsTransformation
            {
                DataDest = mDataProcessed,
                DataSource = mDataSource,
                Transformation = lTransform.LastOrDefault(),
            });
        }

        public void Create(System.Drawing.Bitmap oImage)
        {
            Reset();
            if (oImage == null)
            {
                return;
            }
            try
            {
                ImageSource = new Bitmap(oImage.Width, oImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                using (Graphics g = Graphics.FromImage(ImageSource))
                {
                    g.DrawImage(oImage, 0, 0, ImageWidth, ImageHeight);
                }
                mDataSource = ToMatrix(ImageSource as Bitmap );
                oTrIdentity.Create(mDataSource,null);
                OnCreate?.Invoke(this, EventArgs.Empty);
                ProcessBase();
            }
            catch (Exception ex)
            {
                Reset();
            }
        }

        //int[,] RemoveBkg()
        //{
        //    if (mDataSource == null)
        //        return null;
        //    var oTrBkgRemover = new ColorTransformBkgRemover()
        //        .SetProperty( ColorTransformProperties.ColorBackgroundList, BackgroundColorList)
        //        .SetProperty( ColorTransformProperties.ColorBackgroundReplacement, BackgroundColorReplacement)
        //        .Create(oTrIdentity.OutputHistogram, null);
        //    var mDataBkgRemoved = oTrBkgRemover.ProcessColors(mDataSource);
        //    return mDataBkgRemoved.DataOut;
        //}

        void Quantize()
        {
            if (mDataSource == null)
                return;
            oTrBkgRemover.ColorBackgroundReplacement = BackgroundColorReplacement;
            oTrBkgRemover.BackgroundPalette = ColourClashLib.Color.ColorPalette.CreateColorPalette(BackgroundColorList) ?? new ColourClashLib.Color.ColorPalette();
            oTrBkgRemover.Create(oTrIdentity.OutputHistogram, null);
            //var oTrBkgRemover = new ColorTransformBkgRemover()
            //    .SetProperty( ColorTransformProperties.ColorBackgroundList, BackgroundColorList)
            //    .SetProperty( ColorTransformProperties.ColorBackgroundReplacement, BackgroundColorReplacement)
            //    .Create(oTrIdentity.OutputHistogram, null);
            lTransform.Add(oTrBkgRemover);

            oTrQuantization.QuantizationMode = ColorQuantizationMode;
            oTrQuantization.Create(oTrBkgRemover.OutputHistogram, null);
            //oTrQuantization.Dithering = CreateDithering();
            lTransform.Add(oTrQuantization);

            var oBkgRemover = oTrBkgRemover.ProcessColors(mDataSource);
            mDataBkgRemoved = oBkgRemover.DataOut?.Clone() as int[,];
            var oQuantizer = oTrQuantization.ProcessColors(oBkgRemover.DataOut);
            mDataQuantized = oQuantizer.DataOut;
            mDataProcessed = mDataQuantized.Clone() as int[,];
            RebuildImageOutput();
            OnQuantize?.Invoke(this, new EventArgsTransformation 
            {
                DataDest = mDataQuantized,
                DataSource = mDataSource,
                Transformation = oTrQuantization
            });
            return;
        }

        void RebuildImageOutput()
        {
            ImageBkgRemoved = ToBitmap(mDataBkgRemoved);    
            ImageQuantized = ToBitmap(mDataQuantized);
            ImageProcessed = ToBitmap(mDataProcessed);
        }

        DitherInterface? CreateDithering()
        {
            DitherInterface Dithering;
            switch (DitheringAlgorithm)
            {
                case ColorDithering.Atkinson:
                    Dithering = new DitherAtkinson();
                    break;
                case ColorDithering.Burkes:
                    Dithering = new DitherBurkes();
                    break;
                case ColorDithering.FloydSteinberg:
                    Dithering = new DitherFloydSteinberg();
                    break;
                case ColorDithering.JarvisJudiceNinke:
                    Dithering = new DitherJarvisJudiceNinke();
                    break;
                case ColorDithering.ScanLine:
                    Dithering = new DitherScanLine();
                    break;
                case ColorDithering.None:
                    Dithering = new DitherIdentity();
                    break;
                case ColorDithering.Ordered_2x2:
                    Dithering = new DitherOrdered() { Size = 2 };
                    break;
                case ColorDithering.Ordered_4x4:
                    Dithering = new DitherOrdered() { Size = 4 };
                    break;
                case ColorDithering.Ordered_8x8:
                    Dithering = new DitherOrdered() { Size = 8 };
                    break;
                case ColorDithering.Sierra:
                    Dithering = new DitherSierra();
                    break;
                case ColorDithering.Stucki:
                    Dithering = new DitherStucki();
                    break;

                default:
                    return null;
            }
            if (Dithering != null)
            {
                Dithering.DitheringStrenght = DitheringStrenght;
                Dithering.Create();
            }
            return Dithering;
        }

        int[,]? TransformAndDither(ColorTransformInterface? oTransform, int[,]? oDataOriginal )
        {
            if (oTransform == null)
                return null;
            if (oDataOriginal == null)
                return null;
            oTransform.Create(oDataOriginal, null);
            oTransform.Dithering = CreateDithering();
            lTransform.Add(oTransform);
            var oRet = oTransform.ProcessColors(oDataOriginal);
            return oRet.DataOut;
        }

        public void ColorTranform(ColorTransformType eTrasform)
        {
            if (mDataQuantized == null)
                return;
            ColorTransformInterface oTrI = null;
            switch (eTrasform)
            {
                case ColorTransformType.ColorReductionFast:
                    {
                        var oTrasf = new ColorTransformReductionFast();
                        oTrasf.ColorsMaxWanted = ColorsMax;
                        oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
                        oTrI = oTrasf;
                    }
                    break;
                case ColorTransformType.ColorReductionClustering:
                    {
                        var oTrasf = new ColorTransformReductionCluster();
                        oTrasf.ColorsMaxWanted = ColorsMax;
                        oTrasf.TrainingLoop = ClusteringTrainingLoop;
                        oTrasf.UseClusterColorMean = ClusteringUseMeanColor;
                        oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
                        oTrI = oTrasf;
                    }
                    break;
                case ColorTransformType.ColorReductionScanline:
                    {
                        var oTrasf = new ColorTransformReductionScanLine();
                        oTrasf.ColorsMaxWanted = ColorsMax;
                        oTrasf.LineReductionMaxColors = ScanlineColorsMax;
                        oTrasf.LineReductionClustering = ScanlineClustering;
                        oTrasf.CreateSharedPalette = ScanlineSharedPalette;
                        oTrasf.UseColorMean = ClusteringUseMeanColor;
                        oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
                        oTrI = oTrasf;
                    }
                    break;
                case ColorTransformType.ColorReductionZxSpectrum:
                    {
                        var oTrasf = new ColorTransformReductionZxSpectrum();
                        oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
                        oTrasf.ColL = ZxEqColorLO;
                        oTrasf.ColH = ZxEqColorHI;
                        oTrasf.DitherHighColor = ZxEqDitherHI;
                        oTrasf.IncludeBlackInHighColor = ZxEqBlackHI;
                        oTrI = oTrasf;
                    }
                    break;
                case ColorTransformType.ColorReductionEga:
                {
                        var oTrasf = new ColorTransformReductionEGA();
                        oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
                        oTrI = oTrasf;
                    }
                    break;
                case ColorTransformType.ColorReductionCBM64:
                    {
                        var oTrasf = new ColorTransformReductionC64();
                        oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
                        oTrasf.VideoMode = C64ScreenMode;
                        oTrI = oTrasf;
                    }
                    break;

                case ColorTransformType.ColorReductionCPC:
                    {
                        var oTrasf = new ColorTransformReductionCPC();
                        oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
                        oTrasf.VideoMode = CPCScreenMode;
                        oTrI = oTrasf;
                    }
                    break;

                case ColorTransformType.ColorReductionMedianCut:
                    {
                        var oTrasf = new ColorTransformReductionMedianCut();
                        oTrasf.ColorsMaxWanted = ColorsMax;
                        oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
                        oTrI = oTrasf;
                    }
                    break;
                case ColorTransformType.ColorReductionSaturation:
                    {
                        var oTrasf = new ColorTransformLumSat();
                        oTrasf.SaturationMultFactor = SaturationEnhancement;
                        oTrasf.BrightnessMultFactor = BrightnessEnhancement;
                        oTrasf.HueShift = HsvHueOffset;
                        oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
                        oTrI = oTrasf;
                    }
                    break;
                case ColorTransformType.ColorReductionHam:
                    {
                        var oTrasf = new ColorTransformReductionAmiga();
                        oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
                        oTrasf.AmigaVideoMode = AmigaVideoMode;
                        oTrI = oTrasf;
                    }
                    break;
                default:
                    return;
            }
            if (oTrI == null)
                return;
            mDataProcessed = TransformAndDither(oTrI, mDataQuantized);
            OnProcess?.Invoke(this, new EventArgsTransformation
            {
                DataDest = mDataProcessed,
                DataSource = mDataSource,
                Transformation = oTrI,
            });
        }



        Bitmap CreateIndexedBitmap(ImageTools.ImageWidthAlignMode eWidthAlignMode)
        {           
            if( mDataProcessed == null )
                return null;
            var oTrLast = lTransform.LastOrDefault();
            if (oTrLast == null)
                return null;
            if (oTrLast is ColorTransformReductionScanLine)
            {
                var oTras = oTrLast as ColorTransformReductionScanLine;
                return ImageTools.ImageTools.CreateIndexedBitmap(mDataProcessed, oTras.ColorListRow, ImageTools.ImageWidthAlignMode.MultiplePixel16);                
            }
            else
            {
                HashSet<int> iSet = new HashSet<int>();
                int R = mDataProcessed.GetLength(0);
                int C = mDataProcessed.GetLength(1);
                for (int r = 0; r < R; r++)
                {
                    for (int c = 0; c < C; c++)
                    {
                        iSet.Add(mDataProcessed[r, c]);
                    }
                }
                if (iSet.Count > 256)
                {
                    return null;
                }
                var lPalette = iSet.ToList();
                return ImageTools.ImageTools.CreateIndexedBitmap(mDataProcessed, lPalette, eWidthAlignMode);
            }
        }

        public void WriteBitmapIndex(string sFileName, ImageTools.ImageWidthAlignMode eWidthAlignMode)
        {
            var oBmp = CreateIndexedBitmap(eWidthAlignMode);
            oBmp?.Save(sFileName, ImageFormat.Bmp);
        }
        public void WritePngIndex(string sFileName, ImageTools.ImageWidthAlignMode eWidthAlignMode)
        {
            var oBmp = CreateIndexedBitmap(eWidthAlignMode);
            oBmp?.Save(sFileName, ImageFormat.Png);
        }
        public void WriteBitmap(string sFileName)
        {
            ImageProcessed?.Save(sFileName, ImageFormat.Bmp);
        }
        public void WritePng(string sFileName)
        {
            ImageProcessed?.Save(sFileName, ImageFormat.Png);
        }
        public void WriteBitplane(string sFileName, ImageTools.ImageWidthAlignMode eWidthAlignMode, bool bInterleaveData)
        {
            var oTrLast = lTransform.LastOrDefault();
            if (oTrLast == null)
                return;
            if (oTrLast is ColorTransformReductionScanLine)
            {
                var oTr = oTrLast as ColorTransformReductionScanLine;
                ImageTools.ImageTools.BitplaneWriteFile(sFileName, mDataProcessed, oTr.ColorListRow, eWidthAlignMode, bInterleaveData);
            }
            else
            {
                ImageTools.ImageTools.BitplaneWriteFile(sFileName, mDataProcessed, oTrLast.OutputPalette.ToList(), eWidthAlignMode, bInterleaveData);
            }
        }



    }
}
