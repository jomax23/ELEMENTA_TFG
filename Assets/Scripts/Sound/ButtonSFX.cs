using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Adds audio feedback to any UI Canvas Button.
/// Attach this component to a Button GameObject and assign SoundData in the Inspector.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Sounds")]
    [SerializeField] private SoundData clickSound;
    [SerializeField] private SoundData hoverSound; // Optional

    /// <summary>
    /// Triggered when the button is clicked.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
            AudioManager.Instance?.PlaySFX(clickSound);
    }

    /// <summary>
    /// Triggered when the pointer enters the button area (hover).
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
            AudioManager.Instance?.PlaySFX(hoverSound);
    }
}