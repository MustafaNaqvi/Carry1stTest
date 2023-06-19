using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile tileUnknown, tileMine, tileExplored, tileFlag;
    public List<Tile> tileNumbers = new List<Tile>();

    public void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }
    public void Draw(Cell[,] state)
    {
        int width = state.GetLength(0);
        int height = state.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                tilemap.SetTile(cell.position, GetTile(cell));
            }
        }

    }
    Tile GetTile(Cell cell)
    {
        if (cell.revealed)
        {
            return GetRevealedTile(cell);
        }
        else if (cell.flagged)
        {
            return tileFlag;
        }
        else
        {
            return tileUnknown;
        }
    }
    Tile GetRevealedTile(Cell cell)
    {
        switch (cell.type)
        {
            case Cell.Type.Empty: return tileNumbers[0];
            case Cell.Type.Mine: if (cell.exploded) return tileExplored; else return tileMine;
            case Cell.Type.Number: return GetNumberTile(cell);
            default: return null;
        }
    }
    Tile GetNumberTile(Cell cell)
    {
        return tileNumbers[cell.number];
    }
}
