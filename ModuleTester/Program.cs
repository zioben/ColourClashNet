using ColourClashNet.Color.Transformation;
using ColourClashNet.Color;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using NLog;

namespace ModuleTester
{
    internal static class Program
    {


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            LogMan.Create();
            LogMan.MinLogLevel = ColourClashNet.Log.LogLevel.Message;

            //for( int i = 0; i < 256*256*256; i++)
            //{

            //    var lab = LAB.CreateFromIntRGB(i);
            //    var rgblab = RGB.CreateFromIntRGB(lab.ToIntRGB());
            //    var rgbint = RGB.CreateFromIntRGB(i);
            //    var dist = RGB.Distance(rgbint, rgblab);//, ColorDistanceEvaluationMode.RGB);
            //    if (dist >= 0)
            //    {
            //        LogMan.Message("Err", "Col", $"Color - {rgbint} <-> {lab} - distance from original = {dist}");
            //    }
            //}

            //for (int i = 128+128*256+128*256*256; i < 256 * 256 * 256; i++)
            //{

            //    var con = HSV.CreateFromIntRGB(i);
            //    int j = con.ToIntRGB();
            //    var rgbcon = RGB.CreateFromIntRGB(j);
            //    var rgbint = RGB.CreateFromIntRGB(i);
            //    var dist = RGB.Distance(rgbint, rgbcon);//, ColorDistanceEvaluationMode.RGB);
            //    if (dist >= 0 )
            //    {
            //        LogMan.Message("Err", "Col", $"{con} <-> {rgbint} - distance from original = {dist}");
            //    }
            //}

            Application.Run(new FormTester());


        }
    }
}