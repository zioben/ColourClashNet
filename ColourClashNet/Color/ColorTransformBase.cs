using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace ColourClashNet.Colors
{
    public abstract partial class ColorTransformBase : ColorTransformInterface
    {
        public ColorTransform Type { get; protected init; }

        public Dictionary<int, int> ColorHistogram { get; private set; } = new Dictionary<int, int>();
        public Dictionary<int, int> ColorTransformationMap { get; private init; } = new Dictionary<int, int>();

        public HashSet<int> ColorTransformationPalette { get; private init; } = new HashSet<int>();
        public int ColorsUsed => ColorTransformationPalette?.Count ?? 0;

        public ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; set; } = ColorDistanceEvaluationMode.All;

        abstract protected void CreateTrasformationMap();
        protected virtual int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            return ExecuteStdTransform(oDataSource,this);
        }

        public string Description { get; protected set; } = "";
        public ColorDitherInterface? Dithering { get; set; } = null;

        protected bool BypassDithering { get; set; }

        void Reset()
        {
            ColorHistogram.Clear();
            ColorTransformationMap.Clear();
            ColorTransformationPalette.Clear();
        }


        public void Create(int[,]? oDataSource )
        {
            Reset();
            if (oDataSource == null)
                return;
            ColorHistogram = CreateColorHist(oDataSource);
            CreateTrasformationMap();
        }

        public void Create(Dictionary<int, int>? oDictColorHistogramSource)
        {
            Reset();
            if (oDictColorHistogramSource == null )
                return;
            foreach (var kvp in oDictColorHistogramSource)
            {
                ColorHistogram.Add(kvp.Key, kvp.Value);
                ColorTransformationPalette.Add(kvp.Key);
            }
            CreateTrasformationMap();
            ColorTransformationPalette.Remove(-1);
        }

        public void Create(HashSet<int>? oColorPalette)
        {
            Reset();
            if (oColorPalette == null)
                return;
            foreach (var kvp in oColorPalette)
            {
                ColorHistogram.Add(kvp, kvp);
                ColorTransformationPalette.Add(kvp);
            }
            CreateTrasformationMap();
            ColorTransformationPalette.Remove(-1);
        }

        public int[,]? TransformAndDither(int[,]? oDataSource)
        {
            if (oDataSource == null)
            {
                return null;
            }
            var oProc = ExecuteTransform(oDataSource);
            if (oProc == null || Dithering == null || BypassDithering)
            {
                return oProc;
            }
            if (!Dithering.Create())
            {
                return oProc;
            }
            var oProcDither = Dithering.Dither(oDataSource, oProc, ColorTransformationPalette, ColorDistanceEvaluationMode);

            //var oTransColorRemap = new ColorTransformToPalette() { ColorDistanceEvaluationMode = ColorDistanceEvaluationMode };
            //oTransColorRemap.Create(ColorTransformationPalette);
            //var oRemapData = oTransColorRemap.TransformAndDither(oProcDither);
            return oProcDither;
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
            List<Tuple<int,int>> oListHist = new List<Tuple<int, int>>(ColorHistogram.Count);
            foreach (var kvp in ColorHistogram)
            {
                oListHist.Add(Tuple.Create( kvp.Value, kvp.Key));
            }
            oListHist = oListHist.OrderByDescending(X => X.Item1).ToList();
            ColorHistogram.Clear();
            foreach (var kvp in oListHist)
            {
                ColorHistogram.Add( kvp.Item2, kvp.Item1);
            }
        }

    }
}
