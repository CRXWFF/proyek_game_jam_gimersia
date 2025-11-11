using System.Collections.Generic;
using UnityEngine;

/*
Kartu spesial (Power Cards)
- Word Charm: Setiap pemain berhasil membuat kata dengan 3 kartu, maka akan mendapatkan pengganda poin tambahan 5x
- Lexicon Vault: Pemain mendapatkan slot kartu +1
- Grand Verse: Jika pemain membuat kata dengan 4 kartu, dapat pengganda poin tambahan 10x
- Fresh Muse: Pemain mendapatkan kesempatan assemble +2 setiap ronde
*/

public class SpecialAreaManager : MonoBehaviour
{
    public static SpecialAreaManager Instance { get; private set; }

    [Header("References")]
    public CardView cardPrefab;
    public Transform specialAreaParent;

    [Header("Generation")]
    public int minSpecial = 1;
    public int maxSpecial = 3;
    // if true, the special area will be populated with random specials at Start()
    // default false so player sees no special cards until they are purchased
    public bool spawnRandomOnStart = false;

    List<CardView> specialCards = new List<CardView>();
    List<CardView> armedPowers = new List<CardView>();
    List<Transform> slotTransforms = new List<Transform>();
    Dictionary<CardView, Transform> cardSlotMap = new Dictionary<CardView, Transform>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
        AutoCollectSlots();
        // Only generate random specials at start when explicitly enabled.
        // By default this is false so the special area remains empty until
        // the player purchases cards from the shop (AddSpecificPower).
        if (spawnRandomOnStart)
            GenerateRandomSpecials();
    }

    void AutoCollectSlots()
    {
        slotTransforms.Clear();
        if (specialAreaParent == null) return;

        // collect immediate children that are intended as slots
        for (int i = 0; i < specialAreaParent.childCount; i++)
        {
            var t = specialAreaParent.GetChild(i);
            // skip the manager object itself if present
            if (t == specialAreaParent) continue;
            slotTransforms.Add(t);
        }
    }

    public void GenerateRandomSpecials()
    {
        if (cardPrefab == null || specialAreaParent == null) return;

        AutoCollectSlots();

        int available = slotTransforms.Count;
        if (available == 0)
        {
            Debug.LogWarning("SpecialAreaManager: no slot transforms found under specialAreaParent.");
            return;
        }

        int count = Mathf.Clamp(Random.Range(minSpecial, maxSpecial + 1), 0, available);

        var values = System.Enum.GetValues(typeof(CardModel.PowerType));
        for (int i = 0; i < count; i++)
        {
            // pick a random power excluding None
            CardModel.PowerType choice;
            do
            {
                choice = (CardModel.PowerType)values.GetValue(Random.Range(0, values.Length));
            } while (choice == CardModel.PowerType.None);

            var model = new CardModel(choice);

            // find first empty slot
            Transform slot = null;
            foreach (var s in slotTransforms)
            {
                bool used = false;
                foreach (var kv in cardSlotMap)
                {
                    if (kv.Value == s) { used = true; break; }
                }
                if (!used) { slot = s; break; }
            }

            if (slot == null)
            {
                Debug.LogWarning("SpecialAreaManager: no free slot available for special card.");
                break;
            }

            var v = Instantiate(cardPrefab, slot);
            v.Setup(model);
            v.CurrentArea = CardView.CardArea.Hand;
            // reset transform so it aligns to slot
            v.transform.localPosition = Vector3.zero;
            v.transform.localScale = Vector3.one;

            specialCards.Add(v);
            cardSlotMap[v] = slot;
        }
    }

    // Add a specific power card into the first free slot. Returns true if added.
    public bool AddSpecificPower(CardModel.PowerType power)
    {
        if (cardPrefab == null || specialAreaParent == null) return false;

        AutoCollectSlots();

        // find first empty slot
        Transform slot = null;
        foreach (var s in slotTransforms)
        {
            bool used = false;
            foreach (var kv in cardSlotMap)
            {
                if (kv.Value == s) { used = true; break; }
            }
            if (!used) { slot = s; break; }
        }

        if (slot == null) return false;

        var model = new CardModel(power);
        var v = Instantiate(cardPrefab, slot);
        v.Setup(model);
        v.CurrentArea = CardView.CardArea.Hand;
        v.transform.localPosition = Vector3.zero;
        v.transform.localScale = Vector3.one;

        specialCards.Add(v);
        cardSlotMap[v] = slot;
        return true;
    }

    public void OnSpecialCardClicked(CardView card)
    {
        if (card == null) return;

        // If power is WordCharm or GrandVerse: arm it so it will auto-consume on matching assemble
        if (card.Model.power == CardModel.PowerType.WordCharm || card.Model.power == CardModel.PowerType.GrandVerse)
        {
            if (!armedPowers.Contains(card))
            {
                armedPowers.Add(card);
                // visual feedback
                card.transform.localScale = Vector3.one * 1.1f;
                Debug.Log($"Armed power: {card.Model.power}");
            }
            return;
        }

        // LexiconVault: consume immediately and add slot
        if (card.Model.power == CardModel.PowerType.LexiconVault)
        {
            if (PlayAreaManager.Instance != null)
                PlayAreaManager.Instance.AddSlot();

            ConsumeCard(card);
            Debug.Log("LexiconVault used: added slot");
            return;
        }

        // FreshMuse: grant extra assembles this round and consume
        if (card.Model.power == CardModel.PowerType.FreshMuse)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddExtraAssembles(2);
                Debug.Log("FreshMuse used: +2 assembles this round");
            }
            ConsumeCard(card);
            return;
        }
    }

    void ConsumeCard(CardView card)
    {
        if (card == null) return;
        specialCards.Remove(card);
        armedPowers.Remove(card);
        if (cardSlotMap.ContainsKey(card))
            cardSlotMap.Remove(card);
        Destroy(card.gameObject);
    }

    // Called by GameManager when a word is formed to check & consume armed multipliers
    public int TryConsumeMultiplierForCount(int cardCount)
    {
        // if 3 and WordCharm armed -> consume and return multiplier 5
        if (cardCount == 3)
        {
            var c = armedPowers.Find(x => x.Model.power == CardModel.PowerType.WordCharm);
            if (c != null)
            {
                ConsumeCard(c);
                return 5;
            }
        }

        if (cardCount == 4)
        {
            var c = armedPowers.Find(x => x.Model.power == CardModel.PowerType.GrandVerse);
            if (c != null)
            {
                ConsumeCard(c);
                return 10;
            }
        }

        return 1; // no multiplier
    }
}
