using UnityEngine;

namespace Utils
{
    public class PerformanceMonitor : MonoBehaviour
    {
        [SerializeField] private float updateInterval = 0.5f;
        private float timeAccumulator;
        private int frames;
        private float fps;

        void Update()
        {
            timeAccumulator += Time.unscaledDeltaTime;
            frames++;
            if (timeAccumulator >= updateInterval)
            {
                fps = frames / timeAccumulator;
                frames = 0;
                timeAccumulator = 0f;
            }
        }

        public float CurrentFps => fps;
    }
}


