using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformIdentity : ColorTransformBase
    {
        static string sC = nameof(ColorTransformIdentity);
        public ColorTransformIdentity()
        {
            Type = ColorTransformType.ColorIdentity;
            Description = "1:1 Color transformation";
        }

        

        protected override async Task<ColorTransformResults> ExecuteTransformAsync(CancellationToken? oToken)
        {
            string sM = nameof(ExecuteTransformAsync);
            try
            {
                var res = ColorTransformResults.CreateValidResult(SourceData, SourceData.Clone() as int[,]);
                return await Task.FromResult(res);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{Type}", ex);
                return new ColorTransformResults()
                {
                    Exception = ex,
                    Message = ex.Message
                };
            }
        }
    }
}
