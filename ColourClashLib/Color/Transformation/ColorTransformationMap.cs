using ColourClashNet.Color;
using ColourClashNet.Imaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Class to transform palette colors.
    /// </summary>
    public partial class ColorTransformationMap
    {
        static string sC = nameof(ColorTransformationMap);
        public ConcurrentDictionary<int, int> RgbTransformationMap { get; private init; } = new();

        public void Reset()
        {
            RgbTransformationMap.Clear();
        }

        public int Colors => RgbTransformationMap.Count;

        public bool Create(Palette palette)
        {
            Reset();
            if (palette == null)
            {
                return false;
            }
            var rgbList = palette.ToList();
            foreach (var rgb in rgbList)
            {
                if ( rgb>=0 )
                {
                    RgbTransformationMap[rgb] = rgb;
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
            RgbTransformationMap[sourceRGB] = destRGB;
        }

        public void Remove(int sourceRGB)
        {
            if (RgbTransformationMap.ContainsKey(sourceRGB))
            {
                RgbTransformationMap.TryRemove(sourceRGB, out int removed);
            }
        }

        public ImageData Transform(ImageData image, CancellationToken token=default )
        {
            string sM = nameof(Transform);
            ImageData.AssertValid(image);
            var dataOut = new int[image.Rows,image.Columns];
            //Parallel.For(0, image.Height, r =>
            for( int r =0; r<image.Rows; r++)
            {
                token.ThrowIfCancellationRequested();
                for (int c = 0; c < image.Columns; c++)
                {
                    var rgb = image.matrix[r, c];
                    if (!RgbTransformationMap.TryGetValue(rgb, out dataOut[r, c]))
                    {
                        // Tagged pixels (negative: transparent, mask, ...) not in the map are kept unchanged
                        dataOut[r, c] = rgb < 0 ? rgb : ColorDefaults.DefaultInvalidColorInt;
                    }
                }
            }//);
            return new ImageData().Create(dataOut);
        }


        public Palette GetOutputPalette()
        {
            var newPalette = new Palette();
            foreach (var kvp in RgbTransformationMap)
            {
                if (kvp.Value < 0)
                    continue;
                newPalette.Add(kvp.Value);
            }
            return newPalette; 
        }

        public Palette GetInputPalette()
        {
            var newPalette = new Palette();
            foreach (var kvp in RgbTransformationMap)
            {
                if (kvp.Key < 0)
                    continue;
                newPalette.Add(kvp.Key);
            }
            return newPalette;
        }
    }
}
