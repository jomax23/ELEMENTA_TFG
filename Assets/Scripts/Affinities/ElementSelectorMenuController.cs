using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the Element Selection Menu where the player chooses their main element
/// and the enemy's element before starting the match.
/// </summary>
public class ElementSelectorMenuController : MonoBehaviour
{
    [Header("Play Button")]
    [SerializeField] private Button playButton;
    
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Scenes/Map1";

    [Header("Player Affinity Rows")]
    [SerializeField] private AffinityRowUI rowFire;
    [SerializeField] private AffinityRowUI rowWater;
    [SerializeField] private AffinityRowUI rowEarth;
    [SerializeField] private AffinityRowUI rowAir;

    [Header("Player Selection Visuals")]
    [SerializeField] private Image playerFireImg;
    [SerializeField] private Image playerWaterImg;
    [SerializeField] private Image playerEarthImg;
    [SerializeField] private Image playerAirImg;
    
    [SerializeField] private TextMeshProUGUI playerFireText;
    [SerializeField] private TextMeshProUGUI playerWaterText;
    [SerializeField] private TextMeshProUGUI playerEarthText;
    [SerializeField] private TextMeshProUGUI playerAirText;
    
    [SerializeField] private Color selectedPlayerColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color defaultPlayerColor = Color.white;

    [Header("Enemy Selection Visuals")]
    [SerializeField] private Image enemyFireImg;
    [SerializeField] private Image enemyWaterImg;
    [SerializeField] private Image enemyEarthImg;
    [SerializeField] private Image enemyAirImg;
    
    [SerializeField] private TextMeshProUGUI enemyFireText;
    [SerializeField] private TextMeshProUGUI enemyWaterText;
    [SerializeField] private TextMeshProUGUI enemyEarthText;
    [SerializeField] private TextMeshProUGUI enemyAirText;
    
    [SerializeField] private Color selectedEnemyColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color defaultEnemyColor = Color.white;

    // Cached arrays for cleaner, allocation-free visual updates
    private Image[] playerImages;
    private TextMeshProUGUI[] playerTexts;
    private Image[] enemyImages;
    private TextMeshProUGUI[] enemyTexts;

    private bool playerElementSelected;
    private bool enemyElementSelected;
    private ElementType selectedPlayerElement;
    private ElementType selectedEnemyElement;

    private void Awake()
    {
        // Map serialized fields to arrays indexed by (int)ElementType.
        // This assumes ElementType enum is ordered: Fire=0, Water=1, Earth=2, Air=3.
        playerImages = new[] { playerFireImg, playerWaterImg, playerEarthImg, playerAirImg };
        playerTexts = new[] { playerFireText, playerWaterText, playerEarthText, playerAirText };
        enemyImages = new[] { enemyFireImg, enemyWaterImg, enemyEarthImg, enemyAirImg };
        enemyTexts = new[] { enemyFireText, enemyWaterText, enemyEarthText, enemyAirText };
    }

    private void Start()
    {
        playButton.interactable = false;
        ResetPlayerVisuals();
        ResetEnemyVisuals();
    }

    private void ResetPlayerVisuals()
    {
        for (int i = 0; i < playerImages.Length; i++)
        {
            if (playerImages[i] != null) playerImages[i].color = defaultPlayerColor;
            if (playerTexts[i] != null) playerTexts[i].color = defaultPlayerColor;
        }
    }

    private void ResetEnemyVisuals()
    {
        for (int i = 0; i < enemyImages.Length; i++)
        {
            if (enemyImages[i] != null) enemyImages[i].color = defaultEnemyColor;
            if (enemyTexts[i] != null) enemyTexts[i].color = defaultEnemyColor;
        }
    }

    // ── PLAYER SELECTION ─────────────────────────────────
    public void SelectFire() => SelectPlayer(ElementType.Fire);
    public void SelectWater() => SelectPlayer(ElementType.Water);
    public void SelectEarth() => SelectPlayer(ElementType.Earth);
    public void SelectAir() => SelectPlayer(ElementType.Air);

    private void SelectPlayer(ElementType element)
    {
        selectedPlayerElement = element;
        playerElementSelected = true;

        UpdatePlayButton();
        UpdateAffinityRows(element);
        UpdatePlayerVisuals(element);
    }

    private void UpdatePlayerVisuals(ElementType element)
    {
        ResetPlayerVisuals();
        int index = (int)element;
        
        if (playerImages[index] != null) playerImages[index].color = selectedPlayerColor;
        if (playerTexts[index] != null) playerTexts[index].color = selectedPlayerColor;
    }

    // ── ENEMY SELECTION ──────────────────────────────────
    public void SelectEnemyFire() => SelectEnemy(ElementType.Fire);
    public void SelectEnemyWater() => SelectEnemy(ElementType.Water);
    public void SelectEnemyEarth() => SelectEnemy(ElementType.Earth);
    public void SelectEnemyAir() => SelectEnemy(ElementType.Air);

    private void SelectEnemy(ElementType element)
    {
        selectedEnemyElement = element;
        enemyElementSelected = true;
        
        UpdatePlayButton();
        UpdateEnemyVisuals(element);
    }

    private void UpdateEnemyVisuals(ElementType element)
    {
        ResetEnemyVisuals();
        int index = (int)element;
        
        if (enemyImages[index] != null) enemyImages[index].color = selectedEnemyColor;
        if (enemyTexts[index] != null) enemyTexts[index].color = selectedEnemyColor;
    }

    private void UpdatePlayButton()
    {
        playButton.interactable = playerElementSelected && enemyElementSelected;
    }

    private void UpdateAffinityRows(ElementType element)
    {
        var data = GameSession.Instance?.AffinityData;
        if (data == null) return;

        rowFire.SetData(ElementType.Fire, data.GetAffinityInfo(element, ElementType.Fire));
        rowWater.SetData(ElementType.Water, data.GetAffinityInfo(element, ElementType.Water));
        rowEarth.SetData(ElementType.Earth, data.GetAffinityInfo(element, ElementType.Earth));
        rowAir.SetData(ElementType.Air, data.GetAffinityInfo(element, ElementType.Air));
    }

    /// <summary>
    /// Called when the player confirms their selection and starts the match.
    /// </summary>
    public void OnPlayPressed()
    {
        if (!playerElementSelected || !enemyElementSelected) return;
        
        GameSession.Instance?.SetMainElement(selectedPlayerElement);
        GameSession.Instance?.SetEnemyElement(selectedEnemyElement);
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Returns to the Main Menu scene.
    /// </summary>
    public void Return() => SceneManager.LoadScene("Scenes/MainMenu");
}