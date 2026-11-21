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
    /// <summary>
    /// Ordered (Bayer) dithering with a scalable threshold matrix.
    /// <para/>
    /// A threshold matrix of <see cref="Size"/> x <see cref="Size"/> is tiled over the image; each pixel is
    /// offset by its zero-mean threshold before being mapped to the nearest palette color.
    /// Power of two sizes are built recursively from the 2x2 Bayer matrix, 6x6 combines a 3x3 and a 2x2 matrix.
    /// </summary>
    public class DitherOrdered : DitherBase
    {
        static string sC = nameof(DitherOrdered);

        /// <summary>
        /// Algorithm identifier, derived from <see cref="Size"/>.
        /// </summary>
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

        /// <summary>
        /// Threshold matrix size (minimum 2). Supported values are 2, 4, 6 and 8.
        /// </summary>
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


        /// <summary>
        /// Creates the ordered ditherer.
        /// </summary>
        public DitherOrdered()
        {
            Description = "2^P Scalable ordered dithering";
        }

        // 2x2 Bayer matrix: the seed of every threshold matrix
        double[,] baseMatrix2 = { { 0, 2 }, { 3, 1 } };
        // Number of threshold levels of the current matrix (Size * Size)
        double divider = 4;
        double[,] thresholdMatrix = null;

        /// <summary>
        /// Doubles the size of a Bayer matrix: M(2n)[r,c] = M(n)[r/2,c/2] + 4 * M(2)[r%2,c%2].
        /// </summary>
        /// <param name="matrix">Bayer matrix of size n</param>
        /// <returns>Bayer matrix of size 2n.</returns>
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

        /// <summary>
        /// Builds the zero-mean threshold matrix for the current <see cref="Size"/>.
        /// </summary>
        /// <returns>This instance.</returns>
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
            // Normalize the integer levels to [-0.5, 0.5)
            int matrixSize = thresholdMatrix.GetLength(0);
            double mean = 0;
            for (int r = 0; r < matrixSize; r++)
            {
                for (int c = 0; c < matrixSize; c++)
                {
                    // (v + 0.5) / N - 0.5 is zero-mean: (v + 1) / N - 0.5 biased the image brighter
                    thresholdMatrix[r, c] += 0.5;
                    thresholdMatrix[r, c] /= divider;
                    thresholdMatrix[r, c] -= 0.5;
                    mean += thresholdMatrix[r, c];
                }
            }
            //mean /= matrixSize * matrixSize;
            //for (int r = 0; r < matrixSize; r++)
            //{
            //    for (int c = 0; c < matrixSize; c++)
            //    {
            //        thresholdMatrix[r, c] -= mean;
            //    }
            //}

            return this;
        }

        /// <summary>
        /// Applies the threshold matrix to every pixel and maps it to the nearest color of the processed image palette.
        /// </summary>
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

                int thresholdSize = thresholdMatrix.GetLength(0);

                // Maximum offset applied to each channel at full strength
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
                        int originalColor = dataOriginal[r, c];
                        var offset = spread * strength * thresholdMatrix[r % thresholdSize, c % thresholdSize];
                        var cr = Math.Max(0, originalColor.ToR() + offset);
                        var cg = Math.Max(0, originalColor.ToG() + offset);
                        var cb = Math.Max(0, originalColor.ToB() + offset);
                        var ditheredColor = ColorIntExt.FromRGB(cr, cg, cb);
                        dataOut[r, c] = ColorIntExt.GetNearestColor(ditheredColor, dataProcessedPalette, colorEvaluationMode);
                    }
                }//);
                var ditheredImage = new ImageData().Create(dataOut);
                return ColorTransformResult.CreateValidResult(imageReference, ditheredImage);
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Warning(sC, sM, $"{Type} : dithering cancelled");
                return ColorTransformResult.CreateErrorResult(sC, sM, imageReference, null, $"{Type} : dithering cancelled", ex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : dithering failed", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, imageReference, null, $"{Type} : dithering failed", ex);
            }
        }
    }
}

