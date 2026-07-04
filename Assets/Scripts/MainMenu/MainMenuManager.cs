using UnityEngine;
using UnityEngine.SceneManagement;

// Simple scene router for the main menu.
// Note: Play() goes to the Element Selector first, not directly to the gameplay scene.
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string elementSelectorSceneName = "Scenes/ElementSelector";
    [SerializeField] private string infoSceneName = "Scenes/Info";
    [SerializeField] private string tutorialSceneName = "Scenes/Tutorial";
    [SerializeField] private string abilitiesSceneName = "Scenes/Abilities";

    public void Play()
    {
        GameSession.Instance?.ResetSession();
        SceneManager.LoadScene(elementSelectorSceneName);
    }

    public void Info() => SceneManager.LoadScene(infoSceneName);
    public void Tutorial() => SceneManager.LoadScene(tutorialSceneName);
    public void Abilities() => SceneManager.LoadScene(abilitiesSceneName);

    // Handles quitting safely in both the Unity Editor and standalone builds
    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}