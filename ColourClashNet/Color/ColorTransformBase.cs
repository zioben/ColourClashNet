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
        public Dictionary<int, int> oColorTransformationMap { get; private init; } = new Dictionary<int, int>();

        protected HashSet<int> hashColorsPalette { get; private init; } = new HashSet<int>();
        public List<int> oColorTransformationPalette => hashColorsPalette.ToList();
        public int ColorsUsed => hashColorsPalette?.Count ?? 0;

        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.All;

        abstract protected void CreateTrasformationMap();
        abstract public int[,]? Transform( int[,]? oDataSource, Dictionary<Parameters, object>? oParameters);

        public string Name { get; protected set; } = "";
        public string Description { get; protected set; } = "";

        void Reset()
        {
            oColorHistogram.Clear();
            oColorTransformationMap.Clear();
            hashColorsPalette.Clear();
        }

        public void Create(int[,]? oDataSource )
        {
            Reset();
            if (oDataSource == null)
                return;
            oColorHistogram = CreateColorHist(oDataSource);
            CreateTrasformationMap();
        }

        public void Create(Dictionary<int, int>? oDictColorHistogramSource)
        {
            Reset();
            if (oDictColorHistogramSource == null )
                return;
            foreach (var kvp in oDictColorHistogramSource)
            {
                oColorHistogram.Add(kvp.Key, kvp.Value);
                hashColorsPalette.Add(kvp.Key);
            }
            CreateTrasformationMap();
            hashColorsPalette.Remove(-1);
        }

        public void Create(ColorTransformInterface? oTrasformationSource)
        {
            Create(oTrasformationSource?.oColorHistogram);
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

        protected int[,]? ApplyTransform(int[,]? oDataSource)
        {
            return ApplyTransform(oDataSource, this);
        }
    }
}
