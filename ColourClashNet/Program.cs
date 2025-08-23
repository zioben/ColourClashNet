using ColourClashNet.Colors;
using ColourClashSupport.Log;

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
         
            Log.Create();
            Log.Message( sClass, sMethod , "Hello");
            Log.MinLogLevel = LogLevel.Debug;

            Application.Run(new Form1());
        }
    }
}