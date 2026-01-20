using ColourClashLib.Color;
using ColourClashNet.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformBkgRemover : ColorTransformBase
    {
        static string sC = nameof(ColorTransformBkgRemover);
        public ColorTransformBkgRemover()
        {
            Type = ColorTransformType.ColorRemover;
            Description = "Basic Background Color Replacement";
        }

        public Palette BackgroundPalette { get; set; } = new Palette();
        public int ColorBackgroundReplacement { get; set; } = 0;



        public override ColorTransformInterface SetProperty(ColorTransformProperties propertyName, object value)
        {
            base.SetProperty(propertyName, value);
            switch (propertyName)
            {
                case ColorTransformProperties.ColorBackgroundList:
                    {
                        BackgroundPalette = new Palette();
                        if (value is IEnumerable<int> list)
                            BackgroundPalette = Palette.CreatePalette(list);
                        else if (value is Palette palette)
                            BackgroundPalette = Palette.CreatePalette(palette);
                        else
                            throw new ArgumentException($"Invalid value type for {propertyName}: {value.GetType().Name}");
                    }
                    break;
                case ColorTransformProperties.ColorBackgroundReplacement:
                    if (value is int rgb)
                        ColorBackgroundReplacement = rgb;
                    else if (value is ColorConverterInterface col)
                        ColorBackgroundReplacement = col.ToIntRGB();
                    else
                        throw new ArgumentException($"Invalid value type for {propertyName}: {value.GetType().Name}");
                    break;
                default:
                    break;
            }
            return this;
        }

        protected async override Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken token = default)
        {
            return await Task.Run(() =>
            {
                string sM = nameof(CreateTransformationMap);
                LogMan.Trace(sC, sM, $"{Type} : Creating trasformation map");

                TransformationMap.Reset();
                var oBkgList = BackgroundPalette.ToList();
                var oPalList = SourceData.ColorPalette.ToList();
                foreach (var rgb in oPalList)
                {
                    TransformationMap.Add(rgb, rgb);
                }
                foreach (var rgb in oBkgList)
                {
                    TransformationMap.Remove(rgb);
                    TransformationMap.Add(rgb, ColorBackgroundReplacement);
                }
                return ColorTransformResults.CreateValidResult();
            }, token);
        }
    }
}
