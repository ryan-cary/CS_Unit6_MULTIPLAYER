using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardRow : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private Image healthbar;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text scoreText;

    public void UpdateRow(string name, int health, int lives, int score)
    {
        playerName.text = name;
        healthbar.fillAmount = (float)health / 100f;
        livesText.text = "Lives: " + lives.ToString();
        scoreText.text = "Score: " + score.ToString();
    }
}
