using UnityEngine;
using Unity.Netcode;
using Networking;

public class InputRouter : MonoBehaviour
{
    [SerializeField] private GridManager localGrid;
    [SerializeField] private bool isMultiplayer = false;
    
    private NetworkPlayer networkPlayer;

    void Start()
    {
        // Check if we're in multiplayer mode
        isMultiplayer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
        
        Debug.Log($"InputRouter Start - isMultiplayer: {isMultiplayer}, IsHost: {NetworkManager.Singleton?.IsHost}, IsClient: {NetworkManager.Singleton?.IsClient}");
        
        if (isMultiplayer)
        {
            // Wait longer for NetworkPlayers to spawn, then try to find them
            InvokeRepeating(nameof(FindNetworkPlayer), 1f, 0.5f);
        }
        else
        {
            Debug.Log("Single player mode - using local grid");
        }
    }

    private void FindNetworkPlayer()
    {
        if (networkPlayer != null) return; // Already found
        
        Debug.Log("Searching for NetworkPlayer...");
        Debug.Log($"NetworkManager State: IsHost={NetworkManager.Singleton?.IsHost}, IsClient={NetworkManager.Singleton?.IsClient}, IsConnectedClient={NetworkManager.Singleton?.IsConnectedClient}");
        Debug.Log($"Connected Clients: {NetworkManager.Singleton?.ConnectedClients.Count}");
        
        // Check if we're even connected
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient)
        {
            Debug.LogWarning("Not connected to network, stopping search");
            CancelInvoke(nameof(FindNetworkPlayer));
            return;
        }
        
        // Find our NetworkPlayer (the one we own)
        var players = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        Debug.Log($"Found {players.Length} NetworkPlayer objects");
        
        // Also check all NetworkObjects
        var allNetworkObjects = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
        Debug.Log($"Found {allNetworkObjects.Length} NetworkObject objects total");
        
        // Check if NetworkManager has a player prefab assigned
        if (NetworkManager.Singleton.NetworkConfig.PlayerPrefab == null)
        {
            Debug.LogError("NetworkManager.PlayerPrefab is null! This will prevent NetworkPlayer spawning.");
        }
        else
        {
            Debug.Log($"NetworkManager.PlayerPrefab: {NetworkManager.Singleton.NetworkConfig.PlayerPrefab.name}");
        }
        
        foreach (var player in players)
        {
            Debug.Log($"  - NetworkPlayer: Owner={player.IsOwner}, ClientId={player.OwnerClientId}, LocalClientId={NetworkManager.Singleton.LocalClientId}, IsSpawned={player.IsSpawned}");
            if (player.IsOwner)
            {
                networkPlayer = player;
                Debug.Log("Found owned NetworkPlayer!");
                CancelInvoke(nameof(FindNetworkPlayer));
                return;
            }
        }
        
        // Fallback: if we can't find an owned NetworkPlayer, try to use any NetworkPlayer
        // This might happen if there's an issue with ownership detection
        if (players.Length > 0)
        {
            Debug.LogWarning($"No owned NetworkPlayer found, but found {players.Length} NetworkPlayer objects. Using first one as fallback.");
            networkPlayer = players[0];
            CancelInvoke(nameof(FindNetworkPlayer));
            return;
        }
        
        Debug.LogWarning("No owned NetworkPlayer found yet, will keep searching...");
    }

    public void MoveLeft()
    {
        if (isMultiplayer && networkPlayer != null)
        {
            networkPlayer.InputMoveServerRpc(-1);
        }
        else if (localGrid != null)
        {
            localGrid.MoveLeft();
        }
    }

    public void MoveRight()
    {
        if (isMultiplayer && networkPlayer != null)
        {
            networkPlayer.InputMoveServerRpc(1);
        }
        else if (localGrid != null)
        {
            localGrid.MoveRight();
        }
    }

    public void SoftDrop()
    {
        if (isMultiplayer && networkPlayer != null)
        {
            networkPlayer.InputSoftDropServerRpc();
        }
        else if (localGrid != null)
        {
            localGrid.SoftDrop();
        }
    }

    public void HardDrop()
    {
        if (isMultiplayer && networkPlayer != null)
        {
            networkPlayer.InputHardDropServerRpc();
        }
        else if (localGrid != null)
        {
            localGrid.HardDrop();
        }
    }

    public void RotateCW()
    {
        if (isMultiplayer && networkPlayer != null)
        {
            networkPlayer.InputRotateServerRpc(1);
        }
        else if (localGrid != null)
        {
            localGrid.RotateCW();
        }
    }

    public void RotateCCW()
    {
        if (isMultiplayer && networkPlayer != null)
        {
            networkPlayer.InputRotateServerRpc(-1);
        }
        else if (localGrid != null)
        {
            localGrid.RotateCCW();
        }
    }
}
