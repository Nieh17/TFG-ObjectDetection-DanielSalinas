using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    public int maxLives = 5;
    public int currentLives;
    public int regenTimeMinutes = 1;

    private const string LivesKey = "lives";
    private const string LastLifeLostTimeKey = "lastLifeLostTime";

    private void Start()
    {
        LoadLives();
        UpdateLivesUI();

        if (currentLives < maxLives)
        {
            StartCoroutine(RegenerateLives());
        }
    }

    public void LoseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;
            DateTime nextLifeTime = DateTime.Now.AddMinutes(regenTimeMinutes);

            PlayerPrefs.SetInt(LivesKey, currentLives);
            PlayerPrefs.SetString(LastLifeLostTimeKey, DateTime.Now.ToBinary().ToString());
            PlayerPrefs.Save();
            UpdateLivesUI();

            Debug.Log("Vida perdida. Próxima vida en: " + nextLifeTime.ToString("HH:mm:ss"));


            if (currentLives < maxLives)
            {
                StartCoroutine(RegenerateLives());
            }
        }
    }

    private IEnumerator RegenerateLives()
    {
        while (currentLives < maxLives)
        {
            long lastLifeLostTime = Convert.ToInt64(PlayerPrefs.GetString(LastLifeLostTimeKey, "0"));
            DateTime lastLifeLost = DateTime.FromBinary(lastLifeLostTime);
            TimeSpan timePassed = DateTime.Now - lastLifeLost;

            int livesRecovered = (int)(timePassed.TotalMinutes / regenTimeMinutes);

            if (livesRecovered > 0)
            {
                currentLives = Mathf.Min(currentLives + livesRecovered, maxLives);
                PlayerPrefs.SetInt(LivesKey, currentLives);
                PlayerPrefs.Save();
                UpdateLivesUI();
            }

            yield return new WaitUntil(() => (DateTime.Now - lastLifeLost).TotalMinutes >= regenTimeMinutes);
        }
    }

    private void LoadLives()
    {
        currentLives = PlayerPrefs.GetInt(LivesKey, maxLives);

        long lastLifeLostTime = Convert.ToInt64(PlayerPrefs.GetString(LastLifeLostTimeKey, "0"));
        DateTime lastLifeLost = DateTime.FromBinary(lastLifeLostTime);
        TimeSpan timePassed = DateTime.Now - lastLifeLost;

        int livesRecovered = (int)(timePassed.TotalMinutes / regenTimeMinutes);
        if (livesRecovered > 0)
        {
            currentLives = Mathf.Min(currentLives + livesRecovered, maxLives);
            PlayerPrefs.SetInt(LivesKey, currentLives);
            PlayerPrefs.Save();
        }
    }

    private void UpdateLivesUI()
    {
        Debug.Log("Vidas actuales: " + currentLives);
    }
}
