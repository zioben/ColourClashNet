using ColourClashNet;
using ColourClashNet.Color;
using ColourClashNet.Imaging;
using ColourClashNet.Log;

namespace ColourClashProcessing.Blob
{
    /// <summary>
    /// Horizontal run of consecutive foreground pixels on a single row.
    /// </summary>
    public class RunLength
    {
        static string sC = nameof(RunLength);

        int row = -1;
        int xStart = -1;
        int xEnd = -1;

        bool IsValid => row >= 0 && xStart >= 0 && xEnd >= 0;

        /// <summary>Row of the run.</summary>
        public int Y => row;
        /// <summary>First column of the run (inclusive).</summary>
        public int XS => xStart;
        /// <summary>Last column of the run (inclusive).</summary>
        public int XE => xEnd;
        /// <summary>Row of the run (alias of <see cref="Y"/>).</summary>
        public int R => Y;
        /// <summary>First column (alias of <see cref="XS"/>).</summary>
        public int CS => XS;
        /// <summary>Last column (alias of <see cref="XE"/>).</summary>
        public int CE => XE;

        /// <summary>Number of pixels in the run.</summary>
        public int Length => xEnd - xStart + 1;
        /// <summary>Distance between the first and the last column (<see cref="Length"/> - 1).</summary>
        public int Width => xEnd - xStart;

        /// <summary>
        /// Creates a run covering columns <paramref name="xs"/>..<paramref name="xe"/> of row <paramref name="y"/>.
        /// </summary>
        public RunLength(int y, int xs, int xe)
        {
            row = y;
            xStart = xs;
            xEnd = xe;
        }

        /// <summary>
        /// Creates a run covering columns <paramref name="xs"/>..<paramref name="xe"/> of row <paramref name="y"/>.
        /// </summary>
        static public RunLength Create(int y, int xs, int xe)
        {
            return new RunLength(y, xs, xe);
        }

        /// <summary>
        /// Returns true when two runs on the same or adjacent rows touch.
        /// <para/>
        /// With 8-connectivity runs on adjacent rows also touch diagonally (one pixel tolerance).
        /// </summary>
        static public bool Connected(RunLength a, RunLength b, BlobConnectionModes mode)
        {
            if (a == null || b == null) return false;
            if (mode == BlobConnectionModes.FourConnected)
            {
                if (Math.Abs(a.Y - b.Y) <= 1)
                {
                    return (a.XS <= b.XE && a.XE >= b.XS);
                }
            }
            else if (mode == BlobConnectionModes.EightConnected)
            {
                if (Math.Abs(a.Y - b.Y) <= 1)
                {
                    if (a.Y == b.Y)
                    {
                        return (a.XS <= b.XE && a.XE >= b.XS);
                    }
                    else
                    {
                        return (a.XS <= b.XE + 1 && a.XE >= b.XS - 1);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Draws the runs on a <paramref name="width"/> x <paramref name="height"/> matrix using the default shape colors.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="runLengths"/> is null.</exception>
        /// <exception cref="ArgumentException">Invalid size.</exception>
        public static int[,] ToShapeMatrix(List<RunLength> runLengths, int width, int height)
        {
            string sM = nameof(ToShapeMatrix);
            try
            {
                if (runLengths == null)
                    throw new ArgumentNullException(nameof(runLengths));
                if (width <= 0 || height <= 0)
                    throw new ArgumentException("Width and Height must be positive.");

                var matrix = MatrixTools.CreateMatrix(width, height, ColorDefaults.DefaultShapeBkgColorInt);
                foreach (var rl in runLengths)
                {
                    for (int c = rl.XS; c <= rl.XE; c++)
                    {
                        matrix[rl.Y, c] = ColorDefaults.DefaultShapeForeColorInt;
                    }
                }
                return matrix;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"Operation failed", ex);
                throw;
            }
        }


        /// <summary>
        /// Draws the runs on a matrix as large as their bounding box.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="runLengths"/> is null.</exception>
        public static int[,] ToShapeMatrix(List<RunLength> runLengths)
        {
            string sM = nameof(ToShapeMatrix);
            try
            {
                if (runLengths == null)
                    throw new ArgumentNullException(nameof(runLengths));
                int xMin = runLengths.Min(run => run.XS);
                int xMax = runLengths.Max(run => run.XE);
                int yMin = runLengths.Min(run => run.Y);
                int yMax = runLengths.Max(run => run.Y);
                var width = xMax - xMin+1;
                var height= yMax - yMin+1;
                if (width <= 0 || height <= 0)
                    throw new ArgumentException("Width and Height must be positive.");
                var matrix = MatrixTools.CreateMatrix(width, height, ColorDefaults.DefaultShapeBkgColorInt);
                foreach (var rl in runLengths)
                {
                    for (int c = rl.XS; c <= rl.XE; c++)
                    {
                        matrix[rl.Y-yMin, c-xMin] = ColorDefaults.DefaultShapeForeColorInt;
                    }
                }
                return matrix;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"Operation failed", ex);
                throw;
            }            
        }



        /// <summary>
        /// Draws the runs on a new image of the given size.
        /// </summary>
        public static ImageData ToShapeImageData(List<RunLength> runLengths, int width, int height)
            => new ImageData().Create(ToShapeMatrix(runLengths, width, height));


        /// <summary>
        /// Draws the runs on a new image as large as <paramref name="referenceImage"/>.
        /// </summary>
        public static ImageData ToShapeImageData(List<RunLength> runLengths, ImageData referenceImage)
        {
            ImageData.AssertValid(referenceImage);
            return ToShapeImageData(runLengths, referenceImage.Width, referenceImage.Height);
        }

        /// <summary>
        /// Draws the runs on a new image as large as their bounding box.
        /// </summary>
        public static ImageData ToShapeImageData(List<RunLength> runLengths)
             => new ImageData().Create(ToShapeMatrix(runLengths));
        



        /// <summary>
        /// Draws the runs (foreground color) on an existing matrix.
        /// </summary>
        /// <returns>False when the runs exceed the matrix bounds.</returns>
        public static bool MergeToMatrix(List<RunLength> runLengths, int[,] mergedMatrix)
        {
            string sM = nameof(MergeToMatrix);
            if(mergedMatrix==null)
                throw new ArgumentNullException(nameof(mergedMatrix));
            var width = mergedMatrix.GetLength(1);
            var height = mergedMatrix.GetLength(0);
            var yMin = runLengths.Min(run => run.Y);
            var yMax = runLengths.Max(run => run.Y);
            var xMin = runLengths.Min(run => run.XS);
            var xMax = runLengths.Max(run => run.XE);
            if (yMin < 0 || yMax >= height || xMin < 0 || xMax >= width)
            {
                LogMan.Error(sC, sM, $"Run length dimensions [{xMin},{yMin};{xMax},{yMax}] exceeded matrix dimension [0,0;{width},{height}]");
                return false;
            }
            foreach (var rl in runLengths)
            {
                for (int c = rl.XS; c <= rl.XE; c++)
                {
                    mergedMatrix[rl.Y, c] = ColorDefaults.DefaultShapeForeColorInt;
                }
            }
            return true;
        }

    }
}
