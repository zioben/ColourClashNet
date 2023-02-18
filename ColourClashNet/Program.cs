using ColourClashNet.Colors;

namespace ColourClashNet
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

            //var rgb = ColorIntExt.FromRGB(10,5,1);            
            //var h = rgb.ToH();
            //var s = rgb.ToS();
            //var v = rgb.ToV();
            //var rgb2 = ColorIntExt.FromRGB(0, 255,0);
            //var h2 = rgb2.ToH();
            //var s2 = rgb2.ToS();
            //var v2 = rgb2.ToV();
            //var rgb3 = ColorIntExt.FromHSV(h, s, v);

            Application.Run(new Form1());
        }
    }
}