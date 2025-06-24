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

    [SerializeField] UpdateButtonStatus updateButtonStatus;
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
        else
        {
            timerStringText.gameObject.SetActive(false);
            timerNumberText.gameObject.SetActive(false);
        }
    }


    public void LoseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;

            DateTime timeForNextLife = DateTime.Now;
            if (nextLifeTimes.Count > 0)
            {
                DateTime[] queuedTimes = nextLifeTimes.ToArray();
                timeForNextLife = queuedTimes[queuedTimes.Length - 1];
            }

            DateTime newRegenTime = timeForNextLife.AddMinutes(regenTimeMinutes);
            nextLifeTimes.Enqueue(newRegenTime);

            SaveLives();
            UpdateLivesUI();

            Debug.Log($"Vida perdida. Próxima vida en cola se recuperará en: {newRegenTime:HH:mm:ss}");

            if (currentLives < maxLives)
            {
                StopAllCoroutines();
                StartCoroutine(RegenerateLives());
                StartCoroutine(UpdateTimerText());
            }

            if (currentLives == 0)
            {
                updateButtonStatus.DisableButton();
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
                nextLifeTimes.Dequeue();

                SaveLives();
                UpdateLivesUI();

                if (currentLives > 0)
                {
                    updateButtonStatus.EnableButton();
                }
            }

            yield return new WaitForSeconds(1);
        }
        if (currentLives == maxLives)
        {
            StopCoroutine(UpdateTimerText());
            timerStringText.gameObject.SetActive(false);
            timerNumberText.gameObject.SetActive(false);
            updateButtonStatus.EnableButton();
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
                    if (regenTime > DateTime.Now.AddSeconds(-1))
                        nextLifeTimes.Enqueue(regenTime);
                    else
                        currentLives++;
                }
            }
        }

        while (nextLifeTimes.Count > 0 && nextLifeTimes.Peek() <= DateTime.Now && currentLives < maxLives)
        {
            nextLifeTimes.Dequeue();
            currentLives++;
        }

        currentLives = Mathf.Min(currentLives, maxLives);

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
        for (int i = 0; i < aliveHearts.Count; i++)
        {
            aliveHearts[i].SetActive(i < currentLives);
        }

        if (currentLives < maxLives)
        {
            timerStringText.gameObject.SetActive(true);
            timerNumberText.gameObject.SetActive(true);
        }
        else
        {
            timerStringText.gameObject.SetActive(false);
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
