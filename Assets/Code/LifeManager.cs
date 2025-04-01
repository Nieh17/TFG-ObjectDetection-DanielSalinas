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
    private const string NextLifeTimeKey = "nextLifeTime";

    public Transform heartsContainer;
    public TextMeshProUGUI timerText;

    private List<GameObject> aliveHearts;
    private DateTime nextLifeTime;

    private void Start()
    {
        aliveHearts = new List<GameObject>();

        FindHearts();
        LoadLives();
        UpdateLivesUI();

        if (currentLives < maxLives)
        {
            StartCoroutine(RegenerateLives());
            StartCoroutine(UpdateTimerText());
        }
    }

    public void LoseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;

            if (currentLives < maxLives && nextLifeTime == DateTime.MinValue)
            {
                nextLifeTime = DateTime.Now.AddMinutes(regenTimeMinutes);
                PlayerPrefs.SetString(NextLifeTimeKey, nextLifeTime.ToBinary().ToString());
            }

            SaveLives();
            UpdateLivesUI();

            Debug.Log("Vida perdida. Próxima vida en: " + nextLifeTime.ToString("HH:mm:ss"));

            if (currentLives < maxLives)
            {
                StartCoroutine(RegenerateLives());
                StartCoroutine(UpdateTimerText());
            }
        }
    }

    private IEnumerator RegenerateLives()
    {
        while (currentLives < maxLives)
        {
            if (nextLifeTime == DateTime.MinValue) yield break;

            TimeSpan timeUntilNextLife = nextLifeTime - DateTime.Now;

            if (timeUntilNextLife.TotalSeconds <= 0)
            {
                currentLives++;
                if (currentLives < maxLives)
                {
                    nextLifeTime = DateTime.Now.AddMinutes(regenTimeMinutes);
                    PlayerPrefs.SetString(NextLifeTimeKey, nextLifeTime.ToBinary().ToString());
                }
                else
                {
                    nextLifeTime = DateTime.MinValue;
                    PlayerPrefs.DeleteKey(NextLifeTimeKey);
                }

                SaveLives();
                UpdateLivesUI();
            }

            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator UpdateTimerText()
    {
        while (currentLives < maxLives)
        {
            if (nextLifeTime == DateTime.MinValue)
            {
                timerText.gameObject.SetActive(false);
                yield break;
            }

            TimeSpan remainingTime = nextLifeTime - DateTime.Now;

            if (remainingTime.TotalSeconds > 0)
            {
                int minutes = Mathf.FloorToInt((float)remainingTime.TotalMinutes);
                int seconds = remainingTime.Seconds;

                timerText.text = $"Siguiente vida en: {minutes:D2}:{seconds:D2}";
                timerText.gameObject.SetActive(true);
            }
            else
            {
                timerText.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(1);
        }

        timerText.gameObject.SetActive(false);
    }

    private void LoadLives()
    {
        currentLives = PlayerPrefs.GetInt(LivesKey, maxLives);

        long nextLifeBinary = Convert.ToInt64(PlayerPrefs.GetString(NextLifeTimeKey, "0"));
        nextLifeTime = nextLifeBinary != 0 ? DateTime.FromBinary(nextLifeBinary) : DateTime.MinValue;

        DateTime now = DateTime.Now;
        while (currentLives < maxLives && nextLifeTime != DateTime.MinValue && now >= nextLifeTime)
        {
            currentLives++;
            nextLifeTime = currentLives < maxLives ? now.AddMinutes(regenTimeMinutes) : DateTime.MinValue;
        }

        SaveLives();
    }

    private void SaveLives()
    {
        PlayerPrefs.SetInt(LivesKey, currentLives);

        if (nextLifeTime != DateTime.MinValue)
        {
            PlayerPrefs.SetString(NextLifeTimeKey, nextLifeTime.ToBinary().ToString());
        }
        else
        {
            PlayerPrefs.DeleteKey(NextLifeTimeKey);
        }

        PlayerPrefs.Save();
    }

    private void UpdateLivesUI()
    {
        Debug.Log("Vidas actuales: " + currentLives);

        for (int i = 0; i < aliveHearts.Count; i++)
        {
            aliveHearts[i].SetActive(i < currentLives);
        }

        if (currentLives < maxLives)
        {
            StartCoroutine(UpdateTimerText());
        }
        else
        {
            timerText.gameObject.SetActive(false);
        }
    }

    void FindHearts()
    {
        aliveHearts.Clear();

        foreach (Transform child in heartsContainer)
        {
            if (child.CompareTag("AliveHeart"))
            {
                aliveHearts.Add(child.gameObject);
            }
        }

        aliveHearts.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
    }
}
