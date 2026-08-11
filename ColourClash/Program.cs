using System.Reflection;

namespace ColourClash
{
    internal class Program
    {
        static Assembly ass;

        static void Main(string[] args)
        {
            ass = Assembly.GetExecutingAssembly();
            //-------------------------------------------
            //
            //
            Info();
            Help();
            Console.WriteLine("Hello, World!");
        }

        static void Info()
        {
            Console.WriteLine($"{ass.FullName}");
        }

        static void Help()
        {
            Info();
            Console.WriteLine($"Usage {ass.GetName().Name} --i [filename] --c [x,y,w,h] --t [name][parameters] --o [options][filename]");
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
