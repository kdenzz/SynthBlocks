using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

namespace Cloud
{
    // Define a class to hold the event data
    [System.Serializable]
    public class BlockPlacementEventData
    {
        public string position;
        public int block_type;
        public float timestamp;

        public BlockPlacementEventData(Vector3 pos, int blockType)
        {
            position = pos.ToString();
            block_type = blockType;
            timestamp = Time.time;
        }
    }

    public class AnalyticsManager : MonoBehaviour
    {
        public async void TrackBlockPlacement(Vector3 position, int blockType)
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await InitializeServicesIfNeeded();
            }

            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                var eventData = new BlockPlacementEventData(position, blockType);

                try
                {
                    var evt = new CustomEvent("block_placed")
                    {
                        { "position", eventData.position },
                        { "block_type", eventData.block_type },
                        { "timestamp", eventData.timestamp }
                    };
                    AnalyticsService.Instance.RecordEvent(evt);
                    AnalyticsService.Instance.Flush();
                    Debug.Log("Analytics event recorded: block_placed");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to record analytics event: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("Analytics not initialized. Event not sent.");
            }
        }

        private async System.Threading.Tasks.Task InitializeServicesIfNeeded()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                try
                {
                    await UnityServices.InitializeAsync();
                    Debug.Log("Unity Services initialized.");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Unity Services initialization failed: {e.Message}");
                }
            }
        }
    }
}
