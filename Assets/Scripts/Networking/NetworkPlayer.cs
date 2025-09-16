using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class NetworkPlayer : NetworkBehaviour
    {
        public NetworkVariable<int> score = new(writePerm: NetworkVariableWritePermission.Server);

        [ServerRpc]
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

        [ServerRpc]
        public void InputSoftDropServerRpc()
        {
            var manager = FindFirstObjectByType<MultiplayerGameManager>();
            if (manager == null) return;
            var grid = GetPlayerGrid(manager);
            grid?.SoftDrop();
        }

        [ServerRpc]
        public void InputHardDropServerRpc()
        {
            var manager = FindFirstObjectByType<MultiplayerGameManager>();
            if (manager == null) return;
            var grid = GetPlayerGrid(manager);
            grid?.HardDrop();
        }

        [ServerRpc]
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
        }

        [ServerRpc]
        public void AddScoreServerRpc(int amount)
        {
            score.Value += amount;
        }

        private GridManager GetPlayerGrid(MultiplayerGameManager manager)
        {
            // Determine which grid this player controls
            // Player 1 (host) controls playerOneGrid, Player 2 (client) controls playerTwoGrid
            bool isPlayerOne = OwnerClientId == NetworkManager.Singleton.LocalClientId;
            return isPlayerOne ? manager.GetPlayerOneGrid() : manager.GetPlayerTwoGrid();
        }
    }
}


