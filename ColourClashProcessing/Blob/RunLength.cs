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

        public static int[,] ToMask(List<RunLength> runLengths, int width, int height, int foreRGB, int bkgRGB)
        {
            string sM = nameof(ToMask);
            try
            {
                if (runLengths == null)
                    throw new ArgumentNullException(nameof(runLengths));
                if (width <= 0 || height <= 0)
                    throw new ArgumentException("Width and Height must be positive.");

                var matrix = MatrixTools.CreateMatrix(width, height, bkgRGB);
                foreach (var rl in runLengths)
                {
                    for (int c = rl.XS; c <= rl.XE; c++)
                    {
                        matrix[rl.Y, c] = foreRGB;
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


        public static int[,] ToMask(List<RunLength> runLengths, int foreRGB, int bkgRGB)
        {
            string sM = nameof(ToMask);
            try
            {
                if (runLengths == null)
                    throw new ArgumentNullException(nameof(runLengths));
                int xMin = runLengths.Min(R => R.XS);
                int xMax = runLengths.Max(R => R.XE);
                int yMin = runLengths.Min(R => R.Y);
                int yMax = runLengths.Max(R => R.Y);
                var width = xMax - xMin+1;
                var height= yMax - yMin+1;
                if (width <= 0 || height <= 0)
                    throw new ArgumentException("Width and Height must be positive.");

                var matrix = MatrixTools.CreateMatrix(width, height, bkgRGB);
                foreach (var rl in runLengths)
                {
                    for (int c = rl.XS; c <= rl.XE; c++)
                    {
                        matrix[rl.Y-yMin, c-xMin] = foreRGB;
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


        public static ImageData ToImageData(List<RunLength> runLengths, int width, int height, int foreRGB, int bkgRGB)
            => new ImageData().Create(ToMask(runLengths, width, height, foreRGB, bkgRGB));


        public static ImageData ToImageData(List<RunLength> runLengths, ImageData referenceImage, int foreRGB, int bkgRGB)
        {
            ImageData.AssertValid(referenceImage);
            return ToImageData(runLengths, referenceImage.Width, referenceImage.Height, foreRGB, bkgRGB);
        }


        public static bool ToMatrix(List<RunLength> runLengths, int[,] outputMatrix, int foreRGB)
        {
            string sM = nameof(ToMatrix);
            if(outputMatrix==null)
                throw new ArgumentNullException(nameof(outputMatrix));
            var W = outputMatrix.GetLength(1);
            var H = outputMatrix.GetLength(0);
            var yMin = runLengths.Min(R => R.Y);
            var yMax = runLengths.Max(R => R.Y);
            var xMin = runLengths.Min(R => R.XS);
            var xMax = runLengths.Max(R => R.XE);
            if (yMin < 0 || yMax >= H || xMin < 0 || xMax >= W)
            {
                LogMan.Error(sC, sM, $"Run length dimensions [{xMin},{yMin};{xMax},{yMax}] exceeded matrix dimension [0,0;{W},{H}]");
                return false;
            }
            foreach (var rl in runLengths)
            {
                for (int c = rl.XS; c <= rl.XE; c++)
                {
                    outputMatrix[rl.Y, c] = foreRGB;
                }
            }
            return true;
        }

        public static ImageData ToImageData(List<RunLength> runLengths, int foreRGB, int bkgRGB)
            => new ImageData().Create(ToMask(runLengths, foreRGB, bkgRGB));
    }
}
