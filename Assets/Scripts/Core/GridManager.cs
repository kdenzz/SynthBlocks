using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private GameObject blockPrefab;

    private BlockController[,] grid;
    private List<BlockController> activeBlocks = new List<BlockController>();
    private TetrominoType activeType;
    private int activeRotation;
    private Vector2Int activeAnchor;

    private readonly Queue<TetrominoType> nextPieces = new Queue<TetrominoType>();

    private int totalLinesCleared;
    private int level = 1;
    public int Level => level;
    public int LinesCleared => totalLinesCleared;
    public bool IsGameOver { get; private set; }

    public System.Action<int> OnLinesCleared; // cleared count
    public System.Action OnGameOver;

    private int pendingGarbage;

    void Awake()
    {
        grid = new BlockController[gridWidth, gridHeight];
    }

    public void Initialize()
    {
        ClearAll();
        if (nextPieces.Count == 0) RefillBag();
        SpawnNextPiece();
    }

    public void TickFall()
    {
        if (IsGameOver)
        {
            return;
        }

        if (activeBlocks.Count == 0)
        {
            SpawnNextPiece();
            return;
        }

        if (TryMove(new Vector2Int(0, -1)))
        {
            // moved down
        }
        else
        {
            LockActiveBlocks();
            int cleared = ClearFullLines();
            if (cleared > 0)
            {
                totalLinesCleared += cleared;
                UpdateLevel();
                int add = cleared switch { 1 => 100, 2 => 300, 3 => 500, 4 => 800, _ => cleared * 100 };
                GameManager.I.AddScore(add);
                OnLinesCleared?.Invoke(cleared);
            }
            activeBlocks.Clear();
            if (nextPieces.Count == 0) RefillBag();
            if (pendingGarbage > 0) { ApplyGarbage(); }
            SpawnNextPiece();
        }
    }

    public void MoveLeft()  { TryMove(new Vector2Int(-1, 0)); }
    public void MoveRight() { TryMove(new Vector2Int( 1, 0)); }
    public void SoftDrop()  { TryMove(new Vector2Int( 0,-1)); }
    public void HardDrop()
    {
        while (TryMove(new Vector2Int(0, -1))) { }
        LockActiveBlocks();
        int cleared = ClearFullLines();
        if (cleared > 0)
        {
            totalLinesCleared += cleared;
            UpdateLevel();
            int add = cleared switch { 1 => 100, 2 => 300, 3 => 500, 4 => 800, _ => cleared * 100 };
            GameManager.I.AddScore(add);
            OnLinesCleared?.Invoke(cleared);
        }
        activeBlocks.Clear();
        if (nextPieces.Count == 0) RefillBag();
        if (pendingGarbage > 0) { ApplyGarbage(); }
        SpawnNextPiece();
    }

    public void RotateCW() { TryRotate(1); }
    public void RotateCCW() { TryRotate(-1); }

    private void ClearAll()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    Destroy(grid[x, y].gameObject);
                    grid[x, y] = null;
                }
            }
        }
        foreach (var b in activeBlocks) if (b != null) Destroy(b.gameObject);
        activeBlocks.Clear();
    }

    private void SpawnNextPiece()
    {
        if (nextPieces.Count == 0) RefillBag();
        activeType = nextPieces.Dequeue();
        activeRotation = 0;

        // spawn anchor near top-center; adjust for I/O width
        int centerX = gridWidth / 2;
        int spawnY = gridHeight - 1;
        activeAnchor = new Vector2Int(centerX, spawnY);

        // Nudge left for wide pieces to center better
        if (activeType == TetrominoType.I || activeType == TetrominoType.O)
        {
            activeAnchor += new Vector2Int(-1, 0);
        }

        if (!CanPlace(activeType, activeRotation, activeAnchor))
        {
            // Game Over
            IsGameOver = true;
            activeBlocks.Clear();
            OnGameOver?.Invoke();
            return;
        }

        activeBlocks.Clear();
        foreach (var cell in Tetromino.GetCells(activeType, activeRotation, activeAnchor))
        {
            if (!InBounds(cell.x, cell.y) || grid[cell.x, cell.y] != null) continue;
            var go = Instantiate(blockPrefab);
            var bc = go.GetComponent<BlockController>();
            bc.SetGridPosition(cell.x, cell.y, cellSize);
            activeBlocks.Add(bc);
        }
    }

    private bool TryMove(Vector2Int delta)
    {
        var newAnchor = activeAnchor + delta;
        if (!CanPlace(activeType, activeRotation, newAnchor)) return false;
        activeAnchor = newAnchor;
        UpdateActiveBlockTransforms();
        return true;
    }

    private void UpdateActiveBlockTransforms()
    {
        var cells = new List<Vector2Int>(Tetromino.GetCells(activeType, activeRotation, activeAnchor));
        for (int i = 0; i < activeBlocks.Count && i < cells.Count; i++)
        {
            var b = activeBlocks[i];
            var c = cells[i];
            b.SetGridPosition(c.x, c.y, cellSize);
        }
    }

    private bool CanPlace(TetrominoType type, int rotation, Vector2Int anchor)
    {
        foreach (var c in Tetromino.GetCells(type, rotation, anchor))
        {
            if (!InBounds(c.x, c.y)) return false;
            if (grid[c.x, c.y] != null) return false;
        }
        return true;
    }

    private void LockActiveBlocks()
    {
        foreach (var b in activeBlocks)
        {
            if (InBounds(b.X, b.Y))
            {
                grid[b.X, b.Y] = b;
            }
        }
    }

    private int ClearFullLines()
    {
        int cleared = 0;
        for (int y = 0; y < gridHeight; y++)
        {
            if (IsLineFull(y))
            {
                ClearLine(y);
                DropAbove(y);
                y--; // re-check this row after dropping
                cleared++;
            }
        }
        return cleared;
    }

    private bool IsLineFull(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, y] == null) return false;
        }
        return true;
    }

    private void ClearLine(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, y] != null)
            {
                Destroy(grid[x, y].gameObject);
                grid[x, y] = null;
            }
        }
    }

    private void DropAbove(int clearedY)
    {
        for (int y = clearedY + 1; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                var b = grid[x, y];
                if (b != null)
                {
                    int ny = y - 1;
                    grid[x, y] = null;
                    grid[x, ny] = b;
                    b.SetGridPosition(x, ny, cellSize);
                }
            }
        }
    }

    private bool InBounds(int x, int y) => x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;

    private bool IsOccupied(int x, int y, bool excludeFalling) { return grid[x, y] != null; }

    private void RefillBag()
    {
        var bag = new List<TetrominoType> { TetrominoType.I, TetrominoType.O, TetrominoType.T, TetrominoType.S, TetrominoType.Z, TetrominoType.J, TetrominoType.L };
        // Fisher-Yates
        for (int i = bag.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (bag[i], bag[j]) = (bag[j], bag[i]);
        }
        foreach (var t in bag) nextPieces.Enqueue(t);
    }

    private bool TryRotate(int dir)
    {
        int newRot = (activeRotation + dir + 4) % 4;
        // Test kicks
        Vector2Int[] kicks = new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1), new Vector2Int(2,0), new Vector2Int(-2,0) };
        foreach (var k in kicks)
        {
            var testAnchor = activeAnchor + k;
            if (CanPlace(activeType, newRot, testAnchor))
            {
                activeRotation = newRot;
                activeAnchor = testAnchor;
                UpdateActiveBlockTransforms();
                return true;
            }
        }
        return false;
    }

    private void UpdateLevel()
    {
        level = 1 + (totalLinesCleared / 10);
    }

    public float GetRecommendedTick()
    {
        float baseTick = 0.6f;
        float speedup = 0.05f * (level - 1);
        return Mathf.Max(0.15f, baseTick - speedup);
    }

    public void EnqueueGarbage(int rows)
    {
        pendingGarbage += Mathf.Max(0, rows);
    }

    private void ApplyGarbage()
    {
        while (pendingGarbage > 0)
        {
            // push all rows up by 1; top row may be lost (causing implicit pressure)
            for (int y = gridHeight - 1; y >= 1; y--)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    var from = grid[x, y - 1];
                    grid[x, y] = from;
                    if (from != null)
                    {
                        from.SetGridPosition(x, y, cellSize);
                    }
                }
            }
            // bottom row: fill with blocks except one random hole
            int hole = Random.Range(0, gridWidth);
            for (int x = 0; x < gridWidth; x++)
            {
                var existing = grid[x, 0];
                if (existing != null)
                {
                    Destroy(existing.gameObject);
                    grid[x, 0] = null;
                }
                if (x == hole) continue;
                var go = Object.Instantiate(blockPrefab);
                var bc = go.GetComponent<BlockController>();
                bc.SetGridPosition(x, 0, cellSize);
                grid[x, 0] = bc;
            }
            pendingGarbage--;
        }
    }
}