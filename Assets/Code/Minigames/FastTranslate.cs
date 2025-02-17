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

    public List<WordPair> wordPairs; // Lista de palabras y sus traducciones
    public Button[] optionButtons; // Botones de respuesta
    public TMP_Text wordToTranslateText; // Texto de la palabra a traducir
    public TMP_Text scoreText; // Texto de la puntuación
    public TMP_Text timerText; // Texto del tiempo restante
    public Slider timerSlider; // Slider de tiempo
    public Image timerFillImage; // Imagen del relleno del slider

    private int score = 0;
    private float timer = 30f; // Tiempo límite en segundos
    private float maxTime = 30f; // Tiempo máximo (para calcular porcentaje)
    private WordPair currentWord; // Palabra actual
    private List<string> allTranslations = new List<string>(); // Lista de todas las traducciones
    private bool gameActive = true;

    void Start()
    {
        allTranslations.Clear();
        foreach (var pair in wordPairs)
        {
            allTranslations.Add(pair.translatedWord);
        }

        // Configurar el slider de tiempo
        timerSlider.maxValue = maxTime;
        timerSlider.value = timer;

        NextRound();
    }

    void Update()
    {
        if (!gameActive) return;

        timer -= Time.deltaTime; // Reduce el tiempo en tiempo real
        timerText.text = Mathf.Ceil(timer).ToString(); // Mostrar tiempo en enteros
        timerSlider.value = timer; // Actualizar el slider

        UpdateTimerColor(); // Cambiar color según el tiempo restante

        if (timer <= 0)
        {
            EndGame();
        }
    }

    void NextRound()
    {
        if (wordPairs.Count == 0 || !gameActive)
        {
            EndGame();
            return;
        }

        currentWord = wordPairs[Random.Range(0, wordPairs.Count)];
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
            score += 10;
            scoreText.text = score.ToString();
            wordPairs.Remove(currentWord);
            NextRound();
        }
        else
        {
            score -= 5; 
            scoreText.text = score.ToString();
            timer -= 3;
            timerText.text = Mathf.Ceil(timer).ToString();
            timerSlider.value = timer;
            UpdateTimerColor();
            button.interactable = false;
        }
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
        gameActive = false;
        Debug.Log("Juego terminado. Puntuación final: " + score);
    }
}
