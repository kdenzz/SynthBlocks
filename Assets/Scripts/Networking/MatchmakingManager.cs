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
        [SerializeField] private TMP_Text joinCodeText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Image countdownFill;

        [Header("Settings")]
        [SerializeField] private float countdownDuration = 3f;
        [SerializeField] private string gameplaySceneName = "GameplayMultiplayer";

        private bool isHost = false;
        private bool gameStarted = false;
        private float countdownTimer = 0f;
        private bool hasClientConnected = false;

        void Start()
        {
            // Display join code immediately when scene loads
            DisplayJoinCode();
            
            // Start checking for connections immediately
            InvokeRepeating(nameof(CheckConnections), 0.5f, 0.5f);
        }

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
        
        private void DisplayJoinCode()
        {
            // Check for stored join code
            string storedJoinCode = PlayerPrefs.GetString("RelayJoinCode", "");
            if (!string.IsNullOrEmpty(storedJoinCode))
            {
                Debug.Log($"Found stored join code: {storedJoinCode}");
                if (joinCodeText != null)
                {
                    joinCodeText.text = $"Join Code: {storedJoinCode}";
                }
                else
                {
                    Debug.LogError("Join Code Text UI element is not assigned!");
                }
                // Clear the stored join code after displaying it
                PlayerPrefs.DeleteKey("RelayJoinCode");
            }
            else
            {
                Debug.Log("No stored join code found");
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
            
            Debug.Log($"MatchmakingManager: Connected clients: {connectedClients}, IsHost: {isHost}, IsClient: {isClient}");
            
            // Update UI based on connection status
            if (isHost)
            {
                if (waitingText != null)
                {
                    waitingText.text = $"Waiting for players...\nShare the join code below!\nPlayers: {connectedClients}/2";
                }
                
                // Auto-start countdown if we have 2 players and haven't started yet
                if (connectedClients >= 2 && !gameStarted)
                {
                    Debug.Log("MatchmakingManager: Auto-starting countdown with 2 players!");
                    // Use direct method call instead of ClientRpc to avoid networking issues
                    StartCountdownDirect();
                }
            }
            else if (isClient)
            {
                if (waitingText != null)
                {
                    waitingText.text = "Connected to host!\nWaiting for game to start...";
                }
                
                // Client should also check if countdown should start
                // This handles cases where the host's countdown doesn't sync properly
                if (connectedClients >= 2 && !gameStarted)
                {
                    Debug.Log("MatchmakingManager: Client detected 2 players, starting countdown!");
                    StartCountdownDirect();
                }
            }
            else
            {
                if (waitingText != null)
                {
                    waitingText.text = "Not connected to network";
                }
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
            
            // Use direct method call to avoid networking issues
            StartCountdownDirect();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            Debug.Log($"Client {clientId} disconnected. Stopping countdown...");
            hasClientConnected = false;
            
            // Stop countdown directly
            gameStarted = false;
            countdownTimer = 0f;
            
            // Show waiting message again
            ShowWaitingForPlayers();
        }

        private void StartCountdownDirect()
        {
            if (gameStarted) return;

            Debug.Log("Starting countdown directly!");
            gameStarted = true;
            countdownTimer = countdownDuration;
            ShowCountdown();
            
            // Stop the connection checking since we're starting the game
            CancelInvoke(nameof(CheckConnections));
        }

        [ClientRpc]
        private void StartCountdownClientRpc()
        {
            if (gameStarted) return;

            Debug.Log("Starting countdown on all clients!");
            gameStarted = true;
            countdownTimer = countdownDuration;
            ShowCountdown();
            
            // Stop the connection checking since we're starting the game
            CancelInvoke(nameof(CheckConnections));
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
                waitingText.text = "Waiting for players...\nShare the join code below!";
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
        
        public void ForceStart()
        {
            Debug.Log("Force Start button pressed!");
            if (!gameStarted)
            {
                StartCountdownDirect();
            }
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
