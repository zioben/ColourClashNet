using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public abstract partial class ColorTransformBase : ColorTransformInterface
    {

        public Dictionary<int, int> oColorHistogram { get; private set; } = new Dictionary<int, int>();
        public Dictionary<int, int> oColorTransformation { get; private init; } = new Dictionary<int, int>();

        protected HashSet<int> oColorsPalette { get; private init; } = new HashSet<int>();
        public List<int> oColorTransformationPalette => oColorsPalette.ToList();
        public int ColorsUsed => oColorsPalette?.Count ?? 0;

        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.All;

        abstract protected void BuildTrasformation();
        abstract public int[,] Transform( int[,] oDataSource );

        void Reset()
        {
            oColorHistogram.Clear();
            oColorTransformation.Clear();
            oColorsPalette.Clear();
        }

        public void Create(int[,] oDataSource )
        {
            Reset();
            if (oDataSource == null)
                return;
            oColorHistogram = CreateColorHist(oDataSource);
            SortColorsByHistogram();
            BuildTrasformation();
        }

        public void Create(Dictionary<int, int> oDictColorHistogramSource)
        {
            Reset();
            if (oDictColorHistogramSource == null )
                return;
            foreach (var kvp in oDictColorHistogramSource)
            {
                oColorHistogram.Add(kvp.Key, kvp.Key);
            }
            BuildTrasformation();
            oColorsPalette.Remove(-1);
        }

        public void Create(ColorTransformInterface oTrasformationSource)
        {
            Create(oTrasformationSource?.oColorHistogram);
        }

        public int[,] TransformBase(int[,] oSource)
        {
            if (oSource == null)
                return null;
            if (oColorTransformation == null || oColorTransformation.Count == 0)
                return null;
            // var lListList = ToListList(oSource);
            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new int[R, C];
            Parallel.For(0, R, r =>
            {
                for (int c = 0; c < C; c++)
                {
                    var col = oSource[r, c];
                    if (col < 0 || !oColorTransformation.ContainsKey(col))
                        oRet[r, c] = -1;
                    else
                        oRet[r, c] = oColorTransformation[col];

                }
            });
            return oRet;
        }

        // Sort in reverse in order
        public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
        {
            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1; // Handle equality as being greater. Note: this will break Remove(key) or
                else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
                    return -result;
            }
        }

        protected void SortColorsByHistogram()
        {
            List<Tuple<int,int>> oListHist = new List<Tuple<int, int>>(oColorHistogram.Count);
            foreach (var kvp in oColorHistogram)
            {
                oListHist.Add(Tuple.Create( kvp.Value, kvp.Key));
            }
            oListHist = oListHist.OrderByDescending(X => X.Item1).ToList();
            oColorHistogram.Clear();
            foreach (var kvp in oListHist)
            {
                oColorHistogram.Add( kvp.Item2, kvp.Item1);
            }
        }
    }
}
