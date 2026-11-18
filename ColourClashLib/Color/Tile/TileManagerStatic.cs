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
    public static void AssertValid(TileManager tileMan)
    {
        if (tileMan == null)
            throw new ArgumentNullException($"{nameof(tileMan) is null}");
        ImageData.AssertValid(tileMan.ImageSource);
        if (tileMan.tileProcessingMatrix == null)
            throw new ArgumentNullException($"{nameof(tileMan.tileProcessingMatrix) is null}");
        if (tileMan.tileProcessingMatrix.Length == 0)
            throw new ArgumentOutOfRangeException($"{nameof(tileMan.tileProcessingMatrix)} size is zero");
    }

    static bool MergeToMatrix(int[,] dataOut, List<TileManager?> tileMan)
    {
        string sM = nameof(MergeToMatrix);
        if (dataOut == null)
            throw new ArgumentNullException(nameof(dataOut));

        var tileManFiltered = tileMan?.Where(x => x != null).ToList() ?? null;
        if (tileManFiltered == null || tileManFiltered.Count == 0)
        {
            LogMan.Error(sC, sM, "No valid TileManagers provided");
            return false;
        }

        var minRows = tileManFiltered.Min(x => x.TileRows);
        var minColumns = tileManFiltered.Min(x => x.TileColumns);
        var ret = true;
        for (int r = 0; r < minRows; r++)
        {
            for (int c = 0; c < minColumns; c++)
            {
                var tileProcessingRC = new List<TileProcessing>();
                tileManFiltered.ForEach(x => tileProcessingRC.Add(x.GetTileProcessing(r, c)));
                var tileProcessingFilteredRC = tileProcessingRC.Where(x => x != null).ToList();
                ret &= TileProcessing.MergeData(dataOut, tileProcessingFilteredRC);
            }
        }
        return ret;
    }
    bool MergeToMatrix(int[,] dataRef, TileManager tileA, TileManager tileB)
        => MergeToMatrix(dataRef, new List<TileManager?> { tileA, tileB });
    bool MergeToMatrix(int[,] dataRef, TileManager tileA, TileManager tileB, TileManager tileC, TileManager tileD)
        => MergeToMatrix(dataRef, new List<TileManager?> { tileA, tileB, tileC, tileD });

    public static ImageData MergeToImage(List<TileManager?> tileMan)
    {
        string sM = nameof(MergeToImage);
        var tileManFiltered = tileMan?.Where(x => x != null).ToList() ?? null;
        if (tileManFiltered == null || tileManFiltered.Count == 0)
        {
            LogMan.Error(sC, sM, "No valid TileManagers provided");
            return null;
        }
        int rows = (tileManFiltered.Max(x => x.ImageSource.Rows));
        int columns = (tileManFiltered.Max(x => x.ImageSource.Columns));
        if( rows== 0 || columns == 0)
        {
            LogMan.Error(sC, sM, "Invalid dimensions calculated from TileManagers");
            return null;
        }
        var dataMerge = new int[rows, columns];
        if (!MergeToMatrix(dataMerge, tileManFiltered))
        {
            LogMan.Error(sC, sM, "Merging data from TileManagers failed");
            return null;
        }
        return new ImageData().Create(dataMerge);
    }


    public ImageData? MergeToImage( TileManager? tileA, TileManager? tileB)
       => MergeToImage(new List<TileManager?> { tileA, tileB });
    public ImageData? MergeToImage( TileManager? tileA, TileManager? tileB, TileManager? tileC, TileManager? tileD)
        => MergeToImage(new List<TileManager?> { tileA, tileB, tileC, tileD });

}
