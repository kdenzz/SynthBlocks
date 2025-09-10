using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string localSceneName = "GameplayLocal";
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject relayPanel;
        public void PlaySolo()
        {
            if (!string.IsNullOrEmpty(localSceneName))
            {
                SceneManager.LoadScene(localSceneName);
            }
        }

        public void PlayMultiplayer()
        {
            if (lobbyPanel != null)
            {
                mainMenuPanel.SetActive(false);
                lobbyPanel.SetActive(true);
            }
        }

        public void PlayRelay()
        {
            mainMenuPanel.SetActive(false);
            relayPanel.SetActive(true);
        }
    }
}


