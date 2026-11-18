using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Dithering
{
    public class DitherOrdered : DitherBase
    {
        static string sC = nameof(DitherOrdered);
        public new ColorDithering Type
        {
            get
            {
                if (Size == 2)
                    return ColorDithering.Ordered_2x2;
                else if (Size <= 4)
                    return ColorDithering.Ordered_4x4;
                else if (Size == 6)
                    return ColorDithering.Ordered_6x6;
                else
                    return ColorDithering.Ordered_8x8;
            }
        }

        int size = 2;
        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                size = Math.Max(2, value);
            }
        }


        public DitherOrdered()
        {
            Description = "2^P Scalable ordered dithering";
        }



        double[,] baseMatrix2 = { { 0, 2 }, { 3, 1 } };
        double divider = 4;
        double[,] thresholdMatrix = null;



        double[,] BuildNextThMatrix(double[,] matrix)
        {
            int matrixSize = matrix.GetLength(0) * 2;
            double[,] nextMatrix = new double[matrixSize, matrixSize];
            for (int r = 0; r < matrixSize; r++)
            {
                for (int c = 0; c < matrixSize; c++)
                {
                    nextMatrix[r, c] = matrix[r / 2, c / 2] + 4 * matrix[r % 2, c % 2];
                }
            }
            return nextMatrix;
        }

        public override DitherInterface Create()
        {
            if (size == 6)
            {
                // 6x6 is not a power of 2: compose the 3x3 ordered matrix with the 2x2 one (values 0..35)
                double[,] baseMatrix3 = { { 0, 7, 3 }, { 6, 5, 2 }, { 4, 1, 8 } };
                thresholdMatrix = new double[6, 6];
                for (int r = 0; r < 6; r++)
                {
                    for (int c = 0; c < 6; c++)
                    {
                        thresholdMatrix[r, c] = 4 * baseMatrix3[r / 2, c / 2] + baseMatrix2[r % 2, c % 2];
                    }
                }
                divider = 36;
            }
            else
            {
                int pow = (int)Math.Log2(size);
                thresholdMatrix = baseMatrix2.Clone() as double[,];
                divider = 4;
                for (int i = 1; i < pow; i++)
                {
                    thresholdMatrix = BuildNextThMatrix(thresholdMatrix);
                    divider *= 4;
                }
            }
            int s = thresholdMatrix.GetLength(0);
            double mean = 0;
            for (int r = 0; r < s; r++)
            {
                for (int c = 0; c < s; c++)
                {
                    // (v + 0.5) / N - 0.5 is zero-mean: (v + 1) / N - 0.5 biased the image brighter
                    thresholdMatrix[r, c] += 0.5;
                    thresholdMatrix[r, c] /= divider;
                    thresholdMatrix[r, c] -= 0.5;
                    mean += thresholdMatrix[r, c];
                }
            }
            //mean /= s * s;
            //for (int r = 0; r < s; r++)
            //{
            //    for (int c = 0; c < s; c++)
            //    {
            //        thresholdMatrix[r, c] -= mean;
            //    }
            //}

            return this;
        }

        protected override ColorTransformResult DitherImplementation(ImageData imageReference, ImageData imageProcessed, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token = default)
        {

            string sM = nameof(DitherImplementation);

            try
            {
                ImageData.AssertValidAndDimension(imageReference, imageProcessed);

                var dataOriginal = imageReference.matrix;
                var dataProcessed = imageProcessed.matrix;
                var dataProcessedPalette = imageProcessed.ColorPalette;

                Create();

                LogMan.Trace(sC, sM, $"{Type} : Dithering");
                int thresholdSize = thresholdMatrix.GetLength(0);

                double spread = 127.0;
                int rows = dataOriginal.GetLength(0);
                int columns = dataOriginal.GetLength(1);
                var dataOut = new int[rows, columns];
                var strength = DitheringStrenght;// / 100.0;
                for (int r = 0; r < rows; r++)
                //    Parallel.For(0, rows, r =>
                {
                    for (int c = 0; c < columns; c++)
                    {
                        token.ThrowIfCancellationRequested();
                        // Keep tagged (negative) pixels of the processed image, as error diffusion does
                        if (dataProcessed[r, c] < 0)
                        {
                            dataOut[r, c] = dataProcessed[r, c];
                            continue;
                        }
                        int col = dataOriginal[r, c];
                        var v = spread * strength * thresholdMatrix[r % thresholdSize, c % thresholdSize];
                        var cr = Math.Max(0, col.ToR() + v);
                        var cg = Math.Max(0, col.ToG() + v);
                        var cb = Math.Max(0, col.ToB() + v);
                        var ditheredColor = ColorIntExt.FromRGB(cr, cg, cb);
                        dataOut[r, c] = ColorIntExt.GetNearestColor(ditheredColor, dataProcessedPalette, colorEvaluationMode);
                    }
                }//);
                LogMan.Trace(sC, sM, $"{Type} : Dithering completed");
                var ditheredImage = new ImageData().Create(dataOut);
                return ColorTransformResult.CreateValidResult(imageReference, ditheredImage);
            }
            catch (TaskCanceledException et)
            {
                LogMan.Exception(sC, sM, $"{Type}", et);
                return ColorTransformResult.CreateErrorResult(et);
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Exception(sC, sM, $"{Type}", ex);
                return ColorTransformResult.CreateErrorResult(ex);
            }
        }
    }
}

