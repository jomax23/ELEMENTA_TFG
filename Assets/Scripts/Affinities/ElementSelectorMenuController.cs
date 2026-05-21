using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Enemy Element Buttons")]
    [SerializeField] private Button enemyFireBtn;
    [SerializeField] private Button enemyWaterBtn;
    [SerializeField] private Button enemyEarthBtn;
    [SerializeField] private Button enemyAirBtn;
    
    [Header("Enemy Selection Visuals")] // ← Para el feedback
    [SerializeField] private Image enemyFireImg;
    [SerializeField] private Image enemyWaterImg;
    [SerializeField] private Image enemyEarthImg;
    [SerializeField] private Image enemyAirImg;
    [SerializeField] private Color selectedEnemyColor = new Color(1f, 0.8f, 0.2f); // Dorado
    [SerializeField] private Color defaultEnemyColor  = Color.white;

    private bool playerElementSelected = false;
    private bool enemyElementSelected  = false;
    private ElementType selectedPlayerElement;
    private ElementType selectedEnemyElement;

    private void Start()
    {
        playButton.interactable = false;
        ResetEnemyVisuals();
    }

    private void ResetEnemyVisuals()
    {
        if (enemyFireImg)  enemyFireImg.color  = defaultEnemyColor;
        if (enemyWaterImg) enemyWaterImg.color = defaultEnemyColor;
        if (enemyEarthImg) enemyEarthImg.color = defaultEnemyColor;
        if (enemyAirImg)   enemyAirImg.color   = defaultEnemyColor;
    }

    // ── PLAYER SELECTION (igual que antes) ─────────────────────────────────
    public void SelectFire()  => SelectPlayer(ElementType.Fire);
    public void SelectWater() => SelectPlayer(ElementType.Water);
    public void SelectEarth() => SelectPlayer(ElementType.Earth);
    public void SelectAir()   => SelectPlayer(ElementType.Air);

    private void SelectPlayer(ElementType element)
    {
        selectedPlayerElement = element;
        playerElementSelected = true;
        UpdatePlayButton();
        UpdateAffinityRows(element);
    }

    // ── ENEMY SELECTION (Funciones públicas para el Inspector) ─────────────
    // Asigna estas funciones directamente en el OnClick de cada botón
    public void SelectEnemyFire()  => SelectEnemy(ElementType.Fire);
    public void SelectEnemyWater() => SelectEnemy(ElementType.Water);
    public void SelectEnemyEarth() => SelectEnemy(ElementType.Earth);
    public void SelectEnemyAir()   => SelectEnemy(ElementType.Air);

    private void SelectEnemy(ElementType element)
    {
        selectedEnemyElement = element;
        enemyElementSelected = true;
        UpdatePlayButton();
        UpdateEnemyVisuals(element); // ← Feedback visual
    }

    private void UpdateEnemyVisuals(ElementType element)
    {
        ResetEnemyVisuals(); // Primero deselecciona todos

        // Luego resalta el seleccionado
        Image img = element switch
        {
            ElementType.Fire  => enemyFireImg,
            ElementType.Water => enemyWaterImg,
            ElementType.Earth => enemyEarthImg,
            ElementType.Air   => enemyAirImg,
            _ => null
        };
        
        if (img != null)
            img.color = selectedEnemyColor;
    }

    private void UpdatePlayButton()
    {
        playButton.interactable = playerElementSelected && enemyElementSelected;
    }

    private void UpdateAffinityRows(ElementType element)
    {
        var data = GameSession.Instance?.AffinityData;
        if (data == null) return;

        rowFire.SetData (ElementType.Fire,  data.GetAffinityInfo(element, ElementType.Fire));
        rowWater.SetData(ElementType.Water, data.GetAffinityInfo(element, ElementType.Water));
        rowEarth.SetData(ElementType.Earth, data.GetAffinityInfo(element, ElementType.Earth));
        rowAir.SetData  (ElementType.Air,   data.GetAffinityInfo(element, ElementType.Air));
    }

    public void OnPlayPressed()
    {
        if (!playerElementSelected || !enemyElementSelected) return;
        
        GameSession.Instance?.SetMainElement(selectedPlayerElement);
        GameSession.Instance?.SetEnemyElement(selectedEnemyElement);
        SceneManager.LoadScene(gameSceneName);
    }
}