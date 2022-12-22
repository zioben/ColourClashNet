using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashLib.Color;
using ColourClashNet.Colors;

namespace ColourClashNet.Colors.Dithering
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

        public override int[,]? Dither(int[,]? oDataOriginal, int[,]? oDataProcessed, ColorPalette? oDataProcessedPalette, ColorDistanceEvaluationMode eDistanceMode)
        {
            string sMethod = nameof(Dither);
            try
            {
                if (oDataOriginal == null || oDataProcessedPalette == null || oDataProcessedPalette.Colors == 0 )
                {
                    Trace.TraceError($"{sClass}.{sMethod} ({Type}) : Invalid input data");
                    return null;
                }
                Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Dithering");
                var oRet = oDataProcessed.Clone() as int[,];
                Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Dithering completed");
                return oRet;

            }
            catch (Exception ex)
            {
                Trace.TraceError($"{sClass}.{sMethod} ({Type}) : Exception raised : {ex.Message}");
                return null;
            }
        }
    }
}
