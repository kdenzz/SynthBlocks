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
                Debug.Log("Player prefab set in NetworkManager");
                
                // Subscribe to connection events
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                
                Debug.Log("NetworkPlayerSpawner: Ready to spawn players");
            }
            else
            {
                Debug.LogError("NetworkManager.Singleton is null!");
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected - NetworkManager will auto-spawn player");
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected");
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
