using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Hooks into Unity's UI event system to play sounds on hover and click.
// Keeps audio logic completely out of the actual button scripts.
[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Sounds")]
    [SerializeField] private SoundData clickSound;
    [SerializeField] private SoundData hoverSound; 

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