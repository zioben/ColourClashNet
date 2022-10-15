using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public abstract partial class ColorTransformBase : ColorTransformInterface
    {

        public int ColorsUsed { get; internal set; } = 0;

        public SortedList<int, int> ListColorHistogram { get; private init; } = new SortedList<int, int>();
        public SortedList<int, int> ListColorTransformation { get; private init; } = new SortedList<int, int>();
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.All;

        abstract protected void BuildTrasformation();

        protected int[,] oDataSource;

        void Reset()
        {
            ColorsUsed = 0;
            ListColorHistogram.Clear();
            ListColorTransformation.Clear();
            oDataSource = null;
        }

        public void Create(int[,] oDataSource )
        {
            Reset();
            if (oDataSource == null)
                return;
            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    ListColorHistogram[oDataSource[r, c]]++;
                }
            }
            SortColorsByHistogram();
            BuildTrasformation();
        }

        public void Create(SortedList<int,int> oDictColorHistogramSource)
        {
            Reset();
            if (oDictColorHistogramSource == null )
                return;
            foreach (var kvp in oDictColorHistogramSource)
            {
                ListColorHistogram.Add(kvp.Key, kvp.Value);
            }
            SortColorsByHistogram();
            BuildTrasformation();
        }

        public void Create(ColorTransformInterface oTrasformationSource)
        {
            Reset();
            Create(oTrasformationSource?.ListColorHistogram);
        }

        List<List<int>> ToListList(int[,] oSource)
        {
            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new List<List<int>>(R);
            for (int r = 0; r < R; r++)
            {
                var lColor = new List<int>(C);
                for (int c = 0; c < C; c++)
                {
                    lColor.Add(oSource[r, c]);
                }
                oRet.Add(lColor);   
            }
            return oRet;
        }

        public int[,] Transform(int[,] oSource)
        {
            if (oSource == null)
                return null;
            if (ListColorTransformation == null || ListColorTransformation.Count == 0)
                return null;
            var lListList = ToListList(oSource);
            Parallel.ForEach(lListList, lList =>
            {
                for (int i = 0; i < lList.Count; i++)
                {
                    var col = lList[i];
                    if (col < 0 || !ListColorTransformation.ContainsKey(col))
                        lList[i] = -1;
                    else
                        lList[i] = ListColorTransformation[col];
                }
            });
            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new int[R, C];
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    oRet[r,c] = lListList[r][c];
                }
            }
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
            SortedList<int, int> list = new SortedList<int, int>(new DuplicateKeyComparer<int>());
            foreach (var kvp in ListColorTransformation)
            {
                list.Add(kvp.Value, kvp.Key);
            }
            ListColorHistogram.Clear();
            foreach (var kvp in list)
            {
                ListColorHistogram.Add(kvp.Value, kvp.Key);
            }
        }
    }
}
