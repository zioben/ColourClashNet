using ColourClashNet;
using ColourClashNet.Color;
using ColourClashNet.Imaging;
using ColourClashNet.Log;

namespace ColourClashProcessing.Blob
{
    public class RunLength
    {
        static string sC = nameof(RunLength);

        int _y = -1;
        int _xs = -1;
        int _xe = -1;

        bool IsValid => _y >= 0 && _xs >= 0 && _xe >= 0;
        public int Y => _y;
        public int XS => _xs;
        public int XE => _xe;
        public int R => Y;
        public int CS => XS;
        public int CE => XE;

        public int Length => _xe - _xs + 1;
        public int Width => _xe - _xs;

        public RunLength(int y, int xs, int xe)
        {
            _y = y;
            _xs = xs;
            _xe = xe;
        }


        static public RunLength Create(int y, int xs, int xe)
        {
            return new RunLength(y, xs, xe);
        }

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

        public static int[,] ToShapeMatrix(List<RunLength> runLengths, int width, int height)//, int foreRGB, int bkgRGB)
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


        public static int[,] ToShapeMatrix(List<RunLength> runLengths)//, int foreRGB, int bkgRGB)
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



        public static ImageData ToShapeImageData(List<RunLength> runLengths, int width, int height)
            => new ImageData().Create(ToShapeMatrix(runLengths, width, height));


        public static ImageData ToShapeImageData(List<RunLength> runLengths, ImageData referenceImage)
        {
            ImageData.AssertValid(referenceImage);
            return ToShapeImageData(runLengths, referenceImage.Width, referenceImage.Height);
        }

        public static ImageData ToShapeImageData(List<RunLength> runLengths)
             => new ImageData().Create(ToShapeMatrix(runLengths));
        



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
