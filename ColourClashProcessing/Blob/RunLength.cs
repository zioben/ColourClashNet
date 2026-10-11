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


        public static ImageData ToImageData(List<RunLength> runLengths, int Width, int Height )
        {
            string sM = nameof(ToImageData);
            try
            {
                if (runLengths == null)
                    throw new ArgumentNullException(nameof(runLengths));
                if (Width <= 0 || Height <= 0)
                    throw new ArgumentException("Width and Height must be positive.");

                var matrix = new int[Height, Width];
                var col = ColorIntExt.FromRGB(255, 255, 255);
                foreach (var rl in runLengths)
                {
                    for (int c = rl.XS; c <= rl.XE; c++)
                    {
                        matrix[rl.Y, c] = col;
                    }
                }
                var imageOut = new ImageData().Create(matrix);
                return imageOut;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"Operation failed", ex);
                throw;
            }
        }
    }
}
