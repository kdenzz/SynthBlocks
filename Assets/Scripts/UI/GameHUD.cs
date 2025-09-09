using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class GameHUD : MonoBehaviour
    {
        public static GameHUD I;

        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI linesText;
        [SerializeField] private GameObject scorePanel;
        [SerializeField] private GameObject gameOverPanel;

        void Awake()
        {
            I = this;
            scorePanel.SetActive(true);
            gameOverPanel.SetActive(false);
        }

        public void SetScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }
        }

        public void SetLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = level.ToString();
            }
        }

        public void SetLines(int lines)
        {
            if (linesText != null)
            {
                linesText.text = lines.ToString();
            }
        }

        public void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                scorePanel.SetActive(false);
                gameOverPanel.SetActive(true);
            }
        }
    }
}


