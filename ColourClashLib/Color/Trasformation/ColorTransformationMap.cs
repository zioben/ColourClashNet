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

        public bool Create(ImageData image)
        {           
            Reset();
            if (image == null || image.Colors <= 0 )
            {
                return false;
            }
            return Create(image.ColorPalette);
        }

        public void Add(int sourceRGB, int destRGB)
        {
            if (sourceRGB < 0 || destRGB < 0)
            {
                return;
            }
            rgbTransformationMap[sourceRGB] = destRGB;
        }

        public void Remove(int sourceRGB)
        {
            if (rgbTransformationMap.ContainsKey(sourceRGB))
            {
                rgbTransformationMap.TryRemove(sourceRGB, out int Removed);
            }
        }

        public ImageData Transform(ImageData image, CancellationToken token=default )
        {
            string sM = nameof(Transform);
            if ((!image?.IsValid ?? true))
            {
                return new();
            }
            var oDataOut = new int[image.Height,image.Width];
            //Parallel.For(0, image.Height, r =>
            for( int r =0; r<image.Height; r++)
            {
                token.ThrowIfCancellationRequested();
                for (int c = 0; c < image.Width; c++)
                {
                    if (!rgbTransformationMap.TryGetValue(image.matrix[r, c], out oDataOut[r, c]))
                    {
                        oDataOut[r, c] = Defaults.ColorDefaults.DefaultInvalidColorInt;
                    }
                }
            }//);
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
