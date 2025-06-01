using UnityEngine;
using TMPro;
using System.IO;
using System;
using System.Collections.Generic;

public abstract class GameBase : MonoBehaviour
{
    protected abstract GameObject GeneralCanvas { get; }
    protected abstract GameObject IntroCanvas { get; }
    protected abstract GameObject GameCanvas { get; }
    protected abstract GameObject EndPanel { get; }

    [SerializeField] protected TMP_Text accuracyTitle;
    [SerializeField] protected TMP_Text accuracyText;
    [SerializeField] protected TMP_Text timeText;
    [SerializeField] protected TMP_Text xpText;

    protected bool gameActive = false;
    protected float timer;
    protected int minutes;
    protected int seconds;

    protected int totalTries;
    protected int correctTries;
    protected float accuracyRate;
    protected int score;
    protected int totalXp;

    protected const string SelectedLanguageKey = "SelectedLanguage";
    protected const string JAPANESE = "japanese";
    protected const string SPANISH = "spanish";
    protected const string ENGLISH = "english";

    protected virtual void OnEnable()
    {
        ResetGameStats();
        bool skipIntro = false;
        if (PlayerPrefs.HasKey("SkipIntro"))
        {
            skipIntro = PlayerPrefs.GetInt("SkipIntro") == 1;
        }
        if (GeneralCanvas) GeneralCanvas.SetActive(true);
        if (IntroCanvas && !skipIntro) IntroCanvas.SetActive(true);
        
        if (GameCanvas && !skipIntro) GameCanvas.SetActive(false);
        if (GameCanvas && skipIntro)
        {
            GameCanvas.SetActive(true);
            StartGame();
        }

        if (EndPanel) EndPanel.SetActive(false);
    }

    protected virtual void Update()
    {
        if (!gameActive) return;
        timer += Time.deltaTime;
    }

    protected void ResetGameStats()
    {
        timer = 0f;
        minutes = 0;
        seconds = 0;
        totalTries = 0;
        correctTries = 0;
        accuracyRate = 0f;
        score = 0;
        totalXp = 0;
    }

    public virtual void StartGame()
    {
        IntroCanvas.SetActive(false);
        GameCanvas.SetActive(true);
        gameActive = true;
        SetupGame();
    }

    protected abstract void SetupGame();

    protected void EndGame()
    {
        gameActive = false;
        if (EndPanel) EndPanel.SetActive(true);

        minutes = Mathf.FloorToInt(timer / 60);
        seconds = Mathf.FloorToInt(timer % 60);

        var unmultiplied = (float)correctTries / totalTries;
        accuracyRate = Mathf.RoundToInt(unmultiplied * 100);

        totalXp = Mathf.CeilToInt(score / 10f);
        if (totalXp < 0) totalXp = 1;

        UpdateEndGameUI();
    }

    protected virtual void UpdateEndGameUI()
    {
        if (accuracyTitle) accuracyTitle.text = (accuracyRate > 50) ? "Good" : "Bad";
        if (accuracyText) accuracyText.text = accuracyRate.ToString() + "%";
        if (timeText) timeText.text = minutes + ":" + seconds.ToString("D2");
        if (xpText) xpText.text = totalXp.ToString() + "XP";
    }

    public void ReturnToMainMenu(GameObject objectToActivate)
    {
        LevelManager.Instance.AddXP(totalXp);

        ResetGameSpecificUI();

        if (EndPanel) EndPanel.SetActive(false);
        if (GameCanvas) GameCanvas.SetActive(false);
        if (IntroCanvas) IntroCanvas.SetActive(true);
        if (GeneralCanvas) GeneralCanvas.SetActive(false);

        gameObject.SetActive(false);

        if (objectToActivate) objectToActivate.SetActive(true);
    }

    protected abstract void ResetGameSpecificUI();

    public void LoseLife(TMP_Text currentLivesText)
    {
        LifeManager.instance.LoseLife();
        if (currentLivesText) currentLivesText.text = LifeManager.instance.currentLives.ToString();
    }
}

public static class ListExtensions
{
    private static System.Random rng = new System.Random();
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}