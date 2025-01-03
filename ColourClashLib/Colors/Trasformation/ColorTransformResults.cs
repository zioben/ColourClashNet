using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformResults
    {
        public int[,]? DataSource { get; set; }
        public int[,]? DataTemp { get; set; }
        public int[,]? DataOut { get; set; }
    
        public bool Valid { get; set; }
        public string Message { get; private set; } = string.Empty;

        public Exception? Exception { get; set; }

        public void AddMessage( string sMessage)
        {
            if (string.IsNullOrEmpty(sMessage))
            {
                Message = sMessage;
            }
            else
            {
                Message += $"{Environment.NewLine}{sMessage}";
            }
        }
    }
}
