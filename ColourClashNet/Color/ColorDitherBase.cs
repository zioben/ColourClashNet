using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public abstract class ColorDitherBase : ColorDitherInterface
    {
        public string Description { get; protected set; }
        public string Name { get; protected set; }
        public abstract bool Create();

        public abstract int[,] Dither(int[,] oDataProcessed, List<int> oDataProcessedPalette, int[,] oDataOriginal, ColorDistanceEvaluationMode eDistanceMode);

        protected int[,] Transform(int[,] oDataProcessed, List<int> oDataProcessedPalette, ColorDistanceEvaluationMode eDistanceMode)
        {
            if (oDataProcessed == null || oDataProcessedPalette == null || oDataProcessedPalette.Count == 0)
                return null;
            var R = oDataProcessed.GetLength(0);
            var C = oDataProcessed.GetLength(1);
            var hashPalette = new HashSet<int>();
            foreach (var p in oDataProcessed)
                hashPalette.Add(p);

            Dictionary<int, int> oColorTransformation = new Dictionary<int, int>();
            foreach (var col in hashPalette)
            {
                oColorTransformation.Add(col, ColorTransformBase.GetNearestColor(col, oDataProcessedPalette, eDistanceMode));
            }

            var oRet = new int[R, C];

            Parallel.For(0, R, r =>
                    {
                        for (int c = 0; c < C; c++)
                        {
                            var col = oDataProcessed[r, c];
                            if (col < 0 || !oColorTransformation.ContainsKey(col))
                                oRet[r, c] = -1;
                            else
                                oRet[r, c] = oColorTransformation[col];

                        }
                    });
            return oRet;
        }
    }
}
