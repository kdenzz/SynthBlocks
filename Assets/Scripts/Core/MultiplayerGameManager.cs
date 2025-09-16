using UnityEngine;
using Unity.Netcode;

public class MultiplayerGameManager : NetworkBehaviour
{
    [Header("Game Setup")]
    [SerializeField] private GridManager playerOneGrid;
    [SerializeField] private GridManager playerTwoGrid;
    [SerializeField] private GameManager gameManager;

    private bool gameInitialized = false;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"MultiplayerGameManager OnNetworkSpawn - IsServer: {IsServer}");
        
        if (IsServer)
        {
            Debug.Log("MultiplayerGameManager: Server spawned, initializing game...");
            // Delay initialization to ensure everything is ready
            Invoke(nameof(InitializeGameServerRpc), 2f);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InitializeGameServerRpc()
    {
        if (gameInitialized) return;
        
        Debug.Log("Initializing multiplayer game on server...");
        Debug.Log($"Player One Grid: {(playerOneGrid != null ? "Found" : "NULL")}");
        Debug.Log($"Player Two Grid: {(playerTwoGrid != null ? "Found" : "NULL")}");
        
        // Set up grid references and events, but don't initialize yet
        // Each client will initialize their own grid
        if (playerOneGrid != null)
        {
            Debug.Log("Setting up Player One grid...");
            // Subscribe to game over event
            playerOneGrid.OnGameOver += () => {
                Debug.Log("Player One Game Over!");
                UI.GameHUD.I?.ShowGameOver();
            };
            Debug.Log("Player One grid setup complete");
        }
        else
        {
            Debug.LogError("Player One Grid is null! Cannot initialize.");
        }
        
        if (playerTwoGrid != null)
        {
            Debug.Log("Setting up Player Two grid...");
            // Subscribe to game over event
            playerTwoGrid.OnGameOver += () => {
                Debug.Log("Player Two Game Over!");
                UI.GameHUD.I?.ShowGameOver();
            };
            Debug.Log("Player Two grid setup complete");
        }
        else
        {
            Debug.LogError("Player Two Grid is null! Cannot initialize.");
        }
        
        gameInitialized = true;
        
        // Notify clients that game is ready
        NotifyGameReadyClientRpc();
    }

    [ClientRpc]
    private void NotifyGameReadyClientRpc()
    {
        Debug.Log("Game is ready! Both players can start playing.");
        
        // Update UI with initial values
        if (UI.GameHUD.I != null)
        {
            UI.GameHUD.I.SetScore(0);
            UI.GameHUD.I.SetLevel(1);
            UI.GameHUD.I.SetLines(0);
        }
        
        // Initialize the appropriate grid for this client
        InitializeClientGrid();
        
        // Enable input for the appropriate player
        var inputRouter = FindFirstObjectByType<InputRouter>();
        if (inputRouter != null)
        {
            // Determine which player this client is
            bool isPlayerOne = NetworkManager.Singleton.LocalClientId == NetworkManager.Singleton.ConnectedClientsIds[0];
            
            if (isPlayerOne)
            {
                Debug.Log("This client is Player One - controlling left grid");
            }
            else
            {
                Debug.Log("This client is Player Two - controlling right grid");
            }
        }
    }
    
    private void InitializeClientGrid()
    {
        // Determine which grid this client should control
        // Host (ClientId 0) controls playerOneGrid, Client (ClientId 1) controls playerTwoGrid
        bool isHost = NetworkManager.Singleton.IsHost;
        GridManager clientGrid = isHost ? playerOneGrid : playerTwoGrid;
        
        if (clientGrid != null)
        {
            Debug.Log($"Initializing {(isHost ? "Player One" : "Player Two")} grid for this client...");
            clientGrid.Initialize();
            Debug.Log($"Client grid initialized successfully");
        }
        else
        {
            Debug.LogError($"Client grid is null! Cannot initialize for {(isHost ? "host" : "client")}.");
        }
    }

    void Start()
    {
        // Ensure we have the required components
        if (playerOneGrid == null)
        {
            Debug.LogError("Player One Grid not assigned!");
        }
        
        if (playerTwoGrid == null)
        {
            Debug.LogError("Player Two Grid not assigned!");
        }
        
        if (gameManager == null)
        {
            Debug.LogError("Game Manager not assigned!");
        }
    }

    public GridManager GetPlayerOneGrid()
    {
        return playerOneGrid;
    }

    public GridManager GetPlayerTwoGrid()
    {
        return playerTwoGrid;
    }
}
