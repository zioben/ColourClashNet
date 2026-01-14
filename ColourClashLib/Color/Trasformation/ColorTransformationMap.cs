using ColourClashNet.Imaging;
using System;
using System.Collections.Concurrent;
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
        public ConcurrentDictionary<int, int> rgbTransformationMap { get; private init; } = new();

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
            var rgbList = oPalette.ToList();
            foreach (var rgb in rgbList)
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
        public bool Create(ImageData oImageData)
        {           
            Reset();
            if (oImageData == null || oImageData.Colors <= 0 )
            {
                return false;
            }
            return Create(oImageData.ColorPalette);
        }

        public void Add(int iSourceRGB, int iDestRGB)
        {
            if (iSourceRGB < 0 || iDestRGB < 0)
            {
                return;
            }
            rgbTransformationMap[iSourceRGB] = iDestRGB;
        }

        public void Remove(int iSourceRGB)
        {
            if (rgbTransformationMap.ContainsKey(iSourceRGB))
            {
                rgbTransformationMap.TryRemove(iSourceRGB, out int Removed);
            }
        }

        public async Task<ImageData> TransformAsync(ImageData oImageData, CancellationToken? oToken )
        {
            string sM = nameof(TransformAsync);
            if (oImageData == null || !oImageData.DataValid)
            {
                return null;
            }
            var oDataOut = new int[oImageData.Height,oImageData.Width];
            Parallel.For(0, oImageData.Height, r =>
            {
                oToken?.ThrowIfCancellationRequested();
                for (int c = 0; c < oImageData.Width; c++)
                {
                    if (!rgbTransformationMap.TryGetValue(oImageData.Data[r, c], out oDataOut[r, c]))
                    {
                        oDataOut[r, c] = Defaults.ColorDefaults.DefaultInvalidColorInt;
                    }
                }
            });
            return new ImageData().Create(oDataOut);
        }


        public Palette GetOutputPalette()
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

        public Palette GetInputPalette()
        {
            var oCP = new Palette();
            foreach (var kvp in rgbTransformationMap)
            {
                if (kvp.Key < 0)
                    continue;
                oCP.Add(kvp.Key);
            }
            return oCP;
        }
    }
}
