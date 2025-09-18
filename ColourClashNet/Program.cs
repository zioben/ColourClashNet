using ColourClashNet.Color;
using ColourClashNet.Log;

namespace ColourClashNet
{
    internal static class Program
    {
        static string sClass = nameof(Program);

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string sMethod = nameof(Main);

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
         
            LogMan.Create();
            LogMan.Message( sClass, sMethod , "Hello");
            LogMan.MinLogLevel = LogLevel.Message;

            ColourClashNet.Controls.CoordinateManager oMan = new Controls.CoordinateManager();

            //var oPoint1 = new PointF(1,2);
            //oMan.TransfZoom = new PointF(0.1f,100f);
            //oMan.WorldRotationAngle = 45;
            //oMan.TranslateRotationPoint(10, 10);
            //oMan.TranslateWorldOrigin(10, -10);
            //var oPoint2 = oMan.Transform(oPoint1);
            //var oPoint3 = oMan.InverseTransform(oPoint2);


            Application.Run(new Form1());
        }
    }
}