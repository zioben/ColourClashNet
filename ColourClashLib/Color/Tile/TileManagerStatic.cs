using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile;

public partial class TileManager
{
    /// <summary>
    /// Throws if the manager is null, has no valid source image or has not been tiled yet.
    /// </summary>
    public static void AssertValid(TileManager tileManager)
    {
        if (tileManager == null)
            throw new ArgumentNullException($"{nameof(tileManager) is null}");
        ImageData.AssertValid(tileManager.ImageSource);
        if (tileManager.tileProcessingMatrix == null)
            throw new ArgumentNullException($"{nameof(tileManager.tileProcessingMatrix) is null}");
        if (tileManager.tileProcessingMatrix.Length == 0)
            throw new ArgumentOutOfRangeException($"{nameof(tileManager.tileProcessingMatrix)} size is zero");
    }

    /// <summary>
    /// Merges the processed tiles of several managers into <paramref name="dataOut"/>.
    /// <para/>
    /// For each tile position, the tile with the lowest transformation error among the managers wins
    /// (see <see cref="TileProcessing.MergeData(int[,], IEnumerable{TileProcessing})"/>).
    /// Only the tile grid shared by all managers is merged.
    /// </summary>
    /// <param name="dataOut">Destination matrix</param>
    /// <param name="tileManagers">Managers to merge; null items are ignored</param>
    /// <returns>True if every tile was merged successfully.</returns>
    static bool MergeToMatrix(int[,] dataOut, List<TileManager?> tileManagers)
    {
        string sM = nameof(MergeToMatrix);
        if (dataOut == null)
            throw new ArgumentNullException(nameof(dataOut));

        var validManagers = tileManagers?.Where(x => x != null).ToList() ?? null;
        if (validManagers == null || validManagers.Count == 0)
        {
            LogMan.Error(sC, sM, "No valid TileManagers provided");
            return false;
        }

        var minRows = validManagers.Min(x => x.TileRows);
        var minColumns = validManagers.Min(x => x.TileColumns);
        var ret = true;
        for (int r = 0; r < minRows; r++)
        {
            for (int c = 0; c < minColumns; c++)
            {
                var tileProcessingRC = new List<TileProcessing>();
                validManagers.ForEach(x => tileProcessingRC.Add(x.GetTileProcessing(r, c)));
                var tileProcessingFilteredRC = tileProcessingRC.Where(x => x != null).ToList();
                ret &= TileProcessing.MergeData(dataOut, tileProcessingFilteredRC);
            }
        }
        return ret;
    }
    /// <summary>Merges the tiles of two managers into <paramref name="dataRef"/>.</summary>
    bool MergeToMatrix(int[,] dataRef, TileManager tileA, TileManager tileB)
        => MergeToMatrix(dataRef, new List<TileManager?> { tileA, tileB });
    /// <summary>Merges the tiles of four managers into <paramref name="dataRef"/>.</summary>
    bool MergeToMatrix(int[,] dataRef, TileManager tileA, TileManager tileB, TileManager tileC, TileManager tileD)
        => MergeToMatrix(dataRef, new List<TileManager?> { tileA, tileB, tileC, tileD });

    /// <summary>
    /// Builds an image by picking, for each tile position, the best tile among the given managers.
    /// </summary>
    /// <param name="tileManagers">Managers to merge; null items are ignored</param>
    /// <returns>The merged image, or null on error.</returns>
    public static ImageData MergeToImage(List<TileManager?> tileManagers)
    {
        string sM = nameof(MergeToImage);
        var validManagers = tileManagers?.Where(x => x != null).ToList() ?? null;
        if (validManagers == null || validManagers.Count == 0)
        {
            LogMan.Error(sC, sM, "No valid TileManagers provided");
            return null;
        }
        int rows = (validManagers.Max(x => x.ImageSource.Rows));
        int columns = (validManagers.Max(x => x.ImageSource.Columns));
        if( rows== 0 || columns == 0)
        {
            LogMan.Error(sC, sM, "Invalid dimensions calculated from TileManagers");
            return null;
        }
        var dataMerge = new int[rows, columns];
        if (!MergeToMatrix(dataMerge, validManagers))
        {
            LogMan.Error(sC, sM, "Merging data from TileManagers failed");
            return null;
        }
        return new ImageData().Create(dataMerge);
    }


    /// <summary>Merges the best tiles of two managers into a new image.</summary>
    public ImageData? MergeToImage( TileManager? tileA, TileManager? tileB)
       => MergeToImage(new List<TileManager?> { tileA, tileB });

    /// <summary>Merges the best tiles of four managers into a new image.</summary>
    public ImageData? MergeToImage( TileManager? tileA, TileManager? tileB, TileManager? tileC, TileManager? tileD)
        => MergeToImage(new List<TileManager?> { tileA, tileB, tileC, tileD });

}
