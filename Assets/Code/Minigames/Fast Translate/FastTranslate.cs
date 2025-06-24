using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class FastTranslate : GameBase
{
    // Atributos específicos de FastTranslate
    public Button[] optionButtons;

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


    [Header("Game's Canvas Texts")]
    [SerializeField] TMP_Text wordToTranslateText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text currentLivesText;

    [Header("Slider")]
    [SerializeField] Slider timerSlider;
    [SerializeField] Image timerFillImage;

    [Header("Time")]
    [SerializeField] float maxTime = 30f;

    private int maxWords = 5;

    private WordPair currentWord;
    public List<WordPair> availableWordPairs;
    private List<string> allTranslations = new List<string>();

    protected override void OnEnable()
    {
        base.OnEnable();
        if (scoreText) scoreText.text = "0";
        timer = maxTime;
        currentLivesText.text = LifeManager.instance.currentLives.ToString();
    }

    protected override void Update()
    {
        if (!gameActive) return;

        timer -= Time.deltaTime;
        timerText.text = Mathf.Ceil(timer).ToString();
        timerSlider.value = timer;

        UpdateTimerColor();

        if (timer <= 0)
        {
            EndGame();
        }
    }

    public override void StartGame()
    {
        base.StartGame();
    }

    protected override async void SetupGame()
    {
        string imageDirectory = Path.Combine(Application.persistentDataPath, "SavedImages");
        var preparedWordPairs = await WordPreparationService.PrepareWordQueueAsync(imageDirectory, maxWords);

        if (preparedWordPairs == null || preparedWordPairs.Count == 0)
        {
            Debug.LogWarning("No se encontraron palabras válidas.");
            return;
        }

        availableWordPairs = new List<WordPair>(preparedWordPairs);

        allTranslations.Clear();
        foreach (var pair in availableWordPairs)
        {
            allTranslations.Add(pair.translatedWord);
        }

        timerSlider.maxValue = maxTime;
        timerSlider.value = timer;

        NextRound();
    }

    void NextRound()
    {
        if (availableWordPairs.Count == 0 || !gameActive)
        {
            EndGame();
            return;
        }

        currentWord = availableWordPairs[Random.Range(0, availableWordPairs.Count)];
        if (wordToTranslateText) wordToTranslateText.text = currentWord.nativeWord;

        foreach (var button in optionButtons)
        {
            button.interactable = true;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            button.colors = colors;
        }

        List<string> options = new List<string> { currentWord.translatedWord };
        while (options.Count < optionButtons.Length)
        {
            string randomWord = allTranslations[Random.Range(0, allTranslations.Count)];
            if (!options.Contains(randomWord)) options.Add(randomWord);
        }

        options.Shuffle();

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].GetComponentInChildren<TMP_Text>().text = options[index];
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => CheckAnswer(optionButtons[index], options[index]));
        }
    }

    void CheckAnswer(Button button, string selectedWord)
    {
        if (!gameActive) return;

        totalTries += 1;

        if (selectedWord == currentWord.translatedWord)
        {
            correctTries += 1;
            score += 10;
            if (scoreText) scoreText.text = score.ToString();
            availableWordPairs.Remove(currentWord);
            NextRound();
        }
        else
        {
            score -= 5;
            if (scoreText) scoreText.text = score.ToString();
            if (timerSlider) timerSlider.value = timer;
            UpdateTimerColor();
            button.interactable = false;

            base.LoseLife(currentLivesText);

            if (LifeManager.instance.currentLives == 0) EndGame();

        }
    }

    void UpdateTimerColor()
    {
        float percentage = timer / maxTime;

        if (percentage > 0.5f)
        {
            timerFillImage.color = Color.Lerp(Color.yellow, Color.green, (percentage - 0.5f) * 2);
        }
        else
        {
            timerFillImage.color = Color.Lerp(Color.red, Color.yellow, percentage * 2);
        }
    }

    protected override void ResetGameSpecificUI(){}
}