using System.Reflection;

namespace ColourClash
{
    /// <summary>
    /// Command line front-end (work in progress): currently prints assembly info and usage help only.
    /// </summary>
    internal class Program
    {
        static Assembly assembly;

        static void Main(string[] args)
        {
            assembly = Assembly.GetExecutingAssembly();
            Info();
            Help();
            Console.WriteLine("Hello, World!");
        }

        /// <summary>
        /// Prints the assembly full name.
        /// </summary>
        static void Info()
        {
            Console.WriteLine($"{assembly.FullName}");
        }

        /// <summary>
        /// Prints the planned command line syntax.
        /// </summary>
        static void Help()
        {
            Info();
            Console.WriteLine($"Usage {assembly.GetName().Name} --i [filename] --c [x,y,w,h] --t [name][parameters] --o [options][filename]");
            Console.WriteLine($"--i image filename (supported .png .bmp .jpg)");
            Console.WriteLine($"--c crop image bounding box x left,y top, width, height (optional)");
            Console.WriteLine($"--t transformation");
            Console.WriteLine($"\t-n transformation name");
            Console.WriteLine($"\t-p transformation parameters");
            Console.WriteLine($"--o output");
            Console.WriteLine($"\t-p output parameters");
            Console.WriteLine($"\t-test");

        }
    }
}
