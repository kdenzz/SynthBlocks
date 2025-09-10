using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

namespace Networking
{
    public class MatchmakingManager : NetworkBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject waitingPanel;
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private TMP_Text waitingText;
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private TMP_Text playerCountText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Image countdownFill;

        [Header("Settings")]
        [SerializeField] private float countdownDuration = 3f;
        [SerializeField] private string gameplaySceneName = "GameplayMultiplayer";

        private bool isHost = false;
        private bool gameStarted = false;
        private float countdownTimer = 0f;
        private bool hasClientConnected = false;

        public override void OnNetworkSpawn()
        {
            Debug.Log($"MatchmakingManager OnNetworkSpawn - IsServer: {IsServer}, IsHost: {IsHost}");
            
            if (IsServer)
            {
                // Host setup
                isHost = true;
                ShowWaitingForPlayers();
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                Debug.Log("Host: Waiting for client connection...");
            }
            else
            {
                // Client setup
                isHost = false;
                ShowConnectedToHost();
                Debug.Log("Client: Connected to host, waiting for countdown...");
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelMatchmaking);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        void Update()
        {
            if (gameStarted && countdownTimer > 0)
            {
                countdownTimer -= Time.deltaTime;
                UpdateCountdownUI();

                if (countdownTimer <= 0)
                {
                    StartGameplay();
                }
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;

            Debug.Log($"Client {clientId} connected. Starting countdown...");
            hasClientConnected = true;
            StartCountdownClientRpc();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            Debug.Log($"Client {clientId} disconnected. Stopping countdown...");
            hasClientConnected = false;
            StopCountdownClientRpc();
        }

        [ClientRpc]
        private void StartCountdownClientRpc()
        {
            if (gameStarted) return;

            Debug.Log("Starting countdown on all clients!");
            gameStarted = true;
            countdownTimer = countdownDuration;
            ShowCountdown();
        }

        [ClientRpc]
        private void StopCountdownClientRpc()
        {
            if (!gameStarted) return;

            gameStarted = false;
            countdownTimer = 0f;
            
            if (isHost)
            {
                ShowWaitingForPlayers();
            }
            else
            {
                ShowConnectedToHost();
            }
        }

        private void ShowWaitingForPlayers()
        {
            if (waitingPanel != null) waitingPanel.SetActive(true);
            if (countdownPanel != null) countdownPanel.SetActive(false);
            
            if (waitingText != null)
            {
                waitingText.text = "Waiting for players...";
            }
            
            if (playerCountText != null)
            {
                playerCountText.text = "Players: 1/2";
            }
        }

        private void ShowConnectedToHost()
        {
            if (waitingPanel != null) waitingPanel.SetActive(true);
            if (countdownPanel != null) countdownPanel.SetActive(false);
            
            if (waitingText != null)
            {
                waitingText.text = "Connected to host!";
            }
            
            if (playerCountText != null)
            {
                playerCountText.text = "Players: 2/2";
            }
        }

        private void ShowCountdown()
        {
            if (waitingPanel != null) waitingPanel.SetActive(false);
            if (countdownPanel != null) countdownPanel.SetActive(true);
        }

        private void UpdateCountdownUI()
        {
            if (countdownText != null)
            {
                int seconds = Mathf.CeilToInt(countdownTimer);
                countdownText.text = seconds.ToString();
                
                // Add pulsing animation
                float scale = 1f + 0.2f * Mathf.Sin(Time.time * 10f);
                countdownText.transform.localScale = Vector3.one * scale;
            }

            if (countdownFill != null)
            {
                float fillAmount = 1f - (countdownTimer / countdownDuration);
                countdownFill.fillAmount = fillAmount;
            }
        }

        private void StartGameplay()
        {
            Debug.Log("Starting gameplay!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameplaySceneName);
        }

        public void CancelMatchmaking()
        {
            if (isHost)
            {
                NetworkManager.Singleton.Shutdown();
            }
            else
            {
                NetworkManager.Singleton.Shutdown();
            }
            
            // Return to main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        // Called by MultiplayerLobby and RelayConnector
        public static void StartMatchmaking(bool isHost)
        {
            var matchmakingManager = FindFirstObjectByType<MatchmakingManager>();
            if (matchmakingManager != null)
            {
                if (isHost)
                {
                    matchmakingManager.ShowWaitingForPlayers();
                }
                else
                {
                    matchmakingManager.ShowConnectedToHost();
                }
            }
        }
    }
}
