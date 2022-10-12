using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    public abstract class ColorTransformBase : ColorTransform
    {
        static int[,,] Histogram = new int[256, 256, 256];
        static object locker = new object();

        public int ResultColors { get; internal set; } = 0;
        static void BuildColorHistHistogram(ColorItem[,] m, ColorItem oBackColor, Dictionary<ColorItem, int> oDict)
        {
            oDict.Clear();
            lock (locker)
            {
                Array.Clear(Histogram);
                int R = m.GetLength(0);
                int C = m.GetLength(1);
                for (int r = 0; r < R; r++)
                {
                    for (int c = 0; c < C; c++)
                    {
                        var col = m[r, c];
                        if (col.Valid && !col.Equals(oBackColor))
                        {
                            Histogram[col.R, col.G, col.B]++;
                        }
                    }
                }
                for (int r = 0; r < 256; r++)
                {
                    for (int g = 0; g < 256; g++)
                    {
                        for (int b = 0; b < 256; b++)
                        {
                            int val = Histogram[r, g, b];
                            if (val != 0)
                            {
                                oDict.Add(new ColorItem(r, g, b), val);
                            }
                        }
                    }
                }
            }
        }
        static void BuildColorHistDirect(ColorItem[,] m, ColorItem oBackColor, Dictionary<ColorItem, int> oDict)
        {
            oDict.Clear();
            int R = m.GetLength(0);
            int C = m.GetLength(1);
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var col = m[r, c];
                    if (col.Valid && !col.Equals(oBackColor))
                    {
                        if (oDict.ContainsKey(col))
                            oDict[col]++;
                        else
                            oDict[col]=1;
                    }
                }
            }
        }

        static void BuildColorHist(ColorItem[,] m, ColorItem oBackColor, Dictionary<ColorItem, int> oDict)
        {
            if (m == null)
                return;
            if (oDict == null)
                return;
            if (m.Length < 64 * 64)
                BuildColorHistDirect(m, oBackColor, oDict);
            else
                BuildColorHistHistogram(m, oBackColor, oDict);
        }

        public ColorItem BackColor { get; private set; } = new ColorItem();
        public ColorItem BackColorTransform { get; private set; } = new ColorItem();

        public Dictionary<ColorItem, int> DictHistogram { get; private init; } = new Dictionary<ColorItem, int>();

        public Dictionary<ColorItem, ColorItem> DictTransform { get; private init; } = new Dictionary<ColorItem, ColorItem>();

        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.All;

        abstract protected void BuildTrasformation();

        protected ColorItem[,] oDataSource;

        void Reset()
        {
            BackColor = new ColorItem();
            BackColorTransform = new ColorItem();
            DictHistogram.Clear();
            DictTransform.Clear();
            oDataSource = null;
        }

        public void Create(ColorItem[,] oSource, ColorItem oBackColor )
        {
            Reset();
            if (oSource == null)
                return;
            BackColor = oBackColor;
            BuildColorHist(oSource,oBackColor,DictHistogram);
            SortColors();
            BuildTrasformation();
        }

        public void Create(Dictionary<ColorItem, int> oSourceHistogram, ColorItem oBackColor)
        {
            Reset();
            if (oSourceHistogram==null)
                return;
            BackColor = oBackColor;
            foreach (var kvp in oSourceHistogram)
            {
                if (!kvp.Key.Equals(oBackColor))
                    DictHistogram.Add(kvp.Key, kvp.Value);
            }
            BuildTrasformation();
        }

        public ColorItem[,] Transform(ColorItem[,] oSource)
        {
            if (oSource == null)
                return null;
            if (DictTransform == null || DictTransform.Count == 0)
                return null;


            BackColorTransform = new ColorItem();
            if (DictTransform.ContainsKey(BackColor))
            {
                BackColorTransform = DictTransform[BackColor]; 
            }

            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new ColorItem[R, C];
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var col = oSource[r, c];
                    if (col.Equals(BackColor) || !DictTransform.ContainsKey(col) )
                        oRet[r, c] = new ColorItem();
                    else
                        oRet[r, c] = DictTransform[col];
                }
            }
            return oRet;
        }

        // Sort reverse in order
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

        protected void SortColors()
        {
            SortedList<int, ColorItem> list = new SortedList<int, ColorItem>(new DuplicateKeyComparer<int>());
            foreach (var kvp in DictHistogram)
            {
                list.Add(kvp.Value, kvp.Key);
            }
            DictHistogram.Clear();
            foreach (var kvp in list)
            {
                DictHistogram.Add(kvp.Value, kvp.Key);
            }
        }

        protected List<ColorItem> DictToList()
        {
            List<ColorItem> listP = new List<ColorItem>();
            foreach (var kvp in DictHistogram)
            {
                listP.Add(kvp.Key);
            }
            return listP;
        }

    }
}
