using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Central button manager for UI buttons. Provides methods to show/close/toggle
/// the `gameInfoContainer` popup and helper to clear EventSystem selection.
/// Attach this to a persistent UI manager object and wire its public methods to
/// Button OnClick events.
/// </summary>
public class ButtonManager : MonoBehaviour
{
    [Tooltip("Assign the GameObject popup (gameInfoContainer). If left empty, the script will try to find by TargetName at runtime.")]
    public GameObject gameInfoContainer;

    [Tooltip("Optional: name used to find the popup if gameInfoContainer is not assigned.")]
    public string TargetName = "gameInfoContainer";

    public GameObject overlay;
    public GameObject optionsMenu;
    public GameObject settingsMenu;
    public GameObject creditsMenu;
    [Tooltip("Optional: name to find the options menu GameObject if optionsMenu is not assigned.")]
    public string optionsMenuName = "optionsContainer";
    [Tooltip("Optional: name to find the settings menu GameObject if settingsMenu is not assigned.")]
    public string settingsMenuName = "settingsContainer";
    [Tooltip("Optional: name to find the credits menu GameObject if creditsMenu is not assigned.")]
    public string creditsMenuName = "creditsContainer";

    void Awake()
    {
        EnsureTarget();
        EnsureMenu(ref optionsMenu, optionsMenuName, nameof(optionsMenu));
        EnsureMenu(ref settingsMenu, settingsMenuName, nameof(settingsMenu));
        EnsureMenu(ref creditsMenu, creditsMenuName, nameof(creditsMenu));
    }

    /// <summary>
    /// Show the game info popup
    /// </summary>
    public void ShowGameInfo()
    {
        EnsureTarget();
        if (gameInfoContainer == null) return;
        // prevent repeated open
        if (gameInfoContainer.activeSelf) return;
        gameInfoContainer.SetActive(true);
        UpdateOverlayState();
    }

    /// <summary>
    /// Close the game info popup and clear UI selection
    /// </summary>
    public void CloseGameInfo()
    {
        EnsureTarget();
        if (gameInfoContainer != null)
            gameInfoContainer.SetActive(false);
        UpdateOverlayState();
        ClearSelection();
    }

    /// <summary>
    /// Toggle the popup open/closed. If closing, also clear selection.
    /// </summary>
    public void ToggleGameInfo()
    {
        EnsureTarget();
        if (gameInfoContainer == null) return;
        bool now = !gameInfoContainer.activeSelf;
        gameInfoContainer.SetActive(now);
        UpdateOverlayState();
        if (!now) ClearSelection();
    }

    public void CloseAndClearSelection()
    {
        CloseGameInfo();
        ClearSelection();
    }

    void EnsureTarget()
    {
        if (gameInfoContainer == null && !string.IsNullOrEmpty(TargetName))
        {
            var go = GameObject.Find(TargetName);
            if (go != null) gameInfoContainer = go;
        }
    }

    void ClearSelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    // === OPTIONS ===
    public void OpenOptions()
    {
        if (!EnsureMenu(ref optionsMenu, optionsMenuName, nameof(optionsMenu))) return;
        if (optionsMenu.activeSelf) return; // already open
        optionsMenu.SetActive(true);
        UpdateOverlayState();
    }

    public void CloseOptions()
    {
        if (!EnsureMenu(ref optionsMenu, optionsMenuName, nameof(optionsMenu))) return;
        optionsMenu.SetActive(false);
        UpdateOverlayState();
    }

    // === SETTINGS ===
    public void OpenSettings()
    {
        if (!EnsureMenu(ref settingsMenu, settingsMenuName, nameof(settingsMenu))) return;
        // optionally hide optionsMenu if present
        if (optionsMenu == null) EnsureMenu(ref optionsMenu, optionsMenuName, nameof(optionsMenu));
        if (optionsMenu != null) optionsMenu.SetActive(false);
        if (settingsMenu.activeSelf) return;
        settingsMenu.SetActive(true);
        UpdateOverlayState();
    }

    public void CloseSettings()
    {
        if (!EnsureMenu(ref settingsMenu, settingsMenuName, nameof(settingsMenu))) return;
        settingsMenu.SetActive(false);
        UpdateOverlayState();
        if (!EnsureMenu(ref optionsMenu, optionsMenuName, nameof(optionsMenu))) return;
        optionsMenu.SetActive(true);
    }

    // === CREDITS ===
    public void OpenCredits()
    {
        if (!EnsureMenu(ref creditsMenu, creditsMenuName, nameof(creditsMenu))) return;
        if (optionsMenu == null) EnsureMenu(ref optionsMenu, optionsMenuName, nameof(optionsMenu));
        if (optionsMenu != null) optionsMenu.SetActive(false);
        if (creditsMenu.activeSelf) return;
        creditsMenu.SetActive(true);
        UpdateOverlayState();
    }

    public void CloseCredits()
    {
        if (!EnsureMenu(ref creditsMenu, creditsMenuName, nameof(creditsMenu))) return;
        creditsMenu.SetActive(false);
        UpdateOverlayState();
        if (!EnsureMenu(ref optionsMenu, optionsMenuName, nameof(optionsMenu))) return;
        optionsMenu.SetActive(true);
    }

    void UpdateOverlayState()
    {
        if (overlay == null)
        {
            // try to find overlay by common names
            overlay = GameObject.Find("overlay") ?? GameObject.Find("Overlay");
        }

        if (overlay == null) return;

        bool anyOpen = (gameInfoContainer != null && gameInfoContainer.activeSelf)
                       || (optionsMenu != null && optionsMenu.activeSelf)
                       || (settingsMenu != null && settingsMenu.activeSelf)
                       || (creditsMenu != null && creditsMenu.activeSelf);

        overlay.SetActive(anyOpen);
    }

    bool EnsureMenu(ref GameObject menuField, string menuName, string fieldLabel)
    {
        if (menuField != null) return true;
        if (!string.IsNullOrEmpty(menuName))
        {
            var go = GameObject.Find(menuName);
            if (go != null)
            {
                menuField = go;
                return true;
            }
        }

        Debug.LogWarning($"ButtonManager: {fieldLabel} is not assigned and could not be found by name ('{menuName}'). Please assign it in the inspector.");
        return false;
    }
}
