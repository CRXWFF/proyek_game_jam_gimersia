using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WordValidator : MonoBehaviour
{
    public static WordValidator Instance { get; private set; }

    [Tooltip("Daftar kata valid (misal: KAKAK, KARAN, dsb). Case-insensitive.")]
    public List<string> validWords = new List<string>();

    HashSet<string> wordSet;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        wordSet = new HashSet<string>(
            validWords
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Select(w => w.Trim().ToUpperInvariant())
        );
    }

    public bool IsValid(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return false;
        return wordSet.Contains(word.ToUpperInvariant());
    }
}
