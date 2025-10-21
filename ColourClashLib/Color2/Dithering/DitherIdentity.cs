using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashNet.Color;
using ColourClashNet.Log;

namespace ColourClashNet.Color.Dithering
{
    public class DitherIdentity : DitherBase
    {
        static string sClass = nameof(DitherIdentity);

        public DitherIdentity()
        {
            Type = ColorDithering.None;
            Description = "Passthrought";
        }

        public override bool Create()
        {
            return true;
        }

        public override async Task<int[,]?> DitherAsync(int[,]? oDataOriginal, int[,]? oDataProcessed, Palette? oDataProcessedPalette, ColorDistanceEvaluationMode eDistanceMode, CancellationToken? oToken)
        {
            return await Task.Run(() =>
            {
                string sMethod = nameof(DitherAsync);
                try
                {
                    if (oDataOriginal == null)
                    {
                        LogMan.Error(sClass, sMethod, "Invalid input data");
                        return null;
                    }
                    LogMan.Trace(sClass, sMethod, $"{Type} : Dithering");
                    var oRet = oDataProcessed.Clone() as int[,];
                    LogMan.Trace(sClass, sMethod, $"{Type} : Dithering completed");
                    return oRet;

                }
                catch (Exception ex)
                {
                    LogMan.Exception(sClass, sMethod, ex);
                    return null;
                }
            });
        }
    }
}
