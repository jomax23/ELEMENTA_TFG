using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ElementSelectorMenuController : MonoBehaviour
{
    [Header("Play Button")]
    [SerializeField] private Button playButton;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Scenes/Map1";

    [Header("Affinity Rows (Fire, Water, Earth, Air)")]
    [SerializeField] private AffinityRowUI rowFire;
    [SerializeField] private AffinityRowUI rowWater;
    [SerializeField] private AffinityRowUI rowEarth;
    [SerializeField] private AffinityRowUI rowAir;

    private bool elementSelected = false;
    private ElementType selectedElement;

    private void Start()
    {
        playButton.interactable = false;
    }

    // Llama a estos métodos desde el OnClick de cada botón en el Inspector
    public void SelectFire()  => Select(ElementType.Fire);
    public void SelectWater() => Select(ElementType.Water);
    public void SelectEarth() => Select(ElementType.Earth);
    public void SelectAir()   => Select(ElementType.Air);

    private void Select(ElementType element)
    {
        selectedElement  = element;
        elementSelected  = true;
        playButton.interactable = true;

        AffinityData data = GameSession.Instance?.AffinityData;
        if (data == null) return;

        rowFire.SetData (ElementType.Fire,  data.GetAffinityInfo(element, ElementType.Fire));
        rowWater.SetData(ElementType.Water, data.GetAffinityInfo(element, ElementType.Water));
        rowEarth.SetData(ElementType.Earth, data.GetAffinityInfo(element, ElementType.Earth));
        rowAir.SetData  (ElementType.Air,   data.GetAffinityInfo(element, ElementType.Air));
    }

    public void OnPlayPressed()
    {
        if (!elementSelected) return;
        GameSession.Instance?.SetMainElement(selectedElement);
        SceneManager.LoadScene(gameSceneName);
    }
}