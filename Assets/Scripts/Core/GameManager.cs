using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public HandManager handManager;
    public UIHUD ui;
    public ScorePopup scorePopupPrefab;
    public Transform scorePopupParent;

    [Header("Config")]
    public int targetScore = 300;
    public int handsPerRound = 8;
    public int discardsPerRound = 3;

    int currentScore;
    int handsLeft;
    int discardsLeft;
    bool isGameOver;

    [System.Obsolete]
    void Start()
    {
        StartNewRound();
    }

    [System.Obsolete]
    public void StartNewRound()
    {
        isGameOver = false;

        currentScore = 0;
        handsLeft = handsPerRound;
        discardsLeft = discardsPerRound;

        if (handManager == null)
            handManager = FindObjectOfType<HandManager>();

        if (handManager == null)
        {
            Debug.LogError("GameManager: HandManager belum di-assign.");
            return;
        }

        if (DeckManager.Instance == null)
        {
            Debug.LogError("GameManager: DeckManager tidak ditemukan di scene.");
            return;
        }

        DeckManager.Instance.BuildAndShuffle();
        handManager.InitializeHand();

        UpdateHUD("Mulai round baru.");
    }

    public void OnPlayHandButton()
    {
        if (isGameOver) return;
        if (handsLeft <= 0)
        {
            UpdateHUD("Tidak ada hand tersisa.");
            return;
        }

        var selected = handManager.GetSelectedCards();
        if (selected.Count != 5)
        {
            UpdateHUD("Pilih tepat 5 kartu untuk dimainkan.");
            return;
        }

        var models = selected.Select(c => c.Model).ToList();
        var (rank, score) = PokerHandEvaluator.Evaluate(models);

        handsLeft--;
        currentScore += score;

        // hapus kartu yang dimainkan, refill
        handManager.RemoveCards(selected);
        handManager.RefillHand();

        string msg = $"{rank} (+{score})";
        ShowScorePopup(msg);

        // cek kondisi
        if (currentScore >= targetScore)
        {
            msg += "\nRound Clear!";
            isGameOver = true;
        }
        else if (handsLeft <= 0)
        {
            msg += "\nGame Over";
            isGameOver = true;
        }

        UpdateHUD(msg);
    }

    public void OnDiscardButton()
    {
        if (isGameOver) return;

        if (discardsLeft <= 0)
        {
            UpdateHUD("Discard sudah habis.");
            return;
        }

        var selected = handManager.GetSelectedCards();
        if (selected.Count == 0)
        {
            UpdateHUD("Pilih kartu yang ingin di-discard.");
            return;
        }

        discardsLeft--;

        handManager.RemoveCards(selected);
        handManager.RefillHand();

        UpdateHUD($"Discard {selected.Count} kartu.");
    }

    void UpdateHUD(string lastHandMessage)
    {
        if (ui != null)
        {
            ui.SetScore(currentScore);
            ui.SetHands(handsLeft);
            ui.SetDiscards(discardsLeft);
            ui.SetTarget(targetScore);
            ui.SetLastHand(lastHandMessage);
        }
        else
        {
            Debug.Log(lastHandMessage);
        }
    }

    void ShowScorePopup(string text)
    {
        if (scorePopupPrefab == null || scorePopupParent == null) return;

        var popup = Instantiate(scorePopupPrefab, scorePopupParent);
        popup.Show(text);
    }
}
