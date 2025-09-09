using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class GameNetworkManager : NetworkBehaviour
    {
        [SerializeField] private GridManager playerOneGrid;
        [SerializeField] private GridManager playerTwoGrid;

        private ulong hostClientId;
        private ulong remoteClientId;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback += ApproveConnection;
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }

        public override void OnDestroy()
        {
            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApproveConnection;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
            base.OnDestroy();
        }

        private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Pending = false;
        }

        private void Start()
        {
            if (IsServer)
            {
                hostClientId = NetworkManager.Singleton.LocalClientId;
                WireServerEvents();
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;
            remoteClientId = clientId;
        }

        private void WireServerEvents()
        {
            if (playerOneGrid != null)
            {
                playerOneGrid.OnLinesCleared += cleared => HandleLinesClearedServer(0, cleared);
                playerOneGrid.OnGameOver += () => HandleGameOverServer(0);
            }
            if (playerTwoGrid != null)
            {
                playerTwoGrid.OnLinesCleared += cleared => HandleLinesClearedServer(1, cleared);
                playerTwoGrid.OnGameOver += () => HandleGameOverServer(1);
            }
        }

        private void HandleLinesClearedServer(int playerIndex, int cleared)
        {
            int garbage = cleared switch { 1 => 0, 2 => 1, 3 => 2, 4 => 4, _ => 0 };
            if (garbage <= 0) return;
            var target = playerIndex == 0 ? playerTwoGrid : playerOneGrid;
            target?.EnqueueGarbage(garbage);
            NotifyGarbageClientRpc(playerIndex, garbage);
        }

        private void HandleGameOverServer(int playerIndex)
        {
            EndMatchClientRpc(playerIndex);
        }

        [ClientRpc]
        private void NotifyGarbageClientRpc(int fromPlayerIndex, int rows)
        {
            // Client-side UI feedback could be added here (incoming garbage meter)
        }

        [ClientRpc]
        private void EndMatchClientRpc(int losingPlayerIndex)
        {
            // Show a simple result on clients; server already set game over internally
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlaceBlockServerRpc(Vector3 position, int blockType)
        {
            // Validate and broadcast to clients (stub)
            SyncBlockPlacementClientRpc(position, blockType);
        }

        [ClientRpc]
        private void SyncBlockPlacementClientRpc(Vector3 position, int blockType)
        {
            // Client-side reaction to block placement (stub)
        }

        public GridManager GetPlayerGrid(int index)
        {
            return index == 0 ? playerOneGrid : playerTwoGrid;
        }
    }
}


