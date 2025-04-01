using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    public int maxLives = 5;
    public int currentLives;
    public int regenTimeMinutes = 1;

    private const string LivesKey = "lives";
    private const string LifeLossTimesKey = "lifeLossTimes";

    public Transform heartsContainer;
    private List<GameObject> aliveHearts;
    private List<DateTime> lifeLossTimes = new List<DateTime>();

    private void Start()
    {
        aliveHearts = new List<GameObject>();

        FindHearts();
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
            lifeLossTimes.Add(DateTime.Now);

            SaveLives();
            UpdateLivesUI();

            Debug.Log("Vida perdida. Próxima vida en: " + DateTime.Now.AddMinutes(regenTimeMinutes).ToString("HH:mm:ss"));

            if (currentLives < maxLives)
            {
                StartCoroutine(RegenerateLives());
            }
        }
    }

    private IEnumerator RegenerateLives()
    {
        while (currentLives < maxLives && lifeLossTimes.Count > 0)
        {
            DateTime now = DateTime.Now;
            while (lifeLossTimes.Count > 0 && (now - lifeLossTimes[0]).TotalMinutes >= regenTimeMinutes)
            {
                currentLives++;
                lifeLossTimes.RemoveAt(0);
                SaveLives();
                UpdateLivesUI();
            }

            yield return new WaitForSeconds(1);
        }
    }

    private void LoadLives()
    {
        currentLives = PlayerPrefs.GetInt(LivesKey, maxLives);

        string lossTimesString = PlayerPrefs.GetString(LifeLossTimesKey, "");
        lifeLossTimes.Clear();

        if (!string.IsNullOrEmpty(lossTimesString))
        {
            string[] timeStrings = lossTimesString.Split('|');
            foreach (string timeString in timeStrings)
            {
                if (long.TryParse(timeString, out long binaryTime))
                {
                    lifeLossTimes.Add(DateTime.FromBinary(binaryTime));
                }
            }
        }

        DateTime now = DateTime.Now;
        int recoveredLives = 0;

        while (lifeLossTimes.Count > 0 && (now - lifeLossTimes[0]).TotalMinutes >= regenTimeMinutes)
        {
            lifeLossTimes.RemoveAt(0);
            recoveredLives++;
        }

        currentLives = Mathf.Min(currentLives + recoveredLives, maxLives);

        SaveLives();
    }

    private void SaveLives()
    {
        PlayerPrefs.SetInt(LivesKey, currentLives);

        string lossTimesString = string.Join("|", lifeLossTimes.ConvertAll(time => time.ToBinary().ToString()));
        PlayerPrefs.SetString(LifeLossTimesKey, lossTimesString);

        PlayerPrefs.Save();
    }

    private void UpdateLivesUI()
    {
        Debug.Log("Vidas actuales: " + currentLives);

        for (int i = 0; i < aliveHearts.Count; i++)
        {
            aliveHearts[i].SetActive(i < currentLives);
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
