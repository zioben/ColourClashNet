using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ColourClashLib;
using ColourClashLib.Color;
using ColourClashNet.Colors;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformBkgRemover : ColorTransformBase
    {
        static string sClass = nameof(ColorTransformBkgRemover);
        public ColorTransformBkgRemover()
        {
            Type = ColorTransformType.ColorRemover;
            Description = "Substitute a colorlist with a single color";
        }

        public ColorPalette BackgroundPalette { get; set; } = new ColorPalette();
        public int ColorBackgroundReplacement { get; set; } = 0;

        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            if (base.SetProperty(eProperty, oValue) != null)
                return this;
            switch (eProperty)
            {
                case ColorTransformProperties.ColorBackgroundList:
                    {
                        BackgroundPalette = new ColorPalette();
                        if (oValue is List<int> oList)
                        {
                            BackgroundPalette = ColorPalette.CreateColorPalette(oList);
                            if (BackgroundPalette == null)
                            {
                                BackgroundPalette = new ColorPalette();
                            }
                        }
                        else if (oValue is ColorPalette oPalette)
                        {
                            BackgroundPalette = oPalette;
                        }
                        return this;
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


       
        protected override int[,]? ExecuteTransform(int[,]? oDataSource, CancellationToken oToken)
        {
            if (oDataSource == null)
                return null;
            var R = oDataSource.GetLength(0);
            var C = oDataSource.GetLength(1);
            var oRet = new int[R, C];   
            var oList = BackgroundPalette.ToList();
            var oBkgCol = ColorBackgroundReplacement;
            //oBkgCol = oBkgCol.SetColorInfo(ColorIntType.IsBkg);
            Parallel.For(0, R, r =>
            {
                for (int c = 0; c < C; c++)
                {
                    oRet[r, c] = oDataSource[r, c];
                    if (oRet[r, c].GetColorInfo() == ColorIntType.IsColor)
                    {
                        if (oList.Any(X => X == oRet[r, c]))
                        {
                            oRet[r, c] = oBkgCol;
                        }
                    }
                }
            });
            return oRet;
        }
    }
}
