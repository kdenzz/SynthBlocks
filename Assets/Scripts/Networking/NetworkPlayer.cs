using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class NetworkPlayer : NetworkBehaviour
    {
        public NetworkVariable<int> score = new(writePerm: NetworkVariableWritePermission.Server);

        [ServerRpc(RequireOwnership = false)]
        public void InputMoveServerRpc(int dx)
        {
            // Route to this player's grid on server
            var manager = FindFirstObjectByType<MultiplayerGameManager>();
            if (manager == null) return;
            var grid = GetPlayerGrid(manager);
            if (grid == null) return;
            if (dx < 0) grid.MoveLeft();
            else if (dx > 0) grid.MoveRight();
        }

        [ServerRpc(RequireOwnership = false)]
        public void InputSoftDropServerRpc()
        {
            var manager = FindFirstObjectByType<MultiplayerGameManager>();
            if (manager == null) return;
            var grid = GetPlayerGrid(manager);
            grid?.SoftDrop();
        }

        [ServerRpc(RequireOwnership = false)]
        public void InputHardDropServerRpc()
        {
            var manager = FindFirstObjectByType<MultiplayerGameManager>();
            if (manager == null) return;
            var grid = GetPlayerGrid(manager);
            grid?.HardDrop();
        }

        [ServerRpc(RequireOwnership = false)]
        public void InputRotateServerRpc(int dir)
        {
            var manager = FindFirstObjectByType<MultiplayerGameManager>();
            if (manager == null) return;
            var grid = GetPlayerGrid(manager);
            if (grid == null) return;
            if (dir > 0) grid.RotateCW(); else grid.RotateCCW();
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log($"NetworkPlayer OnNetworkSpawn - OwnerClientId: {OwnerClientId}, IsOwner: {IsOwner}, LocalClientId: {NetworkManager.Singleton.LocalClientId}");
            
            if (IsOwner)
            {
                Debug.Log("NetworkPlayer: I am the owner of this player object!");
            }
            
            // Make sure this NetworkPlayer persists across scene changes
            DontDestroyOnLoad(gameObject);
            
            // Add a component to prevent accidental destruction
            if (!gameObject.GetComponent<NetworkObjectProtection>())
            {
                gameObject.AddComponent<NetworkObjectProtection>();
            }
        }
        
        public override void OnNetworkDespawn()
        {
            Debug.Log($"NetworkPlayer OnNetworkDespawn - OwnerClientId: {OwnerClientId}, IsOwner: {IsOwner}");
            
            // Only destroy on the host or owner
            if (NetworkManager.Singleton.IsHost || IsOwner)
            {
                Debug.Log("NetworkPlayer: Properly despawning NetworkPlayer");
            }
            else
            {
                Debug.LogWarning("NetworkPlayer: Non-host client attempting to despawn NetworkPlayer - this should be handled by host");
            }
        }
        
        void OnDestroy()
        {
            // Log the destruction attempt for debugging
            Debug.Log($"NetworkPlayer OnDestroy called - IsHost: {NetworkManager.Singleton?.IsHost}, IsOwner: {IsOwner}");
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddScoreServerRpc(int amount)
        {
            score.Value += amount;
        }

        private GridManager GetPlayerGrid(MultiplayerGameManager manager)
        {
            // Determine which grid this player controls
            // Player 1 (host) controls playerOneGrid, Player 2 (client) controls playerTwoGrid
            // Use OwnerClientId to determine which player this is
            bool isPlayerOne = OwnerClientId == 0; // Host is always client ID 0
            Debug.Log($"GetPlayerGrid: OwnerClientId={OwnerClientId}, isPlayerOne={isPlayerOne}");
            return isPlayerOne ? manager.GetPlayerOneGrid() : manager.GetPlayerTwoGrid();
        }
    }
}


