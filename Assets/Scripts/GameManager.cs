using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    #region Public Static Events

    public static event Action<bool> OnGameWon;
    public static event Action<bool> OnGameOver;
    public static event Action<bool> OnMineMarked;
    public static event Action OnGameRestarted;

    #endregion

    #region Serialized Variables

    [SerializeField] private string jsonFileNameWithoutExtension;
    [SerializeField] private Board board;
    [SerializeField] private BoardProperties boardProperties;

    #endregion

    #region Public Static Variables

    public static int TotalMines;
    public static int MarkedMines;

    #endregion

    #region Private Static Variables

    private static bool _autoplay;

    #endregion

    #region Private Variables

    private int _width;
    private int _height;
    private int _minesCount;
    private Cell[,] _grid;
    private Transform _cameraTransform;
    private bool _gameOver;
    private IEnumerator _autoplayEnumerator;

    #endregion

    #region Unity Event Functions

    private void OnValidate()
    {
        _minesCount = Mathf.Clamp(_minesCount, 0, _width * _height);
    }

    private void Awake()
    {
        if (Camera.main is null) return;
        _cameraTransform = Camera.main.transform;

        SetupBoardConfiguration();
    }

    private void SetupBoardConfiguration()
    {
        boardProperties = JsonHelper.ImportJson<BoardProperties>($"{jsonFileNameWithoutExtension}") ??
                          new BoardProperties
                          {
                              width = Random.Range(1, 16),
                              height = Random.Range(1, 16),
                              minesCount = Random.Range(1, 32)
                          };

        _width = boardProperties.width;
        _height = boardProperties.height;
        _minesCount = boardProperties.minesCount;
        _minesCount = Mathf.Clamp(_minesCount, 0, _width * _height);
        TotalMines = _minesCount;
        MarkedMines = 0;
    }

    private void OnEnable()
    {
        OnGameRestarted += NewGame;
    }

    private void OnDisable()
    {
        OnGameRestarted -= NewGame;
    }

    private void Start()
    {
        NewGame();

    }

    private void Update()
    {
        if (_gameOver) return;

        if (!_autoplay)
        {
            CheckInput();
            return;
        }
        
        if (_autoplayEnumerator == null) return;

        if (!_autoplayEnumerator.MoveNext())
        {
            // Enumerating
        }
    }

    #endregion

    #region Public Static Functions

    public static void RestartGame() => OnGameRestarted?.Invoke();

    public static void ToggleAutoplay(bool toggleState) => _autoplay = toggleState;
    
    #endregion

    #region Private Functions

    private void NewGame()
    {
        SetupBoardConfiguration();
        _grid = new Cell[_width, _height];
        _gameOver = false;
        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        if (_cameraTransform != null)
            _cameraTransform.position = new Vector3(_width / 2f, _height / 2f, -10f);

        if (board == null) return;
        board.Draw(_grid);

        _autoplayEnumerator = AutoPlay();
    }

    private void GenerateCells()
    {
        for (var i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                var cell = new Cell
                {
                    Position = new Vector3Int(i, j, 0),
                    Type = CellType.Empty
                };
                _grid[i, j] = cell;
            }
        }
    }

    private void GenerateMines()
    {
        for (int i = 0; i < _minesCount; i++)
        {
            var mineX = Random.Range(0, _width);
            var mineY = Random.Range(0, _height);

            while (_grid[mineX, mineY].Type is CellType.Mine)
            {
                mineX++;
                if (mineX < _width) continue;

                mineX = 0;

                mineY++;
                if (mineY < _height) continue;

                mineY = 0;
            }

            _grid[mineX, mineY].Type = CellType.Mine;
        }
    }

    private void GenerateNumbers()
    {
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                var cell = _grid[i, j];

                if (cell.Type is CellType.Mine) continue;

                cell.Number = CountMines(i, j);

                if (cell.Number > 0)
                    cell.Type = CellType.Number;

                _grid[i, j] = cell;
            }
        }
    }

    private int CountMines(int cellX, int cellY)
    {
        var count = 0;

        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX is 0 && adjacentY is 0) continue;

                var x = cellX + adjacentX;
                var y = cellY + adjacentY;

                if (GetCell(x, y).Type != CellType.Mine) continue;

                count++;
            }
        }

        return count;
    }

    private void CheckInput()
    {
        if (Input.GetMouseButtonDown(0))
            OpenTile();

        else if (Input.GetMouseButtonDown(1))
            FlagTile();
    }

    private void FlagTile()
    {
        if (!_cameraTransform.TryGetComponent<Camera>(out var cam)) return;
        var worldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        if (board == null) return;
        var cellPosition = board.tilemap.WorldToCell(worldPosition);
        var cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.Type is CellType.Invalid || cell.Revealed) return;

        cell.Flagged = !cell.Flagged;
        if (cell.Type is CellType.Mine)
        {
            if (cell.Flagged)
                MarkedMines++;
            else
                MarkedMines--;
            OnMineMarked?.Invoke(cell.Flagged);
        }
        _grid[cellPosition.x, cellPosition.y] = cell;
        board.Draw(_grid);
    }

    private void OpenTile()
    {
        if (!_cameraTransform.TryGetComponent<Camera>(out var cam)) return;
        var worldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        if (board == null) return;
        var cellPosition = board.tilemap.WorldToCell(worldPosition);
        var cell = GetCell(cellPosition.x, cellPosition.y);

        OpenTile(cell);
    }

    private void OpenTile(Cell cell)
    {
        if (cell.Type is CellType.Invalid || cell.Revealed || cell.Flagged) return;

        switch (cell.Type)
        {
            case CellType.Mine:
                Explode(cell);
                break;
            case CellType.Empty:
                Flood(cell);
                CheckWinCondition();
                break;
            default:
                cell.Revealed = true;
                _grid[cell.Position.x, cell.Position.y] = cell;
                CheckWinCondition();
                break;
        }

        board.Draw(_grid);
    }

    private void Flood(Cell cell)
    {
        if (cell.Revealed) return;
        if (cell.Type is CellType.Mine or CellType.Invalid) return;

        cell.Revealed = true;
        _grid[cell.Position.x, cell.Position.y] = cell;

        if (cell.Type != CellType.Empty) return;

        Flood(GetCell(cell.Position.x, cell.Position.y + 1));
        Flood(GetCell(cell.Position.x, cell.Position.y - 1));

        Flood(GetCell(cell.Position.x + 1, cell.Position.y));
        Flood(GetCell(cell.Position.x + 1, cell.Position.y + 1));
        Flood(GetCell(cell.Position.x + 1, cell.Position.y - 1));

        Flood(GetCell(cell.Position.x - 1, cell.Position.y));
        Flood(GetCell(cell.Position.x - 1, cell.Position.y + 1));
        Flood(GetCell(cell.Position.x - 1, cell.Position.y - 1));
    }

    private void Explode(Cell cell)
    {
        OnGameOver?.Invoke(true);
        _gameOver = true;

        if (_autoplayEnumerator != null)
            StopCoroutine(_autoplayEnumerator);

        cell.Revealed = true;
        cell.Exploded = true;
        _grid[cell.Position.x, cell.Position.y] = cell;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                cell = _grid[x, y];

                if (cell.Type is CellType.Mine)
                {
                    cell.Revealed = true;
                    _grid[x, y] = cell;
                }
            }
        }
    }

    private void CheckWinCondition()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var cell = _grid[x, y];

                if (cell.Type != CellType.Mine && !cell.Revealed) return;
            }
        }

        OnGameWon?.Invoke(true);
        _gameOver = true;

        if (_autoplayEnumerator != null)
            StopCoroutine(_autoplayEnumerator);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var cell = _grid[x, y];

                if (cell.Type != CellType.Mine) continue;

                cell.Flagged = true;
                _grid[x, y] = cell;
            }
        }
    }

    private Cell GetCell(int x, int y) => IsValid(x, y) ? _grid[x, y] : new Cell();

    private bool IsValid(int x, int y) => x >= 0 && x < _width && y >= 0 && y < _height;

    private IEnumerator AutoPlay()
    {
        var cellsVisited = new Dictionary<Cell, bool>();

        foreach (var gridCell in _grid)
            cellsVisited[gridCell] = false;

        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                var x = Random.Range(0, _width - 1);
                var y = Random.Range(0, _height - 1);
                var cell = _grid[x, y];
                if (!cellsVisited.TryGetValue(cell, out _)) continue;
                OpenTile(cell);
                cellsVisited[cell] = true;
                yield return new WaitForSeconds(1f);
            }
        }

        if (_autoplayEnumerator == null) yield break;
        StopCoroutine(_autoplayEnumerator);
    }

    #endregion
}