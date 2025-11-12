using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WordValidator : MonoBehaviour
{
    public static WordValidator Instance { get; private set; }

    [Tooltip("Daftar kata valid (misal: KAKAK, KARAN, dsb). Case-insensitive.")]
    public List<string> validWords = new List<string>();

    // optional lists of syllables (cards) used in the game rules. Exposed for reference.
    public string[] oneLetterSyllables = new[] { "A", "I", "U", "E", "O", "N" };
    public string[] twoLetterSyllables = new[] {
        "BA","KA","LA","MA","SA","TA","PA","GA","RA","DA","NA","NI","LU","LI","MO","MI","SU","SI","TU","KE"
    };
    public string[] threeLetterSyllables = new[] {
        "RAN","LAH","NGA","NYA","MAN","KAN","ANG","ING","RAH","RUS"
    };

    HashSet<string> wordSet;

    // builtin valid words generated from the game's syllable combinations (automatically populated)
    static readonly string[] builtinWords = new[] {
        // 2-syllable
        "AMAN","ANGSA","BABA","BALA","BALU","BARA","BARUS","BASA","BASI","BATA","BATU",
        "DADA","DANA","DARA","DARAH","DASA","DATU","IKAN","IMAN","INI","KAKA","KALA","KALAH",
        "KAMI","KANDA","KARA","KASA","KASI","KATA","KENA","KERA","LABA","LAGA","LAMA","LALU",
        "LARA","LIMA","LUKA","LUPA","MAKAN","MALA","MALAH","MAMA","MANA","MARAH","MASA","MATA",
        "MAU","NADA","NAGA","NAMA","NASI","NILA","NYALA","NYATA","OLAH","PADA","PALA","PAPA",
        "PARA","PARAH","RAGA","RANDA","RASA","RATU","RUSA","SALAH","SAMA","SANA","SAPA","SARA",
        "SATU","SILA","SINI","SISA","SISI","SUKA","SUSU","TALI","TAMAN","TUA","TULI","URUS", "SARAN", "KEPAL", "GALI",

        // 3-syllable
        "KELAPA","KEPALA","MAKALAH","MASAKAN","MASUKAN","PASARAN","PASUKAN","RASAKAN","SALAMAN",
        "SALURAN","SARANA","SUARA","UDARA", "KEPALAN", "GALIAN", "DALAMAN",

        // 4-syllable
        "KEPADANYA","SAMASAMA",

        // 5-syllable
        "SALURANNYA","KEPALABATU"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Ensure inspector list follows builtin words so designers don't have to fill it manually.
        validWords = new List<string>(builtinWords);

        // Build the word set from validWords (which now contains the builtin list)
        wordSet = new HashSet<string>(
            validWords
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Select(w => w.Trim().ToUpperInvariant())
        );
    }

    // Keep the inspector `validWords` in sync with builtinWords while editing in the Editor
    private void OnValidate()
    {
        // copy builtinWords into the inspector-exposed list so editors see the default words
        if (builtinWords != null && builtinWords.Length > 0)
        {
            validWords = new List<string>(builtinWords);
        }
    }

    public bool IsValid(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return false;
        return wordSet.Contains(word.ToUpperInvariant());
    }
}
