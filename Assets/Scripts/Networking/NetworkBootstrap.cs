using UnityEngine;

namespace Networking
{
    public class NetworkBootstrap : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}