using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MultiplayerLobby : MonoBehaviour
    {
        public void StartHost()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartHost();
                SceneManager.LoadScene("GameplayMultiplayer");
            }
        }

        public void StartClient()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartClient();
                SceneManager.LoadScene("GameplayMultiplayer");
            }
        }
    }
}


