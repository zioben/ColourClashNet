using ColourClashNet.Log;
using System;
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
            if (base.SetProperty(eProperty, oValue) != null)
                return this;
            switch (eProperty)
            {
                case ColorTransformProperties.ColorBackgroundList:
                    {
                        BackgroundPalette = new Palette();
                        if (oValue is List<int> oList)
                        {
                            BackgroundPalette = Palette.CreateColorPalette(oList);
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

        
        protected override async Task<ColorTransformResults> ExecuteTransformAsync( CancellationToken? oToken)
        {
            return await Task.Run(() =>
            {
                string sM = nameof(ExecuteTransformAsync);                
                var oRes = new ColorTransformResults()
                {
                    DataSource = SourceData,
                };
                try
                {
                    if (SourceData == null)
                    {
                        LogMan.Error(sC, sM, $"{Type} : Null SourceData");
                        return oRes;
                    }
                    var R = SourceData.GetLength(0);
                    var C = SourceData.GetLength(1);
                    var oRet = new int[R, C];
                    var oList = BackgroundPalette.ToList();
                    var oBkgCol = ColorBackgroundReplacement;
                    Parallel.For(0, R, r =>
                    {
                        oToken?.ThrowIfCancellationRequested();  
                        for (int c = 0; c < C; c++)
                        {
                            oRet[r, c] = SourceData[r, c];
                            if (oRet[r, c].GetColorInfo() == ColorIntType.IsColor)
                            {
                                if (oList.Any(X => X == oRet[r, c]))
                                {
                                    oRet[r, c] = oBkgCol;
                                }
                            }
                        }
                    });
                    oRes.DataProcessed = oRet;
                    oRes.Valid = true;
                    oRes.Message = "OK";
                    return oRes;
                }
                catch (Exception ex)
                {
                    LogMan.Exception(sC, sM, $"{Type}", ex);
                    oRes.Exception = ex;
                    oRes.Message = ex.Message;  
                    return oRes;
                }
            });
        }
    }
}
