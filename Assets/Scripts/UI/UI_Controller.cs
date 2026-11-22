using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Controller : MonoBehaviour
{
    [SerializeField] private Image healthbar;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text scoreText;

    private NetworkPlayerStats stats;

    public void Bind(NetworkPlayerStats playerStats)
    {
        Unbind();

        stats = playerStats;

        nameText.text = $"Player {playerStats.OwnerClientId}";
        stats.health.CurrentHealth.OnValueChanged += OnHealthChanged;
        stats.Lives.OnValueChanged += OnLivesChanged;
        stats.Score.OnValueChanged += OnScoreChanged;

        Refresh();
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        Refresh();
    }

    private void OnLivesChanged(int oldValue, int newValue)
    {
        Refresh();
    }

    private void OnScoreChanged(int oldValue, int newValue)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (stats == null) return;
        
        healthbar.fillAmount = (float)stats.health.CurrentHealth.Value / (float)stats.health.maxHealth;
        livesText.text = $"Lives: {stats.Lives.Value}";
        scoreText.text = $"Score: {stats.Score.Value}";
    }

    private void Unbind()
    {
        if (stats == null) return;

        nameText.text = $" - ";
        stats.health.CurrentHealth.OnValueChanged -= OnHealthChanged;
        stats.Lives.OnValueChanged -= OnLivesChanged;
        stats.Score.OnValueChanged -= OnScoreChanged;
    }

    private void OnDestroy()
    {
       Unbind();
    }
}
