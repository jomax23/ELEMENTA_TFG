using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the Abilities Information screen UI.
/// Handles element selection, ability display, and visual state updates.
/// </summary>
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

    [Header("Ability Info Display")]
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

    private AbilityData[] _currentAbilities = new AbilityData[4];
    private Button[] _elementButtons;
    private Button[] _abilityButtons;

    private void Awake()
    {
        _elementButtons = new[] { fireButton, waterButton, earthButton, airButton };
        _abilityButtons = new[] { abilityButton1, abilityButton2, abilityButton3, abilityButton4 };

        SetupElementButtons();
        SetupAbilityButtons();

        // Select Fire element by default
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
        var set = abilitySets.FirstOrDefault(s => s.element == element);
        if (set == null) return;

        // Update visual selection state of element buttons
        foreach (var btn in _elementButtons)
        {
            SetButtonSelected(btn, btn == selectedButton);
        }

        // Store current abilities for this element
        _currentAbilities[0] = set.ability1;
        _currentAbilities[1] = set.ability2;
        _currentAbilities[2] = set.ability3;
        _currentAbilities[3] = set.ability4;

        // Update ability button icons safely
        UpdateAbilityIcon(abilityIcon1, set.ability1);
        UpdateAbilityIcon(abilityIcon2, set.ability2);
        UpdateAbilityIcon(abilityIcon3, set.ability3);
        UpdateAbilityIcon(abilityIcon4, set.ability4);

        // Update background panel
        UpdateBackground(element);

        // Select the first ability by default if it exists
        if (set.ability1 != null)
        {
            OnAbilitySelected(0);
        }
    }

    private void UpdateAbilityIcon(Image iconImage, AbilityData ability)
    {
        if (iconImage == null) return;
        
        if (ability != null && ability.icon != null)
        {
            iconImage.sprite = ability.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    private void OnAbilitySelected(int index)
    {
        var ability = _currentAbilities[index];
        if (ability == null) return;

        // Update visual selection state of ability buttons
        for (int i = 0; i < _abilityButtons.Length; i++)
        {
            SetButtonSelected(_abilityButtons[i], i == index);
        }

        // Update info display safely
        if (abilityIcon != null) abilityIcon.sprite = ability.icon;
        if (abilityName != null) abilityName.text = ability.abilityName;
        if (abilityDescription != null) abilityDescription.text = ability.description;
    }

    private void SetButtonSelected(Button button, bool selected)
    {
        if (button == null) return;
        
        var image = button.GetComponent<Image>();
        if (image != null)
        {
            // Dim the button when not selected, full opacity when selected
            Color c = image.color;
            c.a = selected ? 1f : 0.4f;
            image.color = c;
        }
    }

    private void UpdateBackground(ElementType element)
    {
        if (backgroundPanel == null) return;

        backgroundPanel.sprite = element switch
        {
            ElementType.Fire => fireBackground,
            ElementType.Water => waterBackground,
            ElementType.Earth => earthBackground,
            ElementType.Air => airBackground,
            _ => defaultBackground
        };
    }

    public void Return() => SceneManager.LoadScene("Scenes/MainMenu");
}