using UnityEngine;
using Unity.Netcode;

namespace Networking
{
    public class NetworkPlayerSpawner : NetworkBehaviour
    {
        [Header("Player Setup")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform[] spawnPoints;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                Debug.Log("NetworkPlayerSpawner: Server spawned, creating players...");
                SpawnPlayers();
            }
        }

        private void SpawnPlayers()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("Player Prefab not assigned!");
                return;
            }

            // Spawn player for host
            var hostPlayer = Instantiate(playerPrefab);
            var hostNetworkObject = hostPlayer.GetComponent<NetworkObject>();
            if (hostNetworkObject != null)
            {
                hostNetworkObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
                Debug.Log("Host player spawned");
            }

            // Wait for client to connect, then spawn their player
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;

            Debug.Log($"Client {clientId} connected, spawning their player...");
            
            var clientPlayer = Instantiate(playerPrefab);
            var clientNetworkObject = clientPlayer.GetComponent<NetworkObject>();
            if (clientNetworkObject != null)
            {
                clientNetworkObject.SpawnAsPlayerObject(clientId);
                Debug.Log($"Client {clientId} player spawned");
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
        }
    }
}
