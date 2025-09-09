using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;

namespace Cloud
{
    public class CloudSaveManager : MonoBehaviour
    {
        public async Task SavePlayerProgressAsync(object data)
        {
            try
            {
                await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object>
                {
                    { "playerData", data }
                });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Cloud Save failed: {e.Message}");
            }
        }
    }
}


