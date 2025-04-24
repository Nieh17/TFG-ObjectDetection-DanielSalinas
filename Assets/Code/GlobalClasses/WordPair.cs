[System.Serializable]
public class WordPair
{
    public string nativeWord;
    public string translatedWord;

    public WordPair(string native, string translated)
    {
        nativeWord = native;
        translatedWord = translated;
    }
}