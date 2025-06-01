using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class SyllableGameTwo : GameBase
{
    [Header("General Canvas")]
    [SerializeField] private GameObject generalCanvas;
    protected override GameObject GeneralCanvas => generalCanvas;

    [Header("Introduction")]
    [SerializeField] private GameObject introCanvas;
    protected override GameObject IntroCanvas => introCanvas;

    [Header("Game's Canvas")]
    [SerializeField] private GameObject gameCanvas;
    protected override GameObject GameCanvas => gameCanvas;

    [Header("End Game's Canvas")]
    [SerializeField] private GameObject endPanel;
    protected override GameObject EndPanel => endPanel;

    private Queue<WordPair> wordsToPlay;
    private WordPair currentWord;

    private int maxWords = 5;

    public GameObject syllablePrefab;
    public GameObject dropSlotPrefab;
    public Transform syllableSpawnZone;
    public Transform dropSlotZone;

    private float spacing = 250f;
    private float lineHeight = 200f;

    private RectTransform syllableSpawnZoneRect;
    private RectTransform dropSlotZoneRect;

    private List<DropSlot> dropSlots = new List<DropSlot>();

    [Header("Game's Objects")]
    [SerializeField] TMP_Text nativeWordText;
    [SerializeField] TMP_Text currentLivesText;

    private void Awake()
    {
        ClearPreviousElements();

        syllableSpawnZoneRect = syllableSpawnZone.GetComponent<RectTransform>();
        dropSlotZoneRect = dropSlotZone.GetComponent<RectTransform>();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        DropSlot.OnSyllablePlacedCorrectly += CheckWinCondition;
        DraggableSyllable.OnSyllablePlacedUncorrectly += AddTotalTries;

        Debug.Log(syllableSpawnZone != null);
    }

    private void OnDisable()
    {
        DropSlot.OnSyllablePlacedCorrectly -= CheckWinCondition;
        DraggableSyllable.OnSyllablePlacedUncorrectly -= AddTotalTries;
    }

    private void Start(){}

    protected override async void SetupGame()
    {
        bool isReady = await PrepareWordQueue();

        if (isReady) LoadNextWord();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void StartGame()
    {
        base.StartGame();
    }

    private async Task<bool> PrepareWordQueue()
    {
        string imageDirectory = Path.Combine(Application.persistentDataPath, "SavedImages");
        var wordPairs = await WordPreparationService.PrepareWordQueueAsync(imageDirectory, maxWords);

        if (wordPairs == null || wordPairs.Count == 0)
        {
            return false;
        }

        wordsToPlay = new Queue<WordPair>(wordPairs);

        return true;
    }

    private void LoadNextWord()
    {
        if (wordsToPlay.Count == 0)
        {
            EndGame();
            return;
        }

        currentWord = wordsToPlay.Dequeue();
        if (nativeWordText) nativeWordText.text = currentWord.nativeWord;

        GenerateGame(currentWord.translatedWord);
    }

    private void GenerateGame(string wordToSplit)
    {
        gameActive = true;

        List<string> syllables = GetSyllables(wordToSplit);

        List<string> shuffledSyllables = syllables.OrderBy(s => Random.value).ToList();

        ClearPreviousElements();

        GenerateSyllables(shuffledSyllables, syllableSpawnZone);

        GenerateDropSlots(syllables, dropSlotZone);
    }

    private void GenerateSyllables(List<string> syllables, Transform parent)
    {
        GenerateAndPositionElements(syllables, parent, syllablePrefab, (syllableObj, syllableText) =>
        {
            syllableObj.GetComponent<DraggableSyllable>().SetText(syllableText);
        }, syllableSpawnZoneRect);
    }

    private void GenerateDropSlots(List<string> syllables, Transform parent)
    {
        dropSlots.Clear();

        GenerateAndPositionElements(syllables, parent, dropSlotPrefab, (dropSlotObj, expectedSyllable) =>
        {
            DropSlot slotComponent = dropSlotObj.GetComponent<DropSlot>();
            slotComponent.expectedSyllable = expectedSyllable;
            dropSlots.Add(slotComponent);
        }, dropSlotZoneRect);
    }

    private void GenerateAndPositionElements(List<string> elementsData, Transform parent, GameObject prefab, Action<GameObject, string> onElementCreated, RectTransform zoneRectTransform)
    {
        Debug.Log($"Generating elements. Parent: {parent?.name}, Prefab: {prefab?.name}, ZoneRectTransform: {zoneRectTransform?.name}");

        float currentX = 0f;
        float currentY = 0f;
        int elementsPerLine = (spacing > 0) ? Mathf.FloorToInt(zoneRectTransform.rect.width / spacing) : 1;
        if (elementsPerLine == 0) elementsPerLine = 1;

        for (int i = 0; i < elementsData.Count; i++)
        {
            if (currentX + spacing > zoneRectTransform.rect.width)
            {
                currentX = 0f;
                currentY -= lineHeight;
            }

            GameObject newObj = Instantiate(prefab, parent);
            newObj.GetComponent<RectTransform>().localPosition = new Vector3(currentX, currentY, 0);

            onElementCreated?.Invoke(newObj, elementsData[i]);

            currentX += spacing;
        }
    }

    public List<string> GetSyllables(string word)
    {
        List<string> syllables = new List<string>();

        string language = PlayerPrefs.GetString(SelectedLanguageKey, ENGLISH);

        string pattern = "";
        switch (language)
        {
            case JAPANESE:
                pattern = @"(kya|kyu|kyo|gya|gyu|gyo|sha|shu|sho|ja|ju|jo|cha|chu|cho|nya|nyu|nyo|hya|hyu|hyo|"
                        + @"bya|byu|byo|pya|pyu|pyo|mya|myu|myo|rya|ryu|ryo|"
                        + @"kk|ss|tt|pp|"
                        + @"ba|bi|bu|be|bo|ca|chi|da|de|do|fa|fi|fu|fe|fo|ga|gi|gu|ge|go|"
                        + @"ha|hi|fu|he|ho|ka|ki|ku|ke|ko|ma|mi|mu|me|mo|"
                        + @"na|ni|nu|ne|no|pa|pi|pu|pe|po|ra|ri|ru|re|ro|"
                        + @"sa|shi|su|se|so|ta|te|to|wa|wo|ya|yu|yo|za|ji|zu|ze|zo|n|"
                        + @"[aiueo])";
                break;

            case SPANISH:
                pattern = @"([^aeiou]*[aeiou]+(?:[mnrls]?))";
                break;

            default: // English
                pattern = @"[^aeiouy]*[aeiouy]+(?:[^aeiouy]*$|[^aeiouy](?=[^aeiouy]))?";
                break;
        }

        MatchCollection matches = Regex.Matches(word.ToLower(), pattern);

        foreach (Match match in matches)
        {
            syllables.Add(match.Value);
        }

        if (syllables.Count <= 1)
        {
            return new List<string> { word };
        }

        return syllables;
    }

    private void ClearPreviousElements()
    {
        foreach (Transform child in syllableSpawnZone) Destroy(child.gameObject);
        foreach (Transform child in dropSlotZone) Destroy(child.gameObject);
    }

    private void CheckWinCondition()
    {
        correctTries += 1;
        score += 10;
        totalTries += 1;

        foreach (DropSlot slot in dropSlots)
        {
            if (!slot.IsCorrectlyFilled())
            {
                return;
            }
        }

        LoadNextWord();
    }

    private void AddTotalTries()
    {
        totalTries += 1;
        score -= 5;

        base.LoseLife(currentLivesText);
    }

    protected override void ResetGameSpecificUI()
    {
        ClearPreviousElements();
    }
}