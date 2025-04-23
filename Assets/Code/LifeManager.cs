using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    public static LifeManager instance { get; private set; }

    public int maxLives = 5;
    public int currentLives;
    public int regenTimeMinutes = 1;

    private const string LivesKey = "lives";
    private const string NextLivesKey = "nextLives";

    public Transform heartsContainer;
    public TextMeshProUGUI timerStringText;
    public TextMeshProUGUI timerNumberText;

    private List<GameObject> aliveHearts;
    private Queue<DateTime> nextLifeTimes;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
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

            // Calcular la nueva regeneración correctamente
            DateTime lastRegenTime = nextLifeTimes.Count > 0 ? nextLifeTimes.Peek() : DateTime.Now;
            DateTime newRegenTime = lastRegenTime.AddMinutes(regenTimeMinutes);
            nextLifeTimes.Enqueue(newRegenTime);

            SaveLives();
            UpdateLivesUI();

            Debug.Log("Vida perdida. Próxima vida en: " + newRegenTime.ToString("HH:mm:ss"));

            if (currentLives < maxLives)
            {
                StartCoroutine(RegenerateLives());
                StartCoroutine(UpdateTimerText());
            }
        }
    }

    private IEnumerator RegenerateLives()
    {
        while (currentLives < maxLives && nextLifeTimes.Count > 0)
        {
            DateTime nextLifeTime = nextLifeTimes.Peek();
            TimeSpan timeUntilNextLife = nextLifeTime - DateTime.Now;

            if (timeUntilNextLife.TotalSeconds <= 0)
            {
                currentLives++;
                nextLifeTimes.Dequeue(); // Eliminamos la vida regenerada

                SaveLives();
                UpdateLivesUI();
            }

            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator UpdateTimerText()
    {
        while (currentLives < maxLives && nextLifeTimes.Count > 0)
        {
            DateTime nextLifeTime = nextLifeTimes.Peek();
            TimeSpan remainingTime = nextLifeTime - DateTime.Now;

            if (remainingTime.TotalSeconds > 0)
            {
                timerStringText.gameObject.SetActive(true);

                timerNumberText.text = $"{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
                timerNumberText.gameObject.SetActive(true);
            }
            else
            {
                timerNumberText.gameObject.SetActive(false);
                timerStringText.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(1);
        }

        timerStringText.gameObject.SetActive(false);
        timerNumberText.gameObject.SetActive(false);
    }

    private void LoadLives()
    {
        currentLives = PlayerPrefs.GetInt(LivesKey, maxLives);
        nextLifeTimes = new Queue<DateTime>();

        string savedTimes = PlayerPrefs.GetString(NextLivesKey, "");
        if (!string.IsNullOrEmpty(savedTimes))
        {
            string[] timeStrings = savedTimes.Split('|');
            foreach (string timeStr in timeStrings)
            {
                if (long.TryParse(timeStr, out long binaryTime))
                {
                    DateTime regenTime = DateTime.FromBinary(binaryTime);
                    if (regenTime > DateTime.Now)
                        nextLifeTimes.Enqueue(regenTime);
                    else
                        currentLives++; // Si ya pasó el tiempo, recuperamos la vida
                }
            }
        }

        SaveLives();
    }

    private void SaveLives()
    {
        PlayerPrefs.SetInt(LivesKey, currentLives);

        if (nextLifeTimes.Count > 0)
        {
            string serializedTimes = string.Join("|", Array.ConvertAll(nextLifeTimes.ToArray(), dt => dt.ToBinary().ToString()));
            PlayerPrefs.SetString(NextLivesKey, serializedTimes);
        }
        else
        {
            PlayerPrefs.DeleteKey(NextLivesKey);
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
            timerNumberText.gameObject.SetActive(false);
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
