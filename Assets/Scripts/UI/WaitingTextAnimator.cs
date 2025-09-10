using UnityEngine;
using TMPro;

public class WaitingTextAnimator : MonoBehaviour
{
    [SerializeField] private TMP_Text waitingText; // Assign your TMP_Text component here in the Inspector
    [SerializeField] private string baseText = "waiting to join"; // Base text without dots
    [SerializeField] private float animationInterval = 0.5f; // Time between dot changes (in seconds)

    private int dotCount = 0; // Current number of dots (0 to 3 for 1 to 4 dots)
    private float timer = 0f;

    private void Start()
    {
        if (waitingText == null)
        {
            // Automatically find the TMP_Text child if not assigned
            waitingText = GetComponentInChildren<TMP_Text>();
        }
        UpdateText();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= animationInterval)
        {
            timer = 0f;
            dotCount = (dotCount + 1) % 4; // Cycle from 0 to 3 (representing 1 to 4 dots)
            UpdateText();
        }
    }

    private void UpdateText()
    {
        // Add 1 to dotCount to make it 1-4 dots
        waitingText.text = baseText + new string('.', dotCount + 1);
    }
}
