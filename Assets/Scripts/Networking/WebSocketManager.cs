using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking
{
    public class WebSocketManager : MonoBehaviour
    {
        // Placeholder interface to keep compile-time independence from a specific WebSocket lib
        private IWebSocketClient client;

        [SerializeField] private string serverUrl = "ws://localhost:8080";

        private void Awake()
        {
            // Intentionally not instantiating a concrete client to avoid external dependency
        }

        public async Task ConnectAsync()
        {
            if (client == null)
            {
                Debug.LogWarning("WebSocket client not set. Provide an implementation of IWebSocketClient.");
                return;
            }

            await client.ConnectAsync(serverUrl);
            client.OnMessage += HandleMessage;
        }

        public void SendJson(string json)
        {
            if (client == null)
            {
                Debug.LogWarning("WebSocket client not set. Provide an implementation of IWebSocketClient.");
                return;
            }

            client.Send(json);
        }

        private void HandleMessage(string message)
        {
            // Handle incoming messages (stub)
        }

        public void SetClient(IWebSocketClient webSocketClient)
        {
            client = webSocketClient;
        }
    }

    public interface IWebSocketClient
    {
        event Action<string> OnMessage;
        Task ConnectAsync(string url);
        void Send(string message);
    }
}


