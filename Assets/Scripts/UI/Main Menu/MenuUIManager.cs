using UnityEngine;

public class MenuUIManager : MonoBehaviour
{
[Header("Panels")]
    public GameObject overlay;
    public GameObject optionsMenu;
    public GameObject settingsMenu;
    public GameObject creditsMenu;

    // === OPTIONS ===
    public void OpenOptions()
    {
        overlay.SetActive(true);
        optionsMenu.SetActive(true);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }

    public void CloseOptions()
    {
        overlay.SetActive(false);
        optionsMenu.SetActive(false);
    }

    // === SETTINGS ===
    public void OpenSettings()
    {
        optionsMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    // === CREDITS ===
    public void OpenCredits()
    {
        optionsMenu.SetActive(false);
        creditsMenu.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    // === QUIT ===
    public void QuitGame()
    {
        Application.Quit();
    }
}
