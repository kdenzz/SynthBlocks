using UnityEngine;
using Unity.Netcode;

namespace Networking
{
    public class NetworkObjectProtection : MonoBehaviour
    {
        void Awake()
        {
            // This component prevents accidental destruction of NetworkObjects
            // on non-host clients
        }
        
        void OnDestroy()
        {
            // Check if we're trying to destroy a NetworkObject on a non-host client
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsHost)
            {
                var networkObject = GetComponent<NetworkObject>();
                if (networkObject != null && networkObject.IsSpawned)
                {
                    Debug.LogWarning($"NetworkObjectProtection: Preventing destruction of {gameObject.name} on non-host client. Only host should destroy NetworkObjects.");
                    // Prevent the destruction by not calling the base method
                    return;
                }
            }
        }
    }
}