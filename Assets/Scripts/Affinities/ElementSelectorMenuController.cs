using TMPro;
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

    [Header("Player Selection Visuals")]
    [SerializeField] private Image playerFireImg;
    [SerializeField] private Image playerWaterImg;
    [SerializeField] private Image playerEarthImg;
    [SerializeField] private Image playerAirImg;

    [SerializeField] private TMP_Text playerFireText;
    [SerializeField] private TMP_Text playerWaterText;
    [SerializeField] private TMP_Text playerEarthText;
    [SerializeField] private TMP_Text playerAirText;

    [SerializeField] private Color selectedPlayerColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color defaultPlayerColor  = Color.white;
    
    [Header("Enemy Element Buttons")]
    [SerializeField] private Button enemyFireBtn;
    [SerializeField] private Button enemyWaterBtn;
    [SerializeField] private Button enemyEarthBtn;
    [SerializeField] private Button enemyAirBtn;
    
    [Header("Enemy Selection Visuals")]
    [SerializeField] private Image enemyFireImg;
    [SerializeField] private Image enemyWaterImg;
    [SerializeField] private Image enemyEarthImg;
    [SerializeField] private Image enemyAirImg;
    
    [SerializeField] private TMP_Text enemyFireText;
    [SerializeField] private TMP_Text enemyWaterText;
    [SerializeField] private TMP_Text enemyEarthText;
    [SerializeField] private TMP_Text enemyAirText;
    
    [SerializeField] private Color selectedEnemyColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color defaultEnemyColor  = Color.white;

    private bool playerElementSelected = false;
    private bool enemyElementSelected  = false;
    private ElementType selectedPlayerElement;
    private ElementType selectedEnemyElement;

    private void Start()
    {
        playButton.interactable = false;
        ResetPlayerVisuals();
        ResetEnemyVisuals();
    }

    private void ResetPlayerVisuals()
    {
        if (playerFireImg)  playerFireImg.color  = defaultPlayerColor;
        if (playerWaterImg) playerWaterImg.color = defaultPlayerColor;
        if (playerEarthImg) playerEarthImg.color = defaultPlayerColor;
        if (playerAirImg)   playerAirImg.color   = defaultPlayerColor;

        if (playerFireText)  playerFireText.color  = defaultPlayerColor;
        if (playerWaterText) playerWaterText.color = defaultPlayerColor;
        if (playerEarthText) playerEarthText.color = defaultPlayerColor;
        if (playerAirText)   playerAirText.color   = defaultPlayerColor;
    }
    
    private void ResetEnemyVisuals()
    {
        if (enemyFireImg)  enemyFireImg.color  = defaultEnemyColor;
        if (enemyWaterImg) enemyWaterImg.color = defaultEnemyColor;
        if (enemyEarthImg) enemyEarthImg.color = defaultEnemyColor;
        if (enemyAirImg)   enemyAirImg.color   = defaultEnemyColor;
        
        if (enemyFireText)  enemyFireText.color  = defaultEnemyColor;
        if (enemyWaterText) enemyWaterText.color = defaultEnemyColor;
        if (enemyEarthText) enemyEarthText.color = defaultEnemyColor;
        if (enemyAirText)   enemyAirText.color   = defaultEnemyColor;
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
        UpdatePlayerVisuals(element);
    }

    private void UpdatePlayerVisuals(ElementType element)
    {
        ResetPlayerVisuals();

        Image img = null;
        TMP_Text txt = null;

        switch (element)
        {
            case ElementType.Fire:
                img = playerFireImg;
                txt = playerFireText;
                break;

            case ElementType.Water:
                img = playerWaterImg;
                txt = playerWaterText;
                break;

            case ElementType.Earth:
                img = playerEarthImg;
                txt = playerEarthText;
                break;

            case ElementType.Air:
                img = playerAirImg;
                txt = playerAirText;
                break;
        }

        if (img != null)
            img.color = selectedPlayerColor;

        if (txt != null)
            txt.color = selectedPlayerColor;
    }
    
    // ── ENEMY SELECTION ────────────────────────────────────────────────────
    public void SelectEnemyFire()  => SelectEnemy(ElementType.Fire);
    public void SelectEnemyWater() => SelectEnemy(ElementType.Water);
    public void SelectEnemyEarth() => SelectEnemy(ElementType.Earth);
    public void SelectEnemyAir()   => SelectEnemy(ElementType.Air);

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

        Image img = null;
        TMP_Text txt = null;
        
        switch (element)
        {
            case ElementType.Fire:
                img = enemyFireImg;
                txt = enemyFireText;
                break;

            case ElementType.Water:
                img = enemyWaterImg;
                txt = enemyWaterText;
                break;

            case ElementType.Earth:
                img = enemyEarthImg;
                txt = enemyEarthText;
                break;

            case ElementType.Air:
                img = enemyAirImg;
                txt = enemyAirText;
                break;
        }

        if (img != null)
            img.color = selectedEnemyColor;

        if (txt != null)
            txt.color = selectedEnemyColor;
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
    
    public void Return() => SceneManager.LoadScene("Scenes/MainMenu");
}
