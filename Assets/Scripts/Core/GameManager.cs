using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
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
    public int targetScore;
    public int maxDiscards;
    public int discardsLeft;
    public string wordCreated;
    public int basePoint;
    public int baseMult;
    public int maxAssembles;
    public int assembleLeft;

    int currentScore;
    int extraAssemblesThisRound = 0;
    private float nextUpdateTime;

    public static GameManager Instance { get; private set; }

    [Header("Result Popup")]
    public GameObject resultPopup;
    public GameObject overlay;
    public TMP_Text RemainingAssembleValue;
    public TMP_Text EarnedThisRoundValue;
    public TMP_Text CoinText;
    [Header("Game Over")]
    public GameObject gameOverPopup;

    int lastGain = 0;
    int playerCoins = 0;

    public int PlayerCoins => playerCoins;


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
            // show game over popup once when conditions met
            ShowGameOverPopup();
            return;
        }
    }

    void ShowGameOverPopup()
    {
        // update UI message as well
        UpdateUI("Game Over");

        // ensure overlay exists
        if (overlay == null)
        {
            overlay = GameObject.Find("overlay") ?? GameObject.Find("Overlay");
        }

        if (overlay != null)
            overlay.SetActive(true);

        if (gameOverPopup != null)
        {
            if (!gameOverPopup.activeSelf)
                gameOverPopup.SetActive(true);
        }

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseGameOverPopup()
    {
        if (gameOverPopup != null)
            gameOverPopup.SetActive(false);

        if (overlay != null)
            overlay.SetActive(false);
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

        // initialize player coins (could be loaded from player profile later)
        playerCoins = 0;
        if (CoinText != null)
            CoinText.text = "$" + playerCoins.ToString();

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

        // store last gain for result popup
        lastGain = gain;

        assembleLeft -= 1;
        handManager.ReplacePlayedCards(cards);
        playArea.ClearSlotsOnly();

        wordCreated = "";
        basePoint = 0;
        baseMult = 0;

        msg += $"\nTotal Score: {currentScore}";

        if (currentScore >= targetScore)
        {
            msg += "\nTarget tercapai! (placeholder kemenangan).";
            // show result popup only when target reached and not game over
            ShowResultPopup();
        }

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

    // Show result popup and overlay when target is reached
    void ShowResultPopup()
    {
        if (resultPopup == null)
        {
            Debug.LogWarning("ResultPopup is not assigned in GameManager.");
            return;
        }

        // don't show multiple times
        if (resultPopup.activeSelf) return;

        // set overlay active if available
        if (overlay == null)
        {
            overlay = GameObject.Find("overlay") ?? GameObject.Find("Overlay");
        }

        if (overlay != null)
            overlay.SetActive(true);

        // update texts
        if (RemainingAssembleValue != null)
            RemainingAssembleValue.text = "$" + assembleLeft.ToString();

        if (EarnedThisRoundValue != null)
            EarnedThisRoundValue.text = "$" + lastGain.ToString();

        // calculate coin gain as RemainingAssembleValue + EarnedThisRoundValue and add to player's coins
        if (CoinText != null)
        {
            int remaining = assembleLeft;
            int earned = lastGain;
            int coinGain = remaining + earned;
            // Add the gained coins to the player's coin total so CoinText reflects actual owned coins
            ModifyPlayerCoins(coinGain);
        }

        resultPopup.SetActive(true);
    }

    // Close result popup and overlay
    public void CloseResultPopup()
    {
        if (resultPopup != null)
            resultPopup.SetActive(false);

        if (overlay != null)
            overlay.SetActive(false);
    }

    // Called from ResultPopUp Collect button
    public void OnCollectFromResult()
    {
        // close result popup
        CloseResultPopup();

        // open shop
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OpenShop();
            // keep overlay active
            if (overlay != null)
                overlay.SetActive(true);
        }
    }

    public void ModifyPlayerCoins(int delta)
    {
        playerCoins += delta;
        if (CoinText != null)
            CoinText.text = "$" + playerCoins.ToString();
    }

    public void HomeScreenButton()
    {
        SceneManager.LoadScene("HomeScreen");
    }

    public void ProcessAssemble(List<Card> cards, string word)
    {
        int cardPoints = cards.Sum(c => c.cardData.basePoint);

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
        int gain = cardPoints + combo;

        int powerMult = 1;
        if (SpecialAreaManager.Instance != null)
        {
            powerMult = SpecialAreaManager.Instance.TryConsumeMultiplierForCount(n);
        }

        gain = gain * powerMult;
        currentScore += gain;
        lastGain = gain;
        assembleLeft -= 1;

        // Note: AssembleManager handles card replacement differently, skipping handManager.ReplacePlayedCards

        string msg = $"{word} adalah kata! Kartu: {cardPoints} + Kombo: {baseForm} x{mult}";
        if (powerMult != 1) msg += $" + Power x{powerMult}";
        msg += $" = {gain}\nTotal Score: {currentScore}";

        if (currentScore >= targetScore)
        {
            msg += "\nTarget tercapai!";
            ShowResultPopup();
        }

        UpdateUI(msg);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
