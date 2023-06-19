using UnityEngine;

public struct Cell
{
    public Vector3Int Position;
    public CellType Type;
    public int Number;
    public bool Revealed;
    public bool Flagged;
    public bool Exploded;
}
