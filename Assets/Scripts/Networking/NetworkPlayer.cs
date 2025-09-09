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
            var manager = FindFirstObjectByType<GameNetworkManager>();
            if (manager == null) return;
            var grid = OwnerClientId == manager.NetworkManager.LocalClientId ? manager.GetPlayerGrid(0) : manager.GetPlayerGrid(1);
            if (grid == null) return;
            if (dx < 0) grid.MoveLeft();
            else if (dx > 0) grid.MoveRight();
        }

        [ServerRpc]
        public void InputSoftDropServerRpc()
        {
            var manager = FindFirstObjectByType<GameNetworkManager>();
            if (manager == null) return;
            var grid = OwnerClientId == manager.NetworkManager.LocalClientId ? manager.GetPlayerGrid(0) : manager.GetPlayerGrid(1);
            grid?.SoftDrop();
        }

        [ServerRpc]
        public void InputHardDropServerRpc()
        {
            var manager = FindFirstObjectByType<GameNetworkManager>();
            if (manager == null) return;
            var grid = OwnerClientId == manager.NetworkManager.LocalClientId ? manager.GetPlayerGrid(0) : manager.GetPlayerGrid(1);
            grid?.HardDrop();
        }

        [ServerRpc]
        public void InputRotateServerRpc(int dir)
        {
            var manager = FindFirstObjectByType<GameNetworkManager>();
            if (manager == null) return;
            var grid = OwnerClientId == manager.NetworkManager.LocalClientId ? manager.GetPlayerGrid(0) : manager.GetPlayerGrid(1);
            if (grid == null) return;
            if (dir > 0) grid.RotateCW(); else grid.RotateCCW();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                // Owner-specific initialization (stub)
            }
        }

        [ServerRpc]
        public void AddScoreServerRpc(int amount)
        {
            score.Value += amount;
        }
    }
}


