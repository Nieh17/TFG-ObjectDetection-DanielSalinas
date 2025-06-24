using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Experience")]
    [SerializeField] int currentLevel = 1;
    [SerializeField] int currentXP = 0;
    [SerializeField] int xpToNextLevel = 10;

    [Header("UI")]
    [SerializeField] Slider xpSlider;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text xpText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateXPUI();
        //ResetLevelData();
    }

    private void UpdateXPUI()
    {
        if (xpSlider != null)
        {
            xpSlider.value = 0;
            xpSlider.maxValue = xpToNextLevel;
            xpSlider.value = currentXP;
        }

        if (levelText != null)
            levelText.text = "Nivel " + currentLevel;

        if (xpText != null)
            xpText.text = $"{currentXP} / {xpToNextLevel} XP";
    }


    public void AddXP(int amount)
    {
        currentXP += amount;
        while (currentXP >= xpToNextLevel)
        {
            Debug.Log("CURRENT EXP: " + currentXP);

            currentXP -= xpToNextLevel;
            currentLevel++;
            xpToNextLevel = CalculateXPForNextLevel(currentLevel);
        }
        SaveProgress();
        UpdateXPUI();
    }

    int CalculateXPForNextLevel(int level)
    {
        return Mathf.FloorToInt(10 * Mathf.Pow(1.15f, level - 1));
    }

    void SaveProgress()
    {
        PlayerPrefs.SetInt("Level", currentLevel);
        PlayerPrefs.SetInt("XP", currentXP);
        PlayerPrefs.SetInt("XPToNextLevel", xpToNextLevel);
    }

    void LoadProgress()
    {
        currentLevel = PlayerPrefs.GetInt("Level", 1);
        currentXP = PlayerPrefs.GetInt("XP", 0);
        xpToNextLevel = PlayerPrefs.GetInt("XPToNextLevel", 10);
    }



    public void ResetLevelData()
    {
        PlayerPrefs.DeleteKey("Level");
        PlayerPrefs.DeleteKey("XP");
        PlayerPrefs.DeleteKey("XPToNextLevel");
        PlayerPrefs.Save();

        currentLevel = 1;
        currentXP = 0;
        xpToNextLevel = CalculateXPForNextLevel(currentLevel);

        UpdateXPUI();
    }
}
