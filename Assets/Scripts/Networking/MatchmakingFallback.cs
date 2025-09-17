using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

namespace Networking
{
    public class MatchmakingFallback : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Text joinCodeText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button cancelButton;

        void Start()
        {
            // Display join code immediately
            DisplayJoinCode();
            
            // Set up cancel button
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelMatchmaking);
            }
            
            // Start checking for connections
            InvokeRepeating(nameof(CheckConnections), 1f, 0.5f);
        }

        private void DisplayJoinCode()
        {
            // Check for stored join code
            string storedJoinCode = PlayerPrefs.GetString("RelayJoinCode", "");
            if (!string.IsNullOrEmpty(storedJoinCode))
            {
                Debug.Log($"Fallback: Found stored join code: {storedJoinCode}");
                if (joinCodeText != null)
                {
                    joinCodeText.text = $"Join Code: {storedJoinCode}";
                }
                else
                {
                    Debug.LogError("Fallback: Join Code Text UI element is not assigned!");
                }
            }
            else
            {
                Debug.Log("Fallback: No stored join code found");
                if (joinCodeText != null)
                {
                    joinCodeText.text = "No join code available";
                }
            }
        }

        private void CheckConnections()
        {
            if (NetworkManager.Singleton == null) return;

            int connectedClients = NetworkManager.Singleton.ConnectedClients.Count;
            bool isHost = NetworkManager.Singleton.IsHost;
            bool isClient = NetworkManager.Singleton.IsClient;

            Debug.Log($"Fallback: Connected clients: {connectedClients}, IsHost: {isHost}, IsClient: {isClient}");

            if (statusText != null)
            {
                if (isHost)
                {
                    statusText.text = $"Waiting for players...\nShare the join code below!\nPlayers: {connectedClients}/2";
                }
                else if (isClient)
                {
                    statusText.text = "Connected to host!\nWaiting for game to start...";
                }
                else
                {
                    statusText.text = "Not connected to network";
                }
            }

            // Auto-start countdown if we have 2 players
            if (connectedClients >= 2 && !gameStarted)
            {
                Debug.Log("Fallback: Auto-starting countdown with 2 players!");
                StartCountdown();
            }
        }

        private bool gameStarted = false;
        private float countdownTimer = 3f;

        void Update()
        {
            if (gameStarted && countdownTimer > 0)
            {
                countdownTimer -= Time.deltaTime;
                
                if (statusText != null)
                {
                    int seconds = Mathf.CeilToInt(countdownTimer);
                    statusText.text = $"Starting game in {seconds}...";
                }

                if (countdownTimer <= 0)
                {
                    StartGameplay();
                }
            }
        }

        private void StartCountdown()
        {
            if (gameStarted) return;
            
            Debug.Log("Fallback: Starting countdown!");
            gameStarted = true;
            countdownTimer = 3f;
        }

        private void StartGameplay()
        {
            Debug.Log("Fallback: Starting gameplay!");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayMultiplayer");
        }

        public void CancelMatchmaking()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }
            
            // Return to main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        
        public void ForceStart()
        {
            Debug.Log("Fallback: Force Start button pressed!");
            if (!gameStarted)
            {
                StartCountdown();
            }
        }
    }
}
