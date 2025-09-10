using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MultiplayerLobby : MonoBehaviour
    {
        [SerializeField] private string matchmakingSceneName = "Matchmaking";

        public void StartHost()
        {
            if (NetworkManager.Singleton != null)
            {
                bool success = NetworkManager.Singleton.StartHost();
                if (success)
                {
                    SceneManager.LoadScene(matchmakingSceneName);
                }
            }
        }

        public void StartClient()
        {
            if (NetworkManager.Singleton != null)
            {
                bool success = NetworkManager.Singleton.StartClient();
                if (success)
                {
                    SceneManager.LoadScene(matchmakingSceneName);
                }
            }
        }
    }
}


