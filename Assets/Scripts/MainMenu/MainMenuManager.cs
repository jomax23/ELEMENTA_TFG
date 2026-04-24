using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona el menú principal.
///
/// CAMBIO vs versión anterior:
///   Play() ya no carga directamente Map1. En su lugar, carga la escena de selección
///   de elemento (ElementSelector) donde el jugador elige su elemento principal.
///   La escena de juego se carga desde <see cref="ElementSelectorMenuController.OnPlayPressed"/>.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string elementSelectorSceneName = "Scenes/ElementSelector";
    [SerializeField] private string infoSceneName            = "Scenes/Info";

    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Carga la pantalla de selección de elemento.
    /// Desde allí el jugador elige su elemento principal y pulsa Play.
    /// </summary>
    public void Play()
    {
        // Resetear sesión anterior (por si el jugador vuelve del juego)
        GameSession.Instance?.ResetSession();

        SceneManager.LoadScene(elementSelectorSceneName);
    }

    public void Info()
    {
        SceneManager.LoadScene(infoSceneName);
    }

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}