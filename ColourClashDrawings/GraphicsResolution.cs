using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Drawing;
public class GraphicsResolution
{
    public string Name => $"{Width}x{Height}";
    public int Width { get; set; }
    public int Height { get; set; }
}