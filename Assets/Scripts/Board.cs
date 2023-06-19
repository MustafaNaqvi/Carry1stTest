using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    #region Public Properties

    public Tilemap tilemap { get; private set; }

    #endregion

    #region Serialized Variables

    [SerializeField] private Tile tileUnknown, tileEmpty, tileMine,
                                tileExploded, tileFlag, tile1,
                                tile2, tile3, tile4,
                                tile5, tile6, tile7,
                                tile8;

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    #endregion

    #region Public Functions

    public void Draw(Cell[,] state)
    {
        int width = state.GetLength(0);
        int height = state.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                tilemap.SetTile(cell.Position, GetTile(cell));
            }
        }
    }

    #endregion

    #region Private Functions

    private Tile GetTile(Cell cell)
    {
        return cell.Revealed ?
                GetRevealedTile(cell) : cell.Flagged ?
                tileFlag : tileUnknown;
    }

    private Tile GetRevealedTile(Cell cell){
        switch (cell.Type)
        {
            case CellType.Empty: return tileEmpty;
            case CellType.Mine: return cell.Exploded ? tileExploded : tileMine;
            case CellType.Number: return GetNumberTile(cell);
            default: return null;
        }
    }

    private Tile GetNumberTile(Cell cell) =>
        cell.Number switch
        {
            1 => tile1,
            2 => tile2,
            3 => tile3,
            4 => tile4,
            5 => tile5,
            6 => tile6,
            7 => tile7,
            8 => tile8,
            _ => null
        };

    #endregion
}