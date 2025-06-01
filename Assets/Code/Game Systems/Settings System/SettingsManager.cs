using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SettingsManager : MonoBehaviour
{
    [Header("Initial Pop Up")]
    [SerializeField] GameObject initialPopUp;

    [Header("Settings")]
    [SerializeField] GameObject settingsContainer;
    [SerializeField] TMP_Dropdown languageDropdown;
    [SerializeField] Toggle introductionToggle;


    private const string FirstTimeKey = "FirstTimePlayed";
    private const string SelectedLanguageKey = "SelectedLanguage";
    private const string SkipIntroKey = "SkipIntro";

    private List<string> currentLanguageKeys;

    public static event UnityAction OnSettingsSaved;


    private void Awake()
    {
        settingsContainer.SetActive(false);

        //ResetFirstTimeKey();
        ResetLanguageAndSkipIntro();

        if (!PlayerPrefs.HasKey(FirstTimeKey))
        {
            if (initialPopUp != null)
                initialPopUp.SetActive(true);

            PlayerPrefs.SetInt(FirstTimeKey, 1);
            PlayerPrefs.Save();
        }
    }

    

    public void showSettings()
    {
        if (initialPopUp.activeInHierarchy)
        {
            initialPopUp.SetActive(false);
        }

        PopulateDropdown();
        LoadToggleSetting();
    }

    public void saveSettings()
    {
        int selectedIndex = languageDropdown.value;
        if (currentLanguageKeys != null && selectedIndex >= 0 && selectedIndex < currentLanguageKeys.Count)
        {
            string selectedLanguageKey = currentLanguageKeys[selectedIndex];
            PlayerPrefs.SetString(SelectedLanguageKey, selectedLanguageKey);
        }

        bool skipIntro = introductionToggle.isOn;
        PlayerPrefs.SetInt(SkipIntroKey, skipIntro ? 1 : 0);

        PlayerPrefs.Save();

        Debug.Log($"[SaveSettings] Language: {PlayerPrefs.GetString(SelectedLanguageKey)}, SkipIntro: {PlayerPrefs.GetInt(SkipIntroKey)}");

        settingsContainer.SetActive(false);

        OnSettingsSaved?.Invoke();
    }

    private async void PopulateDropdown()
    {
        languageDropdown.ClearOptions();
        currentLanguageKeys = new List<string>();

        Locale currentLocale = LocalizationSettings.SelectedLocale;
        Dictionary<string, string> languagesWithKeys = await LocalizationManager.GetAvailableLanguagesWithKeys(currentLocale);

        string preferredLanguage = PlayerPrefs.GetString(SelectedLanguageKey, "");

        List<string> localizedNames = new List<string>();

        if (!string.IsNullOrEmpty(preferredLanguage) && languagesWithKeys.ContainsKey(preferredLanguage))
        {
            currentLanguageKeys.Add(preferredLanguage);
            localizedNames.Add(languagesWithKeys[preferredLanguage]);
            languagesWithKeys.Remove(preferredLanguage);
        }

        foreach (var pair in languagesWithKeys)
        {
            currentLanguageKeys.Add(pair.Key);
            localizedNames.Add(pair.Value);
        }

        languageDropdown.AddOptions(localizedNames);

        if (!string.IsNullOrEmpty(preferredLanguage))
        {
            languageDropdown.value = 0;
        }

        settingsContainer.SetActive(true);
    }

    private void LoadToggleSetting()
    {
        if (PlayerPrefs.HasKey(SkipIntroKey))
        {
            bool skipIntro = PlayerPrefs.GetInt(SkipIntroKey) == 1;
            introductionToggle.isOn = skipIntro;
        }
        else
        {
            introductionToggle.isOn = false;
        }
    }



    private void ResetFirstTimeKey()
    {
        PlayerPrefs.DeleteKey(FirstTimeKey);
        PlayerPrefs.Save();
    }

    private void ResetLanguageAndSkipIntro()
    {
        PlayerPrefs.DeleteKey(SelectedLanguageKey);
        PlayerPrefs.DeleteKey(SkipIntroKey);
        PlayerPrefs.Save();
    }
}
