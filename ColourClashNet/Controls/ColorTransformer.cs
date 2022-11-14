using ColourClashNet.Colors;
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
        int[,] mDataQuantized = null;
        int[,] mDataProcessed = null;

        #endregion

        #region Windows objects

        public Image ImageSource { get; private set; }
        public Image ImageQuantized { get; private set; }
        public Image ImageProcessed { get; private set; }

        #endregion

        #region Properties
        public int ImageSourceColors => oTrIdentity?.ColorsUsed ?? 0;
        public int ImageQuantizedColors => oTrQuantization?.ColorsUsed ?? 0;
        public int ImageProcessedColors => lTransform.LastOrDefault()?.ColorsUsed ?? 0;
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
        public ColorTransform ColorTransformAlgorithm { get; set; } = ColorTransform.None;

        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode = ColorDistanceEvaluationMode.RGB;
        public ColorQuantizationMode ColorQuantizationMode { get; set; } = ColorQuantizationMode.Unknown;
        public ColorDithering DitheringAlgorithm { get; set; } = ColorDithering.FloydSteinberg;
        public double DiteringStrenght { get; set; } = 1.0;
        public List<int> BackgroundColorList { get; set; } = new List<int>();
        public int BackgroundColorReplacement { get; set; } = 0;

        #endregion

        #region Trasformations

        private ColorTransformIdentity oTrIdentity = new ColorTransformIdentity();
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
            ImageQuantized?.Dispose();
            ImageQuantized = null;
            ImageProcessed?.Dispose();
            ImageProcessed = null;

            mDataSource = null;
            mDataQuantized = null;
            mDataProcessed = null;

            oTrIdentity = new ColorTransformIdentity();
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
            ToPalette(lTransform.LastOrDefault()?.oColorTransformationMap.Select(X=>X.Value).ToList());
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
                return;
            try
            {
                ImageSource = new Bitmap(oImage.Width, oImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                using (Graphics g = Graphics.FromImage(ImageSource))
                    g.DrawImage(oImage, 0, 0, ImageWidth, ImageHeight);
                mDataSource = ToMatrix(ImageSource as Bitmap );
                oTrIdentity.Create(mDataSource);
                OnCreate?.Invoke(this, EventArgs.Empty);
                ProcessBase();
            }
            catch (Exception ex)
            {
                Reset();
            }
        }

        int[,] RemoveBkg()
        {
            if (mDataSource == null)
                return null;
            var oTrBkgRemover = new ColorTransformBkgRemover();
            oTrBkgRemover.ColorBackgroundList = BackgroundColorList;
            oTrBkgRemover.ColorBackground = BackgroundColorReplacement;
            oTrBkgRemover.Create(oTrIdentity.oColorHistogram);
            var mDataBkgRemoved = oTrBkgRemover.Transform(mDataSource);
            return mDataBkgRemoved;
        }

        void Quantize()
        {
            if (mDataSource == null)
                return;
            var oTrBkgRemover = new ColorTransformBkgRemover();
            oTrBkgRemover.ColorBackgroundList = BackgroundColorList;
            oTrBkgRemover.ColorBackground = BackgroundColorReplacement;
            oTrBkgRemover.Create(oTrIdentity.oColorHistogram);
            lTransform.Add(oTrBkgRemover);

            oTrQuantization.QuantizationMode = ColorQuantizationMode;
            oTrQuantization.Create(oTrBkgRemover.oColorHistogram);
            lTransform.Add(oTrQuantization);

            mDataQuantized = oTrQuantization.Transform(oTrBkgRemover.Transform(mDataSource));
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
            ImageQuantized = ToBitmap(mDataQuantized);
            ImageProcessed = ToBitmap(mDataProcessed);
        }

        int[,]? TransformAndDither(ColorTransformInterface? oTransform, int[,]? oDataOriginal )
        {
            if (oTransform == null)
                return null;
            if (oDataOriginal == null)
                return null;
            oTransform.Create(oDataOriginal);
            lTransform.Add(oTransform);
            int[,]? oDataProc = oTransform.Transform(oDataOriginal);
            switch (DitheringAlgorithm)
            {
                case ColorDithering.None:
                    {
                        oTransform.Dithering = new ColorDitherIdentity();
                        break;
                    }
                case ColorDithering.Ordered_2x2:
                    {
                        oTransform.Dithering = new ColorDitherOrdered() { Size = 2, DitheringStrenght = DiteringStrenght };
                        break;
                    }
                case ColorDithering.Ordered_4x4:
                    {
                        oTransform.Dithering = new ColorDitherOrdered() { Size = 4, DitheringStrenght = DiteringStrenght };
                        break;
                    }
                case ColorDithering.Ordered_8x8:
                    {
                        oTransform.Dithering = new ColorDitherOrdered() { Size = 8, DitheringStrenght = DiteringStrenght };
                        break;
                    }
                case ColorDithering.FloydSteinberg:
                    {
                        oTransform.Dithering= new ColorDitherJJS() { DitheringStrenght = DiteringStrenght };
                  //      oTransform.Dithering = new ColorDitherFloydSteinberg() { DitheringStrenght = DiteringStrenght };
                        break;
                    }
                default:
                    return null;
            }
            var oDataProcDither = oTransform.Dithering.Dither(oDataOriginal, oDataProc, oTransform.oColorTransformationPalette, ColorDistanceEvaluationMode);
            var oTransColorRemap = new ColorTransformToPalette() { ColorDistanceEvaluationMode = ColorDistanceEvaluationMode };
            oTransColorRemap.Create(oTransform.oColorTransformationPalette);
            lTransform.Add(oTransColorRemap);
            var oRet = oTransColorRemap.Transform(oDataProcDither);
            return oRet;
        }

        public void ColorTranform(ColorTransform eTrasform)
        {
            if (mDataQuantized == null)
                return;
            ColorTransformInterface oTrI = null;
            switch (eTrasform)
            {
                case ColorTransform.ColorReductionFast:
                    {
                        var oTrasf = new ColorTransformReductionFast();
                        oTrasf.ColorsMax = ColorsMax;
                        oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
                        oTrI = oTrasf;
                    }
                    break;
                case ColorTransform.ColorReductionClustering:
                    {
                        var oTrasf = new ColorTransformReductionCluster();
                        oTrasf.ColorsMax = ColorsMax;
                        oTrasf.TrainingLoop = ClusteringTrainingLoop;
                        oTrasf.UseClusterColorMean = ClusteringUseMeanColor;
                        oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
                        oTrI = oTrasf;
                    }
                    break;
                case ColorTransform.ColorReductionScanline:
                    {
                        var oTrasf = new ColorTransformReductionScanLine();
                        oTrasf.ColorsMax = ColorsMax;
                        oTrasf.Clustering = ScanlineClustering;
                        oTrasf.ClusteringUseMean = ClusteringUseMeanColor;
                        oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
                        oTrI = oTrasf;
                    }
                    break;
                case ColorTransform.ColorReductionZxSpectrum:
                    {
                        var oTrasf = new ColorTransformReductionZxSpectrum();
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
                ImageTools.ImageTools.BitplaneWriteFile(sFileName, mDataProcessed, oTrLast.oColorTransformationPalette, eWidthAlignMode, bInterleaveData);
            }
        }



    }
}
