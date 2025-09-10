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
        
        // Initialize both grids on the server
        if (playerOneGrid != null)
        {
            Debug.Log("Initializing Player One grid...");
            playerOneGrid.Initialize();
            Debug.Log("Player One grid initialized successfully");
        }
        else
        {
            Debug.LogError("Player One Grid is null! Cannot initialize.");
        }
        
        if (playerTwoGrid != null)
        {
            Debug.Log("Initializing Player Two grid...");
            playerTwoGrid.Initialize();
            Debug.Log("Player Two grid initialized successfully");
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
        
        // Enable input for the appropriate player
        var inputRouter = FindFirstObjectByType<InputRouter>();
        if (inputRouter != null)
        {
            // Determine which player this client is
            bool isPlayerOne = NetworkManager.Singleton.LocalClientId == NetworkManager.Singleton.ConnectedClientsIds[0];
            
            if (isPlayerOne)
            {
                Debug.Log("This client is Player One");
                // Player One controls the first grid
            }
            else
            {
                Debug.Log("This client is Player Two");
                // Player Two controls the second grid
            }
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
}
