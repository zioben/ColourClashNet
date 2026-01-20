using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile;

public partial class TileItem : ColorTransformReductionCluster
{
    static readonly string sC =nameof(TileItem);
   
    /// <summary>
    /// Tile Width
    /// </summary>
    public int TileW { get; internal set; } = 8;

    /// <summary>
    /// Tile Height
    /// </summary>
    public int TileH { get; internal set; } = 8;

    /// <summary>
    /// Original position in source
    /// </summary>
    public int DataSourceOriginC { get; private set; } = 0;

    /// <summary>
    /// Original position in source
    /// </summary>
    public int DataSourceOriginR { get; private set; } = 0;

    /// <summary>
    /// Original image reference
    /// </summary>
    public ImageData ImageSource { get; internal set; } = null;

 
    [Obsolete("Not supported in Tile class", true)]
    public ColorTransformResults Create(int[,]? oDataSource, CancellationToken? oToken)
    {
        throw new NotSupportedException("Not valid on Tile Class");
    }


    public TileItem Create(ImageData oDataSource, int iSourceIndexR, int iSourceIndexC)
    {
        string sM = nameof(Create);
        try
        {
            DataSourceOriginC = iSourceIndexC;
            DataSourceOriginR = iSourceIndexR;
            if (oDataSource == null)
            {
                LogMan.Error(sC, sM, "Datasource null");
                return this;
            }
            if (TileW <= 0 || TileH <= 0 || MaxColorsWanted <= 0)
            {
                LogMan.Error(sC, sM, "Invalid Tile Data");
                return this;
            }
            var oTileData = CreateTileData(oDataSource, iSourceIndexR, iSourceIndexC, TileW, TileH);
            base.Create( oTileData);
            return this;
        }
        catch (Exception ex)
        {
            LogMan.Exception(sC, sM, ex);
            return this;
        }
    }


    /// <summary>
    /// Copy tile processed data on destination data
    /// <para>No data will be copied on error</para>
    /// </summary>
    /// <param name="oDestinationData">Data to be overweritten</param>
    public bool MergeData(int[,]? oDestinationData)
    {
        string sM = nameof(MergeData); 
        if (oDestinationData == null)
        {
            LogMan.Error(sC, sM, "Destination data null");
            return false;
        }
        if (OutputData == null)
        {
            LogMan.Error(sC, sM, "Tile output data null");
            return false;
        }
        int R = oDestinationData.GetLength(0);
        int C = oDestinationData.GetLength(1);
        int RR = Math.Max(0, Math.Min(R, DataSourceOriginR + TileH));
        int CC = Math.Max(0, Math.Min(C, DataSourceOriginC + TileW));

        // Merge Data
        Parallel.For(DataSourceOriginR, RR, dr =>//  int dr = DataSourceOriginR, r = 0; dr < RR; dr++, r++)
        {
            int r = dr - DataSourceOriginR;
            for (int dc = DataSourceOriginC, c = 0; dc < CC; dc++, c++)
            {
                oDestinationData[dr, dc] = OutputData.DataX[r, c];
            }
        });
        return true;
    }

    public override string ToString()
    {
        return $"R={DataSourceOriginR}:C={DataSourceOriginC}:H={TileH}:W={TileW} : TE={TransformationError}";
    }  
}
