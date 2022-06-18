using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class WordsHandler:MonoBehaviour
{
    private const string WORDSFILEPATH = "Data/FiveLetterWords";

    public static List<string> FiveLetterWords { get; private set; }
    public void Awake()
    {
        var wordsTextAsset = Resources.Load<TextAsset>(WORDSFILEPATH);
        FiveLetterWords = wordsTextAsset.text.Split('\n', '\r').Where(word=>word.Length==5).ToList();
    }
}
