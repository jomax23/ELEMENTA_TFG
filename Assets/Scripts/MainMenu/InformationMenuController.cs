using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// Manages smooth fade transitions between UI panels using CanvasGroups.
public class InformationMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private CanvasGroup mainMenu;
    [SerializeField] private CanvasGroup settingsMenu;
    [SerializeField] private CanvasGroup abilitiesMenu;
    [SerializeField] private CanvasGroup affinitiesMenu;

    [Header("Transition")]
    [SerializeField] private float fadeDuration = 0.3f;

    private CanvasGroup currentPanel;
    private bool isTransitioning;

    private void Start()
    {
        currentPanel = mainMenu;
        ShowInstant(mainMenu);
    }

    public void OpenSettings() => SwitchTo(settingsMenu);
    public void OpenAbilities() => SwitchTo(abilitiesMenu);
    public void OpenAffinities() => SwitchTo(affinitiesMenu);
    public void BackToMain() => SwitchTo(mainMenu);
    public void Return() => SceneManager.LoadScene("Scenes/MainMenu");

    private void SwitchTo(CanvasGroup target)
    {
        if (isTransitioning || target == currentPanel) return;
        StartCoroutine(FadeTransition(currentPanel, target));
    }

    private IEnumerator FadeTransition(CanvasGroup from, CanvasGroup to)
    {
        isTransitioning = true;
        yield return StartCoroutine(FadeOut(from));
        yield return StartCoroutine(FadeIn(to));
        currentPanel = to;
        isTransitioning = false;
    }

    private IEnumerator FadeOut(CanvasGroup panel)
    {
        // Block raycasts immediately so players can't click through the fading panel
        panel.interactable = false;
        panel.blocksRaycasts = false;
        
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            panel.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        panel.alpha = 0f;
    }

    private IEnumerator FadeIn(CanvasGroup panel)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            // Use unscaled delta time so transitions work perfectly even if the game is paused
            elapsed += Time.unscaledDeltaTime;
            panel.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        
        // Re-enable interactions only after the fade is fully complete
        panel.alpha = 1f;
        panel.interactable = true;
        panel.blocksRaycasts = true;
    }

    private void ShowInstant(CanvasGroup panel)
    {
        panel.alpha = 1f;
        panel.interactable = true;
        panel.blocksRaycasts = true;
    }
}