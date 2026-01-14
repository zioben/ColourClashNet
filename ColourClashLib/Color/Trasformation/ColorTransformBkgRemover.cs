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

      

        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            base.SetProperty(eProperty, oValue);
            switch (eProperty)
            {
                case ColorTransformProperties.ColorBackgroundList:
                    {
                        BackgroundPalette = new Palette();
                        if (oValue is List<int> oList)
                        {
                            BackgroundPalette = Palette.CreatePalette(oList);
                            if (BackgroundPalette == null)
                            {
                                BackgroundPalette = new Palette();
                            }
                        }
                        else if (oValue is Palette oPalette)
                        {
                            BackgroundPalette = oPalette;
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
            return this;
        }

        protected async override Task<ColorTransformResults> CreateTrasformationMapAsync(CancellationToken? oToken)
        {
            return await Task.Run(() =>
            {
                string sM = nameof(CreateTrasformationMapAsync);
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
               // var lRepl = TransformationMap.rgbTransformationMap.Where(X => X.Value == ColorBackgroundReplacement).ToList();
                return ColorTransformResults.CreateValidResult();
            });
        }

        // Not needed
        //protected override async Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken? oToken)
    }
}
