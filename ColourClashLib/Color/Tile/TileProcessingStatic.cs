using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile;

public partial class TileProcessing
{

  
    /// <summary>
    /// Copy the best tile (lower color error) on destination data
    /// </summary>
    /// <param name="oDestinationData"></param>
    /// <param name="oTileA"></param>
    /// <param name="oTileB"></param>
    /// <returns>true if data</returns>
    public static bool MergeData(int[,] oDestinationData, TileProcessing oTileA, TileProcessing oTileB)
    {
        if (oDestinationData == null)
            throw new ArgumentNullException(nameof(oDestinationData));
        if (oTileA == null && oTileB == null)
            throw new ArgumentNullException($"{nameof(oTileA)} or {nameof(oTileB)}");
        if (oTileA != null && oTileB == null)
        {
            return oTileA.MergeData(oDestinationData);
        }
        if (oTileA == null && oTileB != null)
        {
            return oTileB.MergeData(oDestinationData);
        }
        double dErrorA = oTileA.TransformationError;
        double dErrorB = oTileB.TransformationError;
        if (dErrorA <= dErrorB)
        {
            return oTileA.MergeData(oDestinationData);
        }
        else
        {
            return oTileB.MergeData(oDestinationData);
        }
    }

    /// <summary>
    /// Copy the best tile (lower color error) on destination data
    /// </summary>
    /// <param name="oDestinationData"></param>
    /// <param name="lTiles">List to tiles to compare</param>
    /// <returns></returns>
    public static bool MergeData(int[,]? oDestinationData, IEnumerable<TileProcessing> lTiles)
    {
        if (oDestinationData == null)
        {
            return false;
        }
        if (lTiles == null)
        {
            return false;
        }
        var oTile = lTiles.Where(X => X.TransformationError == lTiles.Min(Y => Y.TransformationError)).FirstOrDefault();
        if (oTile == null)
        {
            return false;
        }
        return oTile.MergeData(oDestinationData);
    }
}