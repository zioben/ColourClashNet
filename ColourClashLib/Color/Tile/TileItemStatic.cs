using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile;

public partial class TileItem
{
    /// <summary>
    /// Throws if the tile is null or its image is invalid.
    /// </summary>
    public static void AssertValid(TileItem tileItem)
    {
        if (tileItem == null)
            throw new ArgumentNullException($"{nameof(tileItem) is null}");
        ImageData.AssertValid(tileItem.TileImage);
    }
}
