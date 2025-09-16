using UnityEngine;
using Unity.Netcode;

namespace Networking
{
    public class NetworkBootstrap : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        void Start()
        {
            // Subscribe to network events to handle disconnections
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
            }
        }
        
        private void OnDisconnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected");
            
            // If the host disconnected, we might need to reset
            if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("Local client disconnected - this might indicate a network issue");
            }
        }
        
        void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;
            }
        }
    }
}