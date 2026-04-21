using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Añade retroalimentación de audio a cualquier botón de UI Canvas.
///
/// Uso:
///   1. Añade este componente al GameObject del botón.
///   2. Asigna un SoundData en el Inspector (o deja el default del AudioManager).
///
/// También captura el evento OnPointerEnter opcionalmente para un hover suave.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Sounds")]
    [SerializeField] private SoundData clickSound;
    [SerializeField] private SoundData hoverSound;   // opcional

    // ─────────────────────────────────────────────────────────────────────────

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
            AudioManager.Instance?.PlaySFX(clickSound);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
            AudioManager.Instance?.PlaySFX(hoverSound);
    }
}