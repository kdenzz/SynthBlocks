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
        
        if (isMultiplayer)
        {
            // Find our NetworkPlayer (the one we own)
            var players = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.IsOwner)
                {
                    networkPlayer = player;
                    break;
                }
            }
        }
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
