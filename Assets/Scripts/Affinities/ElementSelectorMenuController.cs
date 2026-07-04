using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Main controller for the pre-match Element Selection screen.
// Handles player/enemy picks, updates the affinity preview table, and manages debug toggles.
public class ElementSelectorMenuController : MonoBehaviour
{
    [Header("Play Button")]
    [SerializeField] private Button playButton;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Scenes/Map1";

    [Header("Debug / Settings")]
    [SerializeField] private Button detectionToggleBtn;
    [Tooltip("Index 0 = Inactive, Index 1 = Active")]
    [SerializeField] private List<Sprite> detectionSprites;
    private Image toggleImage;

    [SerializeField] private Button masterControlToggleBtn;
    [Tooltip("Index 0 = Inactive, Index 1 = Active")]
    [SerializeField] private List<Sprite> masterControlSprites;
    private Image masterControlToggleImage;

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

    // Arrays mapped to ElementType enum indices for fast, allocation-free visual updates.
    // IMPORTANT: Assumes ElementType enum order is Fire=0, Earth=1, Water=2, Air=3.
    private Image[] playerImages;
    private TextMeshProUGUI[] playerTexts;
    private Image[] enemyImages;
    private TextMeshProUGUI[] enemyTexts;

    private bool playerElementSelected = false;
    private bool enemyElementSelected = false;
    private ElementType selectedPlayerElement;
    private ElementType selectedEnemyElement;

    private void Awake()
    {
        // Map serialized fields to arrays using the exact enum order
        playerImages = new[] { playerFireImg, playerEarthImg, playerWaterImg, playerAirImg };
        playerTexts = new[] { playerFireText, playerEarthText, playerWaterText, playerAirText };
        enemyImages = new[] { enemyFireImg, enemyEarthImg, enemyWaterImg, enemyAirImg };
        enemyTexts = new[] { enemyFireText, enemyEarthText, enemyWaterText, enemyAirText };
    }

    private void Start()
    {
        playButton.interactable = false;
        ResetPlayerVisuals();
        ResetEnemyVisuals();

        if (detectionToggleBtn != null)
        {
            toggleImage = detectionToggleBtn.GetComponent<Image>();
            detectionToggleBtn.onClick.AddListener(OnDetectionToggleClicked);
            UpdateDetectionButtonVisuals();
        }

        if (masterControlToggleBtn != null)
        {
            masterControlToggleImage = masterControlToggleBtn.GetComponent<Image>();
            masterControlToggleBtn.onClick.AddListener(OnMasterControlToggleClicked);
            UpdateMasterControlButtonVisuals();
        }
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

    // --- Debug Toggles ---

    private void OnDetectionToggleClicked()
    {
        if (GameSession.Instance == null) return;
        bool newState = !GameSession.Instance.EnemyDetectionActive;
        GameSession.Instance.SetEnemyDetectionActive(newState);
        UpdateDetectionButtonVisuals();
    }

    private void UpdateDetectionButtonVisuals()
    {
        if (toggleImage == null || detectionSprites == null || detectionSprites.Count < 2) return;
        
        bool isActive = GameSession.Instance != null && GameSession.Instance.EnemyDetectionActive;
        toggleImage.sprite = isActive ? detectionSprites[1] : detectionSprites[0];
    }

    private void OnMasterControlToggleClicked()
    {
        if (GameSession.Instance == null) return;
        bool newState = !GameSession.Instance.ForceMasterControl;
        GameSession.Instance.SetForceMasterControl(newState);
        UpdateMasterControlButtonVisuals();
    }

    private void UpdateMasterControlButtonVisuals()
    {
        if (masterControlToggleImage == null || masterControlSprites == null || masterControlSprites.Count < 2) return;
        
        bool isActive = GameSession.Instance != null && GameSession.Instance.ForceMasterControl;
        masterControlToggleImage.sprite = isActive ? masterControlSprites[1] : masterControlSprites[0];
    }

    // --- Player Selection ---

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

    // --- Enemy Selection ---

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

    // --- Validation & Affinity ---

    // Play button only enables when both player and enemy have picked an element
    private void UpdatePlayButton()
    {
        playButton.interactable = playerElementSelected && enemyElementSelected;
    }

    private void UpdateAffinityRows(ElementType element)
    {
        var data = GameSession.Instance?.AffinityData;
        if (data == null) return;

        // Update the preview table to show how the chosen main element interacts with all 4 elements
        rowFire.SetData(ElementType.Fire, data.GetAffinityInfo(element, ElementType.Fire));
        rowWater.SetData(ElementType.Water, data.GetAffinityInfo(element, ElementType.Water));
        rowEarth.SetData(ElementType.Earth, data.GetAffinityInfo(element, ElementType.Earth));
        rowAir.SetData(ElementType.Air, data.GetAffinityInfo(element, ElementType.Air));
    }

    // --- Navigation ---

    public void OnPlayPressed()
    {
        if (!playerElementSelected || !enemyElementSelected) return;
        
        // Lock in the choices to the persistent GameSession before loading the arena
        GameSession.Instance?.SetMainElement(selectedPlayerElement);
        GameSession.Instance?.SetEnemyElement(selectedEnemyElement);
        
        SceneManager.LoadScene(gameSceneName);
    }

    public void Return() => SceneManager.LoadScene("Scenes/MainMenu");
}