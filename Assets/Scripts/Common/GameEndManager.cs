using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Singleton that handles win/lose conditions.
// Listens to both immediate death events and the match timer running out.
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (endPanel != null) endPanel.SetActive(false);
    }

    private void Start()
    {
        // Listen to match timer ending
        if (MatchController.Instance != null)
            MatchController.Instance.OnMatchEnd += EvaluateByHP;

        // Listen to immediate death events
        if (playerHealth != null) 
            playerHealth.OnDeath += () => EndGame(false, "You have been defeated.");
        if (enemyHealth != null) 
            enemyHealth.OnDeath += () => EndGame(true, "You have defeated the enemy.");
    }

    private void OnDestroy()
    {
        if (MatchController.Instance != null)
            MatchController.Instance.OnMatchEnd -= EvaluateByHP;
    }

    // Called when the match timer runs out. Compares remaining HP to determine the winner.
    private void EvaluateByHP()
    {
        if (hasEnded) return;
        
        bool playerWins = playerHealth.CurrentHealth > enemyHealth.CurrentHealth;
        string msg = playerWins ? "You have defeated the enemy." : "You have been defeated.";
        EndGame(playerWins, msg);
    }

    private void EndGame(bool victory, string message)
    {
        if (hasEnded) return;
        hasEnded = true;
        
        Time.timeScale = 0f; // Pause the game
        
        if (resultText != null) resultText.text = message;
        if (endPanel != null) endPanel.SetActive(true);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Scenes/MainMenu");
    }
}