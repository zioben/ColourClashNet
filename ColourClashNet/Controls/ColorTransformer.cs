using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Controls
{
    public partial class ColorTransformer : Component
    {
        public ColorTransformer()
        {
            InitializeComponent();
        }

        public ColorTransformer(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        ColorItem[,] mDataSource = null;
        ColorItem[,] mDataQuantized = null;
        ColorItem[,] mDataProcessed = null;

        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode = ColorDistanceEvaluationMode.RGB;
        public Image ImageSource { get; private set; }
        public Image ImageQuantized { get; private set; }
        public Image ImageProcessed { get; private set; }
        public ColorQuantizationMode ColorQuantizationMode { get; set; } = ColorQuantizationMode.Unknown;

        public int PixelCount = 0;
        public int ColorsSource { get; private set; } = 0;
        public int ColorsQuantized { get; private set; } = 0;
        public int ColorsProcessed { get; private set; } = 0;
        public int Width => ImageSource?.Width ?? 0;
        public int Height => ImageSource?.Height ?? 0;

//        ColorTransformIdentity oTrIdentity = new ColorTransformIdentity();

//        ColorTransformQuantization oTrQuantization = new ColorTransformQuantization();

        public List<ColorItem> ColorBackgroundList { get; set; } = new List<ColorItem>();
        public ColorItem ColorBackgroundReplacement { get; set; } = new ColorItem(0, 0, 0);

        ColorTransformIdentity oTrIdentity = new ColorTransformIdentity();

        public event EventHandler OnReset;
        public event EventHandler OnCreate;
        public event EventHandler OnQuantize;
        public event EventHandler OnProcess;

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

            PixelCount = 0;

            ColorsProcessed = ColorsQuantized = ColorsSource = 0;
            //oTrIdentity = null;

            ColorDistanceEvaluationMode = ColorDistanceEvaluationMode.RGB;

            OnReset?.Invoke(this, EventArgs.Empty);
        }


        #region conversion

        unsafe ColorItem[,] ToMatrix(Bitmap oBmp)
        {
            var m = new ColorItem[oBmp.Height, oBmp.Width];
            var oLock = oBmp.LockBits(new Rectangle(0, 0, oBmp.Width, oBmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            {
                byte* ptr = (byte*)oLock.Scan0.ToPointer();
                for (int y = 0; y < oBmp.Height; y++)
                {
                    int yoff = oLock.Stride * y;
                    for (int x = 0, xx = 0; xx < Width; x += 3, xx++)
                    {
                        m[y, xx] = new ColorItem(ptr[yoff + x + 2], ptr[yoff + x + 1], ptr[yoff + x + 0]);
                    }
                }
                oBmp.UnlockBits(oLock);
            }
            return m;
        }

        unsafe Bitmap ToBitmap(ColorItem[,] m)
        {
            if (ImageSource == null || m == null)
                return null;
            var oBmp = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var BackColorDest = ColorBackgroundReplacement.ToDrawingColor();
            var oLock = oBmp.LockBits(new Rectangle(0, 0, oBmp.Width, oBmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            {
                byte* ptr = (byte*)oLock.Scan0.ToPointer();
                for (int y = 0; y < oBmp.Height; y++)
                {
                    int yoff = oLock.Stride * y;
                    for (int x = 0, xx = 0; xx < oBmp.Width; x += 3, xx++)
                    {
                        if (m[y, xx].Valid)
                        {
                            ptr[yoff + x + 0] = (byte)m[y, xx].B;
                            ptr[yoff + x + 1] = (byte)m[y, xx].G;
                            ptr[yoff + x + 2] = (byte)m[y, xx].R;
                        }
                        else
                        {
                            ptr[yoff + x + 0] = (byte)BackColorDest.B;
                            ptr[yoff + x + 1] = (byte)BackColorDest.G;
                            ptr[yoff + x + 2] = (byte)BackColorDest.R;
                        }
                    }
                }
                oBmp.UnlockBits(oLock);
            }
            return oBmp;
        }

        #endregion

        public void ProcessBase()
        {
            Quantize();
            mDataProcessed = mDataQuantized.Clone() as ColorItem[,];
            RebuildImageOutput();
            OnProcess?.Invoke(this, EventArgs.Empty);
        }

        public void Create(System.Drawing.Bitmap oImage)
        {
            Reset();
            if (oImage == null)
                return;
            try
            {
                ImageSource = new Bitmap(oImage.Width, oImage.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(ImageSource))
                    g.DrawImage(oImage, 0, 0, Width, Height);
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

        ColorItem[,] RemoveBkg()
        {
            var oTrBkgRemover = new ColorTransformBkgRemover();
            oTrBkgRemover.ColorBackgroundList = ColorBackgroundList;
            oTrBkgRemover.ColorBackground = ColorBackgroundReplacement;
            oTrBkgRemover.Create(oTrIdentity.DictHistogram);
            var mDataBkgRemoved = oTrBkgRemover.Transform(mDataSource);
            return mDataBkgRemoved;
        }

        ColorItem[,] Quantize()
        {
            var oTrBkgRemover = new ColorTransformBkgRemover();
            oTrBkgRemover.ColorBackgroundList = ColorBackgroundList;
            oTrBkgRemover.ColorBackground = ColorBackgroundReplacement;
            oTrBkgRemover.Create(oTrIdentity.DictHistogram);

            var oTrQuantization = new ColorTransformQuantization();
            oTrQuantization.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
            oTrQuantization.QuantizationMode = ColorQuantizationMode;
            oTrQuantization.Create(oTrIdentity.DictHistogram);

            ColorsProcessed = ColorsQuantized = oTrQuantization.DictHistogram.Count;

            mDataQuantized = oTrQuantization.Transform(oTrBkgRemover.Transform(mDataSource));
            OnQuantize?.Invoke(this, EventArgs.Empty);
            return mDataQuantized;
        }

        void RebuildImageOutput()
        {
            ImageQuantized = ToBitmap(mDataQuantized);
            ImageProcessed = ToBitmap(mDataProcessed);
        }

        public void ReduceColorsQuantity(int iMaxColor)
        {
            var oTrasf = new ColorTransformReductionFast();
            oTrasf.MaxColors = iMaxColor;
            oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
            oTrasf.Create(mDataQuantized);
            mDataProcessed = oTrasf.Transform(mDataQuantized);
            ColorsProcessed = oTrasf.ResultColors;
            RebuildImageOutput();
            OnProcess?.Invoke(this, EventArgs.Empty);
        }

        public void ReduceColorsClustering(int iMaxColor, int iLoop)
        {
            var oTrasf = new ColorTransformReductionCluster();
            oTrasf.MaxColors = iMaxColor;
            oTrasf.TrainingLoop = iLoop;
            oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
            oTrasf.Create(mDataQuantized);
            mDataProcessed = oTrasf.Transform(mDataQuantized);
            ColorsProcessed = oTrasf.ResultColors;
            RebuildImageOutput();
            OnProcess?.Invoke(this, EventArgs.Empty);

        }

        public void ReduceColorsScanLine(int iMaxColor, bool bUseCluster)
        {
            var oTrasf = new ColorTransformReductionScanLine();
            oTrasf.MaxColors = iMaxColor;
            oTrasf.Clustering = bUseCluster;
            oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
            oTrasf.Create(mDataQuantized);
            mDataProcessed = oTrasf.Transform(mDataQuantized);
            ColorsProcessed = oTrasf.ResultColors;
            RebuildImageOutput();
            OnProcess?.Invoke(this, EventArgs.Empty);
        }
    }
}
