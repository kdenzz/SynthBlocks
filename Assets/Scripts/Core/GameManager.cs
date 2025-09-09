// Scripts/Core/GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    [SerializeField] private GridManager grid;
    [SerializeField] private float tick = 0.6f; // fall speed
    [SerializeField] private int score;
    [SerializeField] private float elapsed;

    private float t;

    void Awake() { I = this; Application.targetFrameRate = 120; }
    void Start()
    {
        if (grid != null) { grid.Initialize(); }
        var hud = UI.GameHUD.I;
        if (hud != null) { hud.SetScore(score); }
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        t += Time.deltaTime;
        if (t >= tick)
        {
            t = 0f;
            if (grid != null) { grid.TickFall(); }
            if (grid != null)
            {
                tick = grid.GetRecommendedTick();
                UI.GameHUD.I?.SetLevel(grid.Level);
                UI.GameHUD.I?.SetLines(grid.LinesCleared);
                if (grid.IsGameOver)
                {
                    // optionally show game over UI via HUD
                    UI.GameHUD.I?.ShowGameOver();
                }
            }
        }
    }

    public void AddScore(int v)
    {
        score += v;
        var hud = UI.GameHUD.I;
        if (hud != null) { hud.SetScore(score); }
    }
    public int Score => score;
}