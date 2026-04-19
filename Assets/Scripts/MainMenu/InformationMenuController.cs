using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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
    private bool isTransitioning = false;

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
        if (isTransitioning || target == currentPanel)
            return;

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
        panel.interactable = false;
        panel.blocksRaycasts = false;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            panel.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        panel.alpha = 0;
    }

    private IEnumerator FadeIn(CanvasGroup panel)
    {
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            panel.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        panel.alpha = 1;
        panel.interactable = true;
        panel.blocksRaycasts = true;
    }

    private void ShowInstant(CanvasGroup panel)
    {
        panel.alpha = 1;
        panel.interactable = true;
        panel.blocksRaycasts = true;
    }
}