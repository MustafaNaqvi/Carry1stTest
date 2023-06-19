using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] int mineCount;
    Board board;
    Cell[,] state;
    bool gameOver;
    int flaggedMines = 0;
    void Awake()
    {
        board = GetComponentInChildren<Board>();
    }
    void Start()
    {
        CenterCamera();
        NewGame();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) NewGame();

        if (!gameOver)
        {
            if (Input.GetMouseButtonDown(1)) Flag();
            else if (Input.GetMouseButtonDown(0)) Reveal();
        }
    }

    void CenterCamera()
    {
        Camera.main.transform.position = new Vector3(width, height / 2, -10);
        Camera.main.orthographicSize = Mathf.Max(width, height) / 3 * 2;
    }
    void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width * height);
    }

    void NewGame()
    {
        state = new Cell[width, height];
        gameOver = false;
        if (mineCount == 0) mineCount = (width * height) / 2;

        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        board.Draw(state);
    }
    void GenerateCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                state[x, y] = cell;

                // state[x,y].transform.parent=transform;
            }
        }
    }
    void GenerateMines()
    {
        for (int i = 0; i < mineCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            while (state[x, y].type == Cell.Type.Mine)
            {
                x++;
                if (x >= width)
                {
                    x = 0;
                    y++;
                    if (y >= height) y = 0;
                }
            }

            state[x, y].type = Cell.Type.Mine;
            // state[x, y].revealed = true;
        }
    }
    void GenerateNumbers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                if (cell.type == Cell.Type.Mine)
                {
                    continue;
                }
                cell.number = CountMine(x, y);

                if (cell.number > 00)
                {
                    cell.type = Cell.Type.Number;
                }
                // cell.revealed = true;
                state[x, y] = cell;
            }
        }
    }
    int CountMine(int cellX, int cellY)
    {
        int count = 0;
        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0) continue;

                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                // if (x < 0 || x >= width || y < 0 || y >= height) continue;

                if (IsValid(x, y) && state[x, y].type == Cell.Type.Mine) count++;
                // if (GetCell(x, y).type == Cell.Type.Mine) count++;
            }
        }
        return count;
    }
    void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid) return;

        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);
    }
    Cell GetCell(int x, int y)
    {
        if (IsValid(x, y)) return state[x, y];
        else return new Cell();
    }
    bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
    void Reveal()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged) return;

        switch (cell.type)
        {
            case Cell.Type.Mine: Explode(cell); break;
            case Cell.Type.Empty: Flood(cell); CheckWinCondition(); break;
            default: cell.revealed = true; state[cellPosition.x, cellPosition.y] = cell; break;
        }

        if (cell.type == Cell.Type.Empty) Flood(cell);

        cell.revealed = true;
        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);

    }
    void Flood(Cell cell)
    {
        if (cell.revealed) return;

        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;

        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell;

        if (cell.type == Cell.Type.Empty)
        {
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x + 1, cell.position.y));
            Flood(GetCell(cell.position.x, cell.position.y - 1));
            Flood(GetCell(cell.position.x, cell.position.y + 1));
        }
    }
    void Explode(Cell cell)
    {
        Debug.Log("Game Over");
        gameOver = true;

        cell.revealed = true;
        cell.exploded = true;

        state[cell.position.x, cell.position.y] = cell;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cell = state[x, y];
                if (cell.type == Cell.Type.Mine)
                {
                    cell.revealed = true;
                    state[x, y] = cell;
                }
            }
        }
    }
    void CheckWinCondition()
    {
        foreach (Cell cell in state)
        {
            if (cell.type != Cell.Type.Mine && !cell.revealed) return;
        }


        Debug.Log("Win");
        gameOver = true;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    cell.flagged = true;
                    state[x, y] = cell;
                }
            }
        }
    }
}
