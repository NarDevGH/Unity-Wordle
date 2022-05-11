using System.Collections.Generic;
using System.IO;
using System.Linq;

public class WordsHandler
{
    private const string WORDSFILEPATH = "Assets/Resources/Data/FiveLetterWords.txt";

    public static List<string> words { get; private set; }
    public WordsHandler()
    {
        words = File.ReadAllLines(WORDSFILEPATH).ToList();
    }
}
