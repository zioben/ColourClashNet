using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color
{
    /// <summary>
    /// Class to transform palette colors.
    /// </summary>
    public partial class ColorTransformationMap
    {
        static string sC = nameof(ColorTransformationMap);
        public Dictionary<int, int> rgbTransformationMap { get; private init; } = new Dictionary<int, int>();

        public void Reset()
        {
            rgbTransformationMap.Clear();
        }

        public int Colors => rgbTransformationMap.Count;

        public bool Create(Palette oPalette)
        {
            Reset();
            if (oPalette == null)
            {
                return false;
            }
            foreach (var rgb in oPalette.rgbPalette)
            {
                if ( rgb>=0 )
                {
                    rgbTransformationMap[rgb] = rgb;
                }
            }
            return true;
        }

        public bool Create(int[,] oDataSource)
        {
            Reset();
            if (oDataSource == null)
            {
                return false;
            }
            Palette oPalette = new Palette();
            foreach (var rgb in oDataSource) 
            {
                if (rgb >= 0)
                {
                    oPalette.Add(rgb);
                }
            }
            return Create(oPalette);
        }

        public void Add(int iSorceRGB, int iDestRGB)
        {
            if (iSorceRGB < 0 || iDestRGB < 0)
            {
                return;
            }
            rgbTransformationMap[iSorceRGB] = iDestRGB;
        }
        public void Remove(int iSorceRGB)
        {
            if (rgbTransformationMap.ContainsKey(iSorceRGB))
            {
                rgbTransformationMap.Remove(iSorceRGB);
            }
        }

        public async Task<int[,]?> TransformAsync(int[,]? oData, CancellationToken? oToken )
        {
            return await Task.Run(() =>
            {
                string sM = nameof(TransformAsync);
                if (oData == null)
                {
                    return null;
                }
                int R = oData.GetLength(0);
                int C = oData.GetLength(1);
                var oRet = new int[R, C];
                Parallel.For(0, R, r =>
                {
                    oToken?.ThrowIfCancellationRequested();
                    for (int c = 0; c < C; c++)
                    {
                        if (!rgbTransformationMap.TryGetValue(oData[r, c], out oRet[r, c]))
                        {
                            oRet[r, c] = Defaults.ColorDefaults.DefaultInvalidColorInt;
                        }
                    }
                });
                return oRet;
            });
        }


        public Palette ToColorPalette()
        {
            var oCP = new Palette();
            foreach (var kvp in rgbTransformationMap)
            {
                if (kvp.Value < 0)
                    continue;
                oCP.Add(kvp.Value);
            }
            return oCP; 
        }       
    }
}
