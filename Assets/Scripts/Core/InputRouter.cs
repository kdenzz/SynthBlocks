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
            // Wait a bit for NetworkPlayers to spawn, then try to find them
            InvokeRepeating(nameof(FindNetworkPlayer), 0.5f, 0.5f);
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
        
        // Find our NetworkPlayer (the one we own)
        var players = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        Debug.Log($"Found {players.Length} NetworkPlayer objects");
        
        foreach (var player in players)
        {
            Debug.Log($"  - NetworkPlayer: Owner={player.IsOwner}, ClientId={player.OwnerClientId}, LocalClientId={NetworkManager.Singleton.LocalClientId}");
            if (player.IsOwner)
            {
                networkPlayer = player;
                Debug.Log("Found owned NetworkPlayer!");
                CancelInvoke(nameof(FindNetworkPlayer));
                return;
            }
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
