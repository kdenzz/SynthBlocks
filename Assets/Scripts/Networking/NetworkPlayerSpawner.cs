using UnityEngine;
using Unity.Netcode;

namespace Networking
{
    public class NetworkPlayerSpawner : MonoBehaviour
    {
        [Header("Player Setup")]
        [SerializeField] private GameObject playerPrefab;

        void Start()
        {
            Debug.Log("NetworkPlayerSpawner Start - Setting up player spawning...");
            
            if (NetworkManager.Singleton != null)
            {
                // Set the player prefab in NetworkManager
                NetworkManager.Singleton.NetworkConfig.PlayerPrefab = playerPrefab;
                Debug.Log($"Player prefab set in NetworkManager: {playerPrefab?.name ?? "NULL"}");
                
                // Subscribe to connection events
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                
                Debug.Log($"NetworkPlayerSpawner: Ready to spawn players. Current state - IsHost: {NetworkManager.Singleton.IsHost}, IsClient: {NetworkManager.Singleton.IsClient}, IsConnectedClient: {NetworkManager.Singleton.IsConnectedClient}");
                
                // Check if we're already connected (host connecting to itself)
                if (NetworkManager.Singleton.IsConnectedClient)
                {
                    Debug.Log($"Already connected as client {NetworkManager.Singleton.LocalClientId} at Start. Checking for existing players...");
                    Invoke(nameof(CheckAndSpawnPlayer), 0.5f);
                }
            }
            else
            {
                Debug.LogError("NetworkManager.Singleton is null!");
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected - NetworkManager will auto-spawn player");
            
            // Manual fallback: spawn NetworkPlayer if auto-spawn fails
            Invoke(nameof(CheckAndSpawnPlayer), 2f);
        }
        
        private void CheckAndSpawnPlayer()
        {
            var players = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
            Debug.Log($"CheckAndSpawnPlayer: Found {players.Length} NetworkPlayer objects");
            
            if (players.Length == 0)
            {
                Debug.LogWarning("No NetworkPlayer found, attempting manual spawn...");
                if (playerPrefab != null && NetworkManager.Singleton != null)
                {
                    // Get the NetworkObject component from the prefab
                    var networkObject = playerPrefab.GetComponent<NetworkObject>();
                    if (networkObject != null)
                    {
                        // Use NetworkManager's SpawnManager to properly spawn the player
                        var spawnedPlayer = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(networkObject);
                        if (spawnedPlayer != null)
                        {
                            Debug.Log("Successfully spawned NetworkPlayer using NetworkManager.SpawnManager");
                        }
                        else
                        {
                            Debug.LogError("Failed to spawn NetworkPlayer using NetworkManager.SpawnManager");
                        }
                    }
                    else
                    {
                        Debug.LogError("Player prefab does not have a NetworkObject component!");
                    }
                }
                else
                {
                    Debug.LogError("Cannot spawn player - playerPrefab is null or NetworkManager is null");
                }
            }
            else
            {
                Debug.Log($"Found {players.Length} existing NetworkPlayer objects, no need to spawn");
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected");
            
            // Only the host should handle NetworkPlayer cleanup
            if (NetworkManager.Singleton.IsHost)
            {
                // Find and despawn the NetworkPlayer for the disconnected client
                var players = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
                foreach (var player in players)
                {
                    if (player.OwnerClientId == clientId)
                    {
                        Debug.Log($"Host: Despawning NetworkPlayer for disconnected client {clientId}");
                        var networkObject = player.GetComponent<NetworkObject>();
                        if (networkObject != null && networkObject.IsSpawned)
                        {
                            networkObject.Despawn();
                        }
                        break;
                    }
                }
            }
        }

        void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
    }
}
