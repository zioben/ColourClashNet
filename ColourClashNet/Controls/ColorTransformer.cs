using ColourClashNet.Color;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

        ColorItem BackColorSource = new ColorItem();
        ColorItem BackColorDest = new ColorItem(0, 0, 0);

        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode = ColorDistanceEvaluationMode.RGB;

        public Image ImageSource { get; private set; }
        public Image ImageQuantized { get; private set; }
        public Image ImageProcessed { get; private set; }
        public ColorQuantizationMode ColorQuantizationMode => oTrQuantization?.QuantizationMode ?? ColorQuantizationMode.Unknown;


        ColorTransformIdentity oTrIdentity = new ColorTransformIdentity();

        ColorTransformQuantization oTrQuantization = new ColorTransformQuantization();

        public int PixelCount = 0;
        public int ColoursSource => oTrQuantization?.DictHistogram?.Count ?? 0;
        public int ColoursQuantized => oTrQuantization?.ResultColors ?? 0;
        public int ColoursTransform { get; private set; } = 0;
        public int Width => ImageSource?.Width ?? 0;
        public int Height => ImageSource?.Height ?? 0;


        void Reset()
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

            PixelCount = 0;
            oTrIdentity = null;

            BackColorDest = new ColorItem();
            BackColorSource = new ColorItem();
            ColorDistanceEvaluationMode = ColorDistanceEvaluationMode.RGB;
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

        public void Create(System.Drawing.Bitmap oImage, ColorItem oBackColorSource, ColorItem oBackColorDest, ColorQuantizationMode eColorMode, ColorDistanceEvaluationMode eColorDistanceMode )
        {
            Reset();
            if (oImage == null)
                return;
            try
            {
                ColorDistanceEvaluationMode = eColorDistanceMode;
                BackColorSource = oBackColorSource;
                BackColorDest = oBackColorDest;
                ImageSource = new Bitmap(oImage.Width, oImage.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(ImageSource))
                {
                    g.DrawImage(oImage, 0, 0, Width, Height);
                }
                mDataSource = ToMatrix(ImageSource as Bitmap );
                ImageSource = ToBitmap(mDataSource);
                oTrIdentity = new ColorTransformIdentity();
                oTrIdentity.Create(mDataSource);
                Quantize(eColorMode);
            }
            catch (Exception ex)
            {
                Reset();
            }
        }


        public void Quantize(ColorQuantizationMode eMode)
        {
            if (oTrIdentity == null)
                return;

            oTrQuantization.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
            oTrQuantization.QuantizationMode = eMode;
            oTrQuantization.Create(oTrIdentity.DictHistogram);
            mDataQuantized = oTrQuantization.Transform(mDataSource);
            ImageQuantized = ToBitmap(mDataQuantized);
            mDataProcessed = oTrQuantization.Transform(mDataSource);
            ImageProcessed = ToBitmap(mDataProcessed);
        }

        void RebuildImageOutput()
        {
            ImageProcessed = ToBitmap(mDataProcessed);
        }

        public void ReduceColorsQuantity(int iMaxColor)
        {
            var oTrasf = new ColorTransformReductionFast();
            oTrasf.MaxColors = iMaxColor;
            oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
            oTrasf.Create(mDataQuantized);
            mDataProcessed = oTrasf.Transform(mDataQuantized);
            ColoursTransform = oTrasf.ResultColors;
            RebuildImageOutput();
        }

        public void ReduceColorsClustering(int iMaxColor, int iLoop)
        {
            var oTrasf = new ColorTransformReductionCluster();
            oTrasf.MaxColors = iMaxColor;
            oTrasf.TrainingLoop = iLoop;
            oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
            oTrasf.Create(mDataQuantized);
            mDataProcessed = oTrasf.Transform(mDataQuantized);
            ColoursTransform = oTrasf.ResultColors;
            RebuildImageOutput();
        }

        public void ReduceColorsScanLine(int iMaxColor, bool bUseCluster)
        {
            var oTrasf = new ColorTransformReductionScanLine();
            oTrasf.MaxColors = iMaxColor;
            oTrasf.Clustering = bUseCluster;
            oTrasf.ColorDistanceEvaluationMode = ColorDistanceEvaluationMode;
            oTrasf.Create(mDataQuantized);
            mDataProcessed = oTrasf.Transform(mDataQuantized);
            ColoursTransform = oTrasf.ResultColors;
            RebuildImageOutput();
        }
    }
}
