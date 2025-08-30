using ColourClashNet.Color;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformResults
    {
        static string sClass = nameof(ColorTransformResults);

        public int[,]? DataSource { get; set; }
        public int[,]? DataTemp { get; set; }
        public int[,]? DataOut { get; set; }
    
        public bool Valid { get; set; }
        public string Message { get; private set; } = string.Empty;

        public Exception? Exception { get; set; }

        public ColorTransformInterface ColorTransformInterface { get; }

        public void AddMessage( string sMessage)
        {
            string sMethod = nameof(AddMessage);
            if (string.IsNullOrEmpty(sMessage))
            {
                Message = sMessage;
            }
            else
            {
                Message += $"{Environment.NewLine}{sMessage}";
            }
            LogMan.Message(sClass, sMethod, sMessage);
        }
    }
}
