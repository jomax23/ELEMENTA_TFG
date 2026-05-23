using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class GameEndManager : MonoBehaviour
{
    public static GameEndManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Health enemyHealth;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button restartButton;

    private bool hasEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (endPanel != null) endPanel.SetActive(false);
    }

    private void Start()
    {
        // Evento cuando se acaba el tiempo
        if (MatchController.Instance != null)
            MatchController.Instance.OnMatchEnd += EvaluateByHP;

        // Eventos de muerte
        if (playerHealth != null) playerHealth.OnDeath += () => EndGame(false, "You have been defeated.");
        if (enemyHealth != null)  enemyHealth.OnDeath  += () => EndGame(true, "You have defeated the enemy");
    }

    private void OnDestroy()
    {
        if (MatchController.Instance != null)
            MatchController.Instance.OnMatchEnd -= EvaluateByHP;
    }

    private void EvaluateByHP()
    {
        if (hasEnded) return;
        bool wins = playerHealth.health > enemyHealth.health;
        EndGame(wins, wins ? "You have defeated the enemy" : "You have been defeated.");
    }

    private void EndGame(bool victory, string message)
    {
        if (hasEnded) return;
        hasEnded = true;

        Time.timeScale = 0f; // Pausa total
        resultText.text = message;
        endPanel.SetActive(true);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Scenes/MainMenu");
    }
}