using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using TMPro;
using RelayAllocation = Unity.Services.Relay.Models.Allocation;
using RelayJoinAllocation = Unity.Services.Relay.Models.JoinAllocation;

  namespace Networking
  {
      public class RelayConnector : MonoBehaviour
      {
          [SerializeField] private TMP_Text joinCodeText;
          [SerializeField] private TMP_InputField joinCodeInput;
          [SerializeField] private GameObject hostPanel;
          [SerializeField] private GameObject clientPanel;
          [SerializeField] private string gameplaySceneName = "GameplayMultiplayer";

          private bool isInitialized = false;

          async void Start()
          {
              try
              {
                  await UnityServices.InitializeAsync();
                  isInitialized = true;
                  Debug.Log("Unity Services initialized successfully");
              }
              catch (System.Exception e)
              {
                  Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
              }
          }

          public async void HostWithRelay()
          {
              if (!isInitialized)
              {
                  Debug.LogError("Unity Services not initialized");
                  return;
              }

              try
              {
                  // Create allocation for 1 additional connection (host + 1 client)
                  RelayAllocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
                  string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                  // Configure transport
                  var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                  transport.SetRelayServerData(
                      allocation.RelayServer.IpV4,
                      (ushort)allocation.RelayServer.Port,
                      allocation.AllocationIdBytes,
                      allocation.Key,
                      allocation.ConnectionData,
                      null
                  );

                  // Show join code
                  if (joinCodeText != null)
                  {
                      joinCodeText.text = $"Join Code: {joinCode}";
                  }
                  if (hostPanel != null)
                  {
                      hostPanel.SetActive(true);
                  }

                  // Start host
                  bool success = NetworkManager.Singleton.StartHost();
                  if (success)
                  {
                      Debug.Log($"Host started with join code: {joinCode}");
                      // Load gameplay scene after a short delay
                      Invoke(nameof(LoadGameplayScene), 1f);
                  }
              }
              catch (System.Exception e)
              {
                  Debug.LogError($"Failed to create relay allocation: {e.Message}");
              }
          }

          public async void JoinWithRelay()
          {
              if (!isInitialized)
              {
                  Debug.LogError("Unity Services not initialized");
                  return;
              }

              string joinCode = joinCodeInput?.text?.Trim();
              if (string.IsNullOrEmpty(joinCode))
              {
                  Debug.LogError("Please enter a join code");
                  return;
              }

              try
              {
                  // Join allocation
                  RelayJoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                  // Configure transport
                  var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                  transport.SetRelayServerData(
                      joinAllocation.RelayServer.IpV4,
                      (ushort)joinAllocation.RelayServer.Port,
                      joinAllocation.AllocationIdBytes,
                      joinAllocation.Key,
                      joinAllocation.ConnectionData,
                      joinAllocation.HostConnectionData
                  );

                  // Start client
                  bool success = NetworkManager.Singleton.StartClient();
                  if (success)
                  {
                      Debug.Log($"Client started, joining with code: {joinCode}");
                      // Load gameplay scene after a short delay
                      Invoke(nameof(LoadGameplayScene), 1f);
                  }
              }
              catch (System.Exception e)
              {
                  Debug.LogError($"Failed to join relay allocation: {e.Message}");
              }
          }

          private void LoadGameplayScene()
          {
              UnityEngine.SceneManagement.SceneManager.LoadScene(gameplaySceneName);
          }

          public void ShowHostPanel()
          {
              if (hostPanel != null) hostPanel.SetActive(true);
              if (clientPanel != null) clientPanel.SetActive(false);
          }

          public void ShowClientPanel()
          {
              if (hostPanel != null) hostPanel.SetActive(false);
              if (clientPanel != null) clientPanel.SetActive(true);
          }
      }
  }