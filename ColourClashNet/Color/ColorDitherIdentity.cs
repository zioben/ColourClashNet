using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorDitherIdentity : ColorDitherBase
    {
        static string sClass = nameof(ColorDitherIdentity);

        public ColorDitherIdentity()
        {
            Type = ColorDithering.None;
            Description = "Passthrought";
        }

        public override  bool Create()
        {
            return true;
        }

        public override int[,]? Dither(int[,]? oDataOriginal, int[,]? oDataProcessed, List<int>? oDataProcessedPalette, ColorDistanceEvaluationMode eDistanceMode)
        {
            string sMethod = nameof(Dither);
            try
            {
                if (oDataOriginal == null)
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
