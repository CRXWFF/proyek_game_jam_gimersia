using UnityEngine;

public class CreditMenuController : MonoBehaviour
{
    [Header("Arrows")]
    public GameObject arrowOurTeam;
    public GameObject arrowAudio;
    public GameObject arrowFonts;

    [Header("Text Containers")]
    public GameObject ourTeamContainer;
    public GameObject audioContainer;
    public GameObject fontsContainer;

    private void OnEnable()
    {
        // Ketika Credit Menu dibuka, default aktif "Our Team"
        ShowOurTeam();
    }

    public void ShowOurTeam()
    {
        SetActiveSection(arrowOurTeam, ourTeamContainer);
    }

    public void ShowAudio()
    {
        SetActiveSection(arrowAudio, audioContainer);
    }

    public void ShowFonts()
    {
        SetActiveSection(arrowFonts, fontsContainer);
    }

    private void SetActiveSection(GameObject activeArrow, GameObject activeContainer)
    {
        // Nonaktifkan semua arrow
        arrowOurTeam.SetActive(false);
        arrowAudio.SetActive(false);
        arrowFonts.SetActive(false);

        // Nonaktifkan semua container
        ourTeamContainer.SetActive(false);
        audioContainer.SetActive(false);
        fontsContainer.SetActive(false);

        // Aktifkan yang dipilih
        activeArrow.SetActive(true);
        activeContainer.SetActive(true);
    }
}
