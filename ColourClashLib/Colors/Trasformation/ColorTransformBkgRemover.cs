using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ColourClashLib;
using ColourClashNet.Colors;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformBkgRemover : ColorTransformBase
    {
        static string sClass = nameof(ColorTransformBkgRemover);
        public ColorTransformBkgRemover()
        {
            Name = ColorTransformType.ColorRemover;
            Description = "Substitute a colorlist with a single color";
        }

        public List<int> ColorBackgroundList { get; set; } = new List<int>();
        public int ColorBackgroundReplacement { get; set; } = 0;

        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            if (base.SetProperty(eProperty, oValue) != null)
                return this;
            switch (eProperty)
            {
                case ColorTransformProperties.ColorBackgroundList:
                    {
                        ColorBackgroundList = oValue as List<int>;
                        if (ColorBackgroundList != null)
                        {
                            return this;
                        }
                    }
                    break;
                case ColorTransformProperties.ColorBackgroundReplacement:
                    if (int.TryParse(oValue?.ToString(), out var rgb))
                    {
                        ColorBackgroundReplacement = rgb;
                        return this;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }


        protected override void CreateTrasformationMap()
        {
            string sMethod = nameof(CreateTrasformationMap);
            if (ColorDefaults.Trace)
                Trace.TraceInformation($"{sClass}.{sMethod} ({Name}) : Creating trasformation map");
            foreach (var rgb in Palette.rgbPalette)
            {
                if (rgb < 0)
                    continue;
                ColorTransformationMapper.Add(rgb, rgb);
            }

            foreach (var rgb in Palette.rgbPalette)
            {
                if (rgb < 0)
                    continue;
                if (ColorBackgroundList.Any(X => X.Equals(rgb)))
                {
                    Palette.Remove(rgb);
                    ColorTransformationMapper.rgbTransformationMap[rgb] = ColorBackgroundReplacement;
                }
            }
        }
    }
}
