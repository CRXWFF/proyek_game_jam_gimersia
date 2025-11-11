using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public DeckManager deckManager;
    public HandManager handManager;
    public PlayAreaManager playArea;
    public UIHUD ui;

    [Header("Config")]
    public int targetScore = 200;
    public int maxDiscards = 3;
    public int discardsLeft;
    public string wordCreated;
    public int basePoint;
    public int baseMult;
    public int maxAssembles = 4;
    public int assembleLeft;

    int currentScore;
    int extraAssemblesThisRound = 0;
    private float nextUpdateTime;

    public static GameManager Instance { get; private set; }

    [System.Obsolete]
    private void Start()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;

        if (deckManager == null) deckManager = DeckManager.Instance;
        if (handManager == null) handManager = FindObjectOfType<HandManager>();
        if (playArea == null) playArea = FindObjectOfType<PlayAreaManager>();
        if (ui == null) ui = FindObjectOfType<UIHUD>();

        StartLevel();
    }

    private void Update()
    {

        if (assembleLeft == 0 && currentScore < targetScore)
        {
            UpdateUI("Game Over");
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
            return;
        }
    }

    public void AddExtraAssembles(int n)
    {
        extraAssemblesThisRound += n;
        Debug.Log($"Extra assembles added: {n}. Total this round: {extraAssemblesThisRound}");
    }

    public void StartLevel()
    {
        currentScore = 0;

        if (deckManager != null)
            deckManager.BuildAndShuffle();

        if (playArea != null)
            playArea.ClearSlotsOnly();

        if (handManager != null)
        {
            handManager.deck = deckManager;
            handManager.playArea = playArea;
            handManager.InitializeHand();
        }
        discardsLeft = maxDiscards;
        assembleLeft = maxAssembles;

        UpdateUI("Susun kartu di area tengah untuk membentuk kata.");
    }

    public void OnPlayHandButton()
    {
        var cards = playArea.GetPlayedCardsInOrder();

        if (cards.Count == 0)
        {
            UpdateUI("Belum ada kartu di area susun.");
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        // bentuk kata
        string word = string.Concat(cards.Select(c => c.Model.text));
        int cardPoints = cards.Sum(c => c.Model.basePoints);

        bool isWord = WordValidator.Instance != null && WordValidator.Instance.IsValid(word);

        int gain;
        string msg;

        if (!isWord)
        {
            gain = cardPoints;
            msg = $"{word} bukan kata valid. Skor: {gain} (dari poin kartu).";
            wordCreated = "Bukan Kata";
        }
        else
        {
            int n = cards.Count;
            int baseForm = 0;
            int mult = 1;

            switch (n)
            {
                case 1: baseForm = 10; mult = 1; break;
                case 2: baseForm = 20; mult = 2; break;
                case 3: baseForm = 40; mult = 3; break;
                case 4: baseForm = 60; mult = 4; break;
                case 5: baseForm = 80; mult = 5; break;
            }

            int combo = baseForm * mult;
            gain = cardPoints + combo;
            // check for special multipliers armed in special area
            int powerMult = 1;
            if (SpecialAreaManager.Instance != null)
            {
                powerMult = SpecialAreaManager.Instance.TryConsumeMultiplierForCount(n);
            }

            gain = gain * powerMult;
            msg = $"{word} adalah kata! Kartu: {cardPoints} + Kombo: {baseForm} x{mult}";
            if (powerMult != 1) msg += $" + Power x{powerMult}";
            msg += $" = {gain}";
        }

        currentScore += gain;

        assembleLeft -= 1;
        handManager.ReplacePlayedCards(cards);
        playArea.ClearSlotsOnly();

        wordCreated = "";
        basePoint = 0;
        baseMult = 0;

        msg += $"\nTotal Score: {currentScore}";

        if (currentScore >= targetScore)
            msg += "\nTarget tercapai! (placeholder kemenangan).";

        UpdateUI(msg);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnDiscardButton()
    {
        var cards = playArea.GetPlayedCardsInOrder();
        if (cards.Count == 0 || discardsLeft == 0)
        {
            UpdateUI("Tidak ada kartu di area susun untuk dibuang.");
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        discardsLeft -= 1;
        handManager.ReplacePlayedCards(cards);
        playArea.ClearSlotsOnly();

        wordCreated = "";
        basePoint = 0;
        baseMult = 0;

        UpdateUI("Kartu di area susun dibuang dan diganti.");
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    void UpdateUI(string msg)
    {
        if (ui != null)
        {
            ui.SetScore(currentScore);
            ui.SetTarget(targetScore);
            ui.setword(wordCreated);
            ui.SetDiscardsleft(discardsLeft);
            ui.SetBasePoint(basePoint);
            ui.SetBaseMult(baseMult);
            ui.SetAssemblesLeft(assembleLeft);
            ui.SetLastHand(msg);
        }
        else
        {
            Debug.Log(msg);
        }
    }

    public void HomeScreenButton()
    {
        SceneManager.LoadScene("HomeScreen");
    }

    public void UpdateWordPreview()
    {
        if (Time.time < nextUpdateTime) return;
        nextUpdateTime = Time.time + 0.1f;
    
        var cards = playArea.GetPlayedCardsInOrder();

        if (cards.Count == 0)
        {
            wordCreated = "";
            basePoint = 0;
            baseMult = 0;
            UpdateUI("Susun kartu untuk membentuk kata.");
            return;
        }

        // bentuk kata sementara
        string word = string.Concat(cards.Select(c => c.Model.text));

        bool isWord = WordValidator.Instance != null && WordValidator.Instance.IsValid(word);

        int n = cards.Count;
        int baseForm = 0;
        int mult = 1;

        if (!isWord)
        {
            n = 1;
        }

        switch (n)
        {
            case 1: baseForm = 10; mult = 1; break;
            case 2: baseForm = 20; mult = 2; break;
            case 3: baseForm = 40; mult = 3; break;
            case 4: baseForm = 60; mult = 4; break;
            case 5: baseForm = 80; mult = 5; break;
        }

        wordCreated = isWord ? word : "Bukan Kata";
        basePoint = baseForm;
        baseMult = mult;

        string msg = isWord ? $"Preview: {word} (x{mult})" : $"{word} bukan kata valid.";
        UpdateUI(msg);
    }
}
