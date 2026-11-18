using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Drawing;
/// <summary>
/// Screen resolution of a target video mode.
/// </summary>
public class GraphicsResolution
{
    /// <summary>
    /// Display name ("WIDTHxHEIGHT").
    /// </summary>
    public string Name => $"{Width}x{Height}";
    /// <summary>
    /// Width in pixels.
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// Height in pixels.
    /// </summary>
    public int Height { get; set; }
}