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
        public Dictionary<ColorItem, int> DictColorHistogram { get; private init; } = new Dictionary<ColorItem, int>();
        public Dictionary<ColorItem, ColorItem> DictColorTransformation { get; private init; } = new Dictionary<ColorItem, ColorItem>();
        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.All;

        abstract protected void BuildTrasformation();

        protected ColorItem[,] oDataSource;

        void Reset()
        {
            ColorsUsed = 0;
            DictColorHistogram.Clear();
            DictColorTransformation.Clear();
            oDataSource = null;
        }

        public void Create(ColorItem[,] oDataSource )
        {
            Reset();
            if (oDataSource == null)
                return;
            BuildColorHist(oDataSource,DictColorHistogram);
            SortColorsByHistogram();
            BuildTrasformation();
        }

        public void Create(Dictionary<ColorItem,int> oDictColorHistogramSource)
        {
            Reset();
            if (oDictColorHistogramSource == null )
                return;
            foreach (var kvp in oDictColorHistogramSource)
            {
                DictColorHistogram.Add(kvp.Key, kvp.Value);
            }
            SortColorsByHistogram();
            BuildTrasformation();
        }

        public void Create(ColorTransformInterface oTrasformationSource)
        {
            Reset();
            Create(oTrasformationSource?.DictColorHistogram);
        }

        public ColorItem[,] Transform(ColorItem[,] oSource)
        {
            if (oSource == null)
                return null;
            if (DictColorTransformation == null || DictColorTransformation.Count == 0)
                return null;

            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new ColorItem[R, C];
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var col = oSource[r, c];
                    if (!col.Valid || !DictColorTransformation.ContainsKey(col))
                        oRet[r, c] = new ColorItem(-1, -1, -1);
                    else
                        oRet[r, c] = DictColorTransformation[col];
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
            SortedList<int, ColorItem> list = new SortedList<int, ColorItem>(new DuplicateKeyComparer<int>());
            foreach (var kvp in DictColorHistogram)
            {
                list.Add(kvp.Value, kvp.Key);
            }
            DictColorHistogram.Clear();
            foreach (var kvp in list)
            {
                DictColorHistogram.Add(kvp.Value, kvp.Key);
            }
        }

        protected List<ColorItem> DictToList()
        {
            List<ColorItem> listP = new List<ColorItem>();
            foreach (var kvp in DictColorHistogram)
            {
                listP.Add(kvp.Key);
            }
            return listP;
        }

    }
}
