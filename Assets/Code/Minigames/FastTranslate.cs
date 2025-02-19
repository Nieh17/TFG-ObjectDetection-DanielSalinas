using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FastTranslate : MonoBehaviour
{
    [System.Serializable]
    public class WordPair
    {
        public string nativeWord;
        public string translatedWord;
    }

    public List<WordPair> wordPairs;
    public Button[] optionButtons;


    [Header("General Canvas")]
    [SerializeField] GameObject generalCanvas;

    [Header("Introduction")]
    [SerializeField] GameObject introCanvas;

    [Header("Game's Canvas")]
    [SerializeField] GameObject gameCanvas;

    [Header("Game's Canvas Texts")]
    [SerializeField] TMP_Text wordToTranslateText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text timerText;

    [Header("Slider")]
    [SerializeField] Slider timerSlider;
    [SerializeField] Image timerFillImage;

    [Header("Time")]
    [SerializeField] float maxTime = 30f;

    [Header("End Game's Canvas")]
    [SerializeField] GameObject endPanel;

    [Header("End Game's Canvas Texts")]
    [SerializeField] TMP_Text accuracyTitle;
    [SerializeField] TMP_Text accuracyText;
    [SerializeField] TMP_Text timeText;
    [SerializeField] TMP_Text xpText;

    private int score;
    private float timer;
    private float timeLeft;
    private int minutes;
    private int seconds;

    private WordPair currentWord;
    private List<WordPair> availableWordPairs;
    private List<string> allTranslations = new List<string>();
    private bool gameActive = false;

    private int totalTries;
    private int correctTries;
    private float accuracyRate;
    private int totalXp;
    

    void OnEnable()
    {
        score = 0;
        timer = maxTime;
        timeLeft = 0f;
        minutes = 0;
        seconds = 0;

        allTranslations = new List<string>();
        gameActive = false;

        totalTries = 0;
        correctTries = 0;
        accuracyRate = 0;
        totalXp = 0;

        endPanel.SetActive(false);
        generalCanvas.SetActive(true);
        introCanvas.SetActive(true);
        timer = maxTime;
    }

    void Update()
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

    public void StartGame()
    {
        introCanvas.SetActive(false);
        gameCanvas.SetActive(true);
        Setup();
    }

    void Setup()
    {
        gameActive = true;
        availableWordPairs = new List<WordPair>(wordPairs);

        allTranslations.Clear();
        foreach (var pair in wordPairs)
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
        wordToTranslateText.text = currentWord.nativeWord;

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

        if (selectedWord == currentWord.translatedWord)
        {
            correctTries += 1;
            score += 10;
            scoreText.text = score.ToString();
            availableWordPairs.Remove(currentWord);
            NextRound();
        }
        else
        {
            score -= 5; 
            scoreText.text = score.ToString();
            //timer -= 3;
            timerText.text = Mathf.Ceil(timer).ToString();
            timerSlider.value = timer;
            UpdateTimerColor();
            button.interactable = false;
        }

        totalTries += 1;
    }

    void UpdateTimerColor()
    {
        float percentage = timer / maxTime;

        if (percentage > 0.5f)
        {
            // Verde a amarillo
            timerFillImage.color = Color.Lerp(Color.yellow, Color.green, (percentage - 0.5f) * 2);
        }
        else
        {
            // Amarillo a rojo
            timerFillImage.color = Color.Lerp(Color.red, Color.yellow, percentage * 2);
        }
    }

    void EndGame()
    {
        endPanel.SetActive(true);

        timeLeft = maxTime - timer;
        minutes = Mathf.FloorToInt(timeLeft / 60);
        seconds = Mathf.FloorToInt(timeLeft % 60);

        accuracyRate = (correctTries / totalTries) * 100f;
        gameActive = false;
        totalXp = Mathf.CeilToInt(score / 10f);


        if (accuracyRate > 50) accuracyTitle.text = "Good";
        else accuracyTitle.text = "Bad";
        accuracyText.text = accuracyRate.ToString()+"%"; 
        timeText.text = minutes+":"+seconds.ToString("D2");
        xpText.text = totalXp.ToString()+"XP";

        Debug.Log("Juego terminado. Puntuación final: " + score);
    }

    public void returnToMainMenu(GameObject objectToActivate)
    {
        scoreText.text = "0";

        endPanel.SetActive(false);
        gameCanvas.SetActive(false);
        introCanvas.SetActive(true);
        generalCanvas.SetActive(false);

        gameObject.SetActive(false);

        objectToActivate.SetActive(true);
    }
}
