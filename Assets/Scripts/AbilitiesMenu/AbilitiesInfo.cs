using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
public class AbilitiesInfo : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button fireButton;
    [SerializeField] private Button waterButton;
    [SerializeField] private Button earthButton;
    [SerializeField] private Button airButton;
    
    [SerializeField] private Button abilityButton1;
    [SerializeField] private Button abilityButton2;
    [SerializeField] private Button abilityButton3;
    [SerializeField] private Button abilityButton4;
    
    [SerializeField] private Image abilityIcon1;
    [SerializeField] private Image abilityIcon2;
    [SerializeField] private Image abilityIcon3;
    [SerializeField] private Image abilityIcon4;

    [Header("Ability Info")]
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TextMeshProUGUI abilityName;
    [SerializeField] private TextMeshProUGUI abilityDescription;
    [SerializeField] private Image backgroundPanel;

    [Header("Backgrounds")]
    [SerializeField] private Sprite defaultBackground;
    [SerializeField] private Sprite fireBackground;
    [SerializeField] private Sprite waterBackground;
    [SerializeField] private Sprite earthBackground;
    [SerializeField] private Sprite airBackground;

    [Header("Data")]
    [SerializeField] private ElementAbilitySet[] abilitySets;

    private ElementAbilitySet[] _sets;
    private AbilityData[] _currentAbilities = new AbilityData[4];
    private Button[] _elementButtons;
    private Button[] _abilityButtons;

    private void Awake()
    {
        _sets = abilitySets;
        
        _elementButtons = new Button[] { fireButton, waterButton, earthButton, airButton };
        _abilityButtons = new Button[] { abilityButton1, abilityButton2, abilityButton3, abilityButton4 };
        
        SetupElementButtons();
        SetupAbilityButtons();
        
        // Seleccionar Fire por defecto
        OnElementSelected(ElementType.Fire, fireButton);
    }

    private void SetupElementButtons()
    {
        fireButton.onClick.AddListener(() => OnElementSelected(ElementType.Fire, fireButton));
        waterButton.onClick.AddListener(() => OnElementSelected(ElementType.Water, waterButton));
        earthButton.onClick.AddListener(() => OnElementSelected(ElementType.Earth, earthButton));
        airButton.onClick.AddListener(() => OnElementSelected(ElementType.Air, airButton));
    }

    private void SetupAbilityButtons()
    {
        abilityButton1.onClick.AddListener(() => OnAbilitySelected(0));
        abilityButton2.onClick.AddListener(() => OnAbilitySelected(1));
        abilityButton3.onClick.AddListener(() => OnAbilitySelected(2));
        abilityButton4.onClick.AddListener(() => OnAbilitySelected(3));
    }

    private void OnElementSelected(ElementType element, Button selectedButton)
    {
        var set = _sets.FirstOrDefault(s => s.element == element);
        if (set == null) return;

        // Actualizar selección visual de elementos
        foreach (var btn in _elementButtons)
            SetButtonSelected(btn, btn == selectedButton);

        // Guardar habilidades actuales
        _currentAbilities[0] = set.ability1;
        _currentAbilities[1] = set.ability2;
        _currentAbilities[2] = set.ability3;
        _currentAbilities[3] = set.ability4;

        // Actualizar iconos de botones de habilidad
        abilityIcon1.sprite = set.ability1?.icon;
        abilityIcon2.sprite = set.ability2?.icon;
        abilityIcon3.sprite = set.ability3?.icon;
        abilityIcon4.sprite = set.ability4?.icon;

        // Actualizar fondo
        UpdateBackground(element);

        // Seleccionar primera habilidad por defecto
        if (set.ability1 != null)
            OnAbilitySelected(0);
    }

    private void OnAbilitySelected(int index)
    {
        var ability = _currentAbilities[index];
        if (ability == null) return;

        // Actualizar selección visual
        for (int i = 0; i < _abilityButtons.Length; i++)
            SetButtonSelected(_abilityButtons[i], i == index);

        // Mostrar info
        abilityIcon.sprite = ability.icon;
        abilityName.text = ability.abilityName;
        abilityDescription.text = ability.description;
    }

    private void SetButtonSelected(Button button, bool selected)
    {
        var image = button.GetComponent<Image>();
        
    }

    private void UpdateBackground(ElementType element)
    {
        if (backgroundPanel == null) return;

        backgroundPanel.sprite = element switch
        {
            ElementType.Fire  => fireBackground,
            ElementType.Water => waterBackground,
            ElementType.Earth => earthBackground,
            ElementType.Air   => airBackground,
            _ => defaultBackground
        };
    }
    
    public void Return() => SceneManager.LoadScene("Scenes/MainMenu");
}