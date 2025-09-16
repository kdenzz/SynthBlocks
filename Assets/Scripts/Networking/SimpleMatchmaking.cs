using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Networking
{
    public class SimpleMatchmaking : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject waitingPanel;
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Image countdownFill;

        [Header("Settings")]
        [SerializeField] private float countdownDuration = 3f;
        [SerializeField] private string gameplaySceneName = "GameplayMultiplayer";

        private bool isHost = false;
        private bool gameStarted = false;
        private float countdownTimer = 0f;
        private float connectionCheckTimer = 0f;

        void Start()
        {
            // Determine if we're host or client
            isHost = NetworkManager.Singleton.IsHost;
            
            Debug.Log($"SimpleMatchmaking Start - IsHost: {isHost}, IsClient: {NetworkManager.Singleton.IsClient}");
            
            if (isHost)
            {
                statusText.text = "Waiting for players...\nPlayers: 1/2";
                Debug.Log("Host: Waiting for client...");
            }
            else
            {
                statusText.text = "Connected to host!\nPlayers: 2/2";
                Debug.Log("Client: Connected, waiting for countdown...");
                // Client should start countdown immediately since host already detected connection
                StartCountdown();
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelMatchmaking);
            }

            // Start checking for connections with longer delay for builds
            float checkDelay = Application.isEditor ? 1f : 2f;
            InvokeRepeating(nameof(CheckConnections), checkDelay, 0.5f);
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

        private void CheckConnections()
        {
            if (!isHost || gameStarted) return;

            // Check if we have a client connected (excluding host)
            int connectedClients = NetworkManager.Singleton.ConnectedClients.Count;
            Debug.Log($"[{Time.time:F1}] Checking connections: {connectedClients} clients connected");
            
            // List all connected client IDs for debugging
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                Debug.Log($"  - Client ID: {clientId}");
            }
            
            if (connectedClients >= 2)
            {
                Debug.Log("Client detected! Starting countdown...");
                StartCountdown();
                CancelInvoke(nameof(CheckConnections)); // Stop checking
            }
        }

        private void StartCountdown()
        {
            if (gameStarted) return;

            gameStarted = true;
            countdownTimer = countdownDuration;
            ShowCountdown();
            
            // Notify client to start countdown too
            if (NetworkManager.Singleton.IsHost)
            {
                var clientRpc = GetComponent<NetworkBehaviour>();
                if (clientRpc != null)
                {
                    // We'll use a simple approach - both will start countdown when host detects client
                }
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
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void CancelMatchmaking()
        {
            Debug.Log("Cancelling matchmaking...");
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MainMenu");
        }

        // Manual start button for testing
        public void ForceStartGame()
        {
            Debug.Log("Force starting game (for testing)");
            StartCountdown();
        }
    }
}
