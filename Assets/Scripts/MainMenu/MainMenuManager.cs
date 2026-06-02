using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages navigation from the Main Menu.
/// Note: Play() loads the Element Selector scene instead of the gameplay scene directly.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string elementSelectorSceneName = "Scenes/ElementSelector";
    [SerializeField] private string infoSceneName = "Scenes/Info";
    [SerializeField] private string tutorialSceneName = "Scenes/Tutorial";
    [SerializeField] private string abilitiesSceneName = "Scenes/Abilities";

    /// <summary>
    /// Resets the current game session and loads the element selection screen.
    /// </summary>
    public void Play()
    {
        GameSession.Instance?.ResetSession();
        SceneManager.LoadScene(elementSelectorSceneName);
    }

    /// <summary>Loads the Information/About screen.</summary>
    public void Info() => SceneManager.LoadScene(infoSceneName);

    /// <summary>Loads the Tutorial screen.</summary>
    public void Tutorial() => SceneManager.LoadScene(tutorialSceneName);

    /// <summary>Loads the Abilities explanation screen.</summary>
    public void Abilities() => SceneManager.LoadScene(abilitiesSceneName);

    /// <summary>Quits the game or stops play mode in the Editor.</summary>
    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}