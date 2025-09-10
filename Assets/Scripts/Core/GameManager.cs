// Scripts/Core/GameManager.cs
using UnityEngine;
using Unity.Netcode;

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
        // Only initialize grid in single-player mode
        if (grid != null && !IsMultiplayerMode()) 
        { 
            grid.Initialize(); 
        }
        var hud = UI.GameHUD.I;
        if (hud != null) { hud.SetScore(score); }
    }

    private bool IsMultiplayerMode()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
    }

    void Update()
    {
        // Only run game loop in single-player mode
        if (IsMultiplayerMode()) return;
        
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