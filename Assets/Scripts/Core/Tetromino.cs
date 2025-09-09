using System.Collections.Generic;
using UnityEngine;

public enum TetrominoType { I, O, T, S, Z, J, L }

public static class Tetromino
{
    // Each tetromino has 4 rotation states; each state has 4 cell offsets
    // Offsets are relative to an anchor in grid space
    public static readonly Dictionary<TetrominoType, Vector2Int[][]> Shapes = new()
    {
        { TetrominoType.I, new []
            {
                new [] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0) },
                new [] { new Vector2Int(1,1), new Vector2Int(1,0), new Vector2Int(1,-1), new Vector2Int(1,-2) },
                new [] { new Vector2Int(-1,-1), new Vector2Int(0,-1), new Vector2Int(1,-1), new Vector2Int(2,-1) },
                new [] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(0,-2) },
            }
        },
        { TetrominoType.O, new []
            {
                new [] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(1,-1) },
                new [] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(1,-1) },
                new [] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(1,-1) },
                new [] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(1,-1) },
            }
        },
        { TetrominoType.T, new []
            {
                new [] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,-1) },
                new [] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(1,0) },
                new [] { new Vector2Int(-1,-1), new Vector2Int(0,-1), new Vector2Int(1,-1), new Vector2Int(0,0) },
                new [] { new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1) },
            }
        },
        { TetrominoType.S, new []
            {
                new [] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,-1), new Vector2Int(0,-1) },
                new [] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,-1) },
                new [] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,-1), new Vector2Int(0,-1) },
                new [] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,-1) },
            }
        },
        { TetrominoType.Z, new []
            {
                new [] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(1,-1) },
                new [] { new Vector2Int(1,1), new Vector2Int(1,0), new Vector2Int(0,0), new Vector2Int(0,-1) },
                new [] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(1,-1) },
                new [] { new Vector2Int(1,1), new Vector2Int(1,0), new Vector2Int(0,0), new Vector2Int(0,-1) },
            }
        },
        { TetrominoType.J, new []
            {
                new [] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,-1) },
                new [] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(-1,1) },
                new [] { new Vector2Int(1,0), new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(1,1) },
                new [] { new Vector2Int(0,-1), new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(1,-1) },
            }
        },
        { TetrominoType.L, new []
            {
                new [] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,-1) },
                new [] { new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(-1,-1) },
                new [] { new Vector2Int(1,0), new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(-1,1) },
                new [] { new Vector2Int(0,-1), new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(1,1) },
            }
        }
    };

    public static IEnumerable<Vector2Int> GetCells(TetrominoType type, int rotationIndex, Vector2Int anchor)
    {
        var states = Shapes[type];
        var state = states[(rotationIndex % 4 + 4) % 4];
        for (int i = 0; i < state.Length; i++)
        {
            yield return anchor + state[i];
        }
    }
}


