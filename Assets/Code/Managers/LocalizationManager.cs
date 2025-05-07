using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LocalizationManager : MonoBehaviour
{

    private const string CurrentLanguageKey = "CurrentLanguage";

    private const string UI_TEXTS_TABLE = "UI_Texts";
    private const string LEARNING_CONTENT_TABLE = "Learning_Content";

    private const string JAPANESE = "japanese";
    private const string SPANISH = "spanish";
    private const string ENGLISH = "english";

    private const string SelectedLanguageKey = "SelectedLanguage";

    private void Awake()
    {
        SetAvailableLanguages();
        StartCoroutine(SetLocaleToDeviceLanguage());
    }

    private void SetAvailableLanguages()
    {
        Dictionary<string, List<string>> languageAlternatives = new Dictionary<string, List<string>>()
    {
        { SPANISH, new List<string> { ENGLISH, JAPANESE } },
        { JAPANESE, new List<string> { ENGLISH, SPANISH } },
        { ENGLISH, new List<string> { JAPANESE, SPANISH } }
    };

        foreach (var pair in languageAlternatives)
        {
            string key = pair.Key;
            string value = string.Join(",", pair.Value);
            PlayerPrefs.SetString(key, value);
        }

        string systemLanguage = Application.systemLanguage.ToString().ToLower();

        if (!languageAlternatives.ContainsKey(systemLanguage))
        {
            systemLanguage = "english";
        }

        PlayerPrefs.SetString(CurrentLanguageKey, systemLanguage);
        PlayerPrefs.Save();
    }


    private IEnumerator SetLocaleToDeviceLanguage()
    {
        yield return LocalizationSettings.InitializationOperation;

        SystemLanguage systemLanguage = Application.systemLanguage;
        Locale deviceLocale = LocalizationSettings.AvailableLocales.GetLocale(systemLanguage);

        if (deviceLocale != null)
        {
            LocalizationSettings.SelectedLocale = deviceLocale;
            Debug.Log($"Idioma del sistema detectado y aplicado: {deviceLocale.Identifier.Code}");
        }
        else
        {
            Locale fallback = LocalizationSettings.AvailableLocales.GetLocale("en");
            LocalizationSettings.SelectedLocale = fallback;
            Debug.LogWarning("Idioma del sistema no soportado. Se aplica inglés por defecto.");
        }
    }

    public static async Task<Dictionary<string, string>> GetAvailableLanguagesWithKeys(Locale locale)
    {
        string code = locale.Identifier.Code.ToLower();

        Dictionary<string, string> localeMap = new Dictionary<string, string>
        {
            {"es", SPANISH },
            {"en", ENGLISH },
            {"ja", JAPANESE }
        };

        if (!localeMap.TryGetValue(code, out string currentLanguageKey))
            currentLanguageKey = "english";

        string saved = PlayerPrefs.GetString(currentLanguageKey, "");
        if (string.IsNullOrEmpty(saved)) return new Dictionary<string, string>();

        List<string> languageKeys = new List<string>(saved.Split(','));
        Dictionary<string, string> result = new Dictionary<string, string>();

        foreach (string key in languageKeys)
        {
            string trimmedKey = key.Trim();
            string localized = await GetLocalizedValueFromTable(UI_TEXTS_TABLE, trimmedKey, locale);
            result[trimmedKey] = string.IsNullOrEmpty(localized) ? trimmedKey : localized;
        }

        return result;
    }


    public static List<string> GetLanguageKeys(Locale locale)
    {
        string code = locale.Identifier.Code.ToLower();

        Dictionary<string, string> localeMap = new Dictionary<string, string>
        {
            {"es", SPANISH },
            {"en", ENGLISH },
            {"ja", JAPANESE }
        };

        if (!localeMap.TryGetValue(code, out string currentLanguageKey))
            currentLanguageKey = "english";

        string saved = PlayerPrefs.GetString(currentLanguageKey, "");
        if (string.IsNullOrEmpty(saved)) return new List<string>();

        return new List<string>(saved.Split(',')).ConvertAll(k => k.Trim());
    }


    public static async Task<string> GetLearningLocalizedString(string key)
    {
        string languageStudying = PlayerPrefs.GetString(SelectedLanguageKey, ENGLISH);
        string languageStudyingCode = "en";
        switch (languageStudying)
        {
            case JAPANESE:
                languageStudyingCode = "ja";
                break;

            case SPANISH:
                languageStudyingCode = "es";
                break;

            default:
                break;
        }

        var locale = LocalizationSettings.AvailableLocales.GetLocale(languageStudyingCode);
        if (locale == null)
        {
            Debug.LogWarning("Idioma inglés no disponible.");
            return key;
        }

        var localizedValue = await GetLocalizedValueFromTable(LEARNING_CONTENT_TABLE, key, locale);
        return localizedValue ?? key;
    }

    public static async Task<string> GetUILocalizedString(string key)
    {
        // Obtener el locale según el idioma actual del dispositivo
        var locale = LocalizationSettings.SelectedLocale;

        if (locale == null)
        {
            Debug.LogWarning("Locale del dispositivo no disponible. Se devuelve la clave como fallback.");
            return key;
        }

        var localizedValue = await GetLocalizedValueFromTable(UI_TEXTS_TABLE, key, locale);
        return localizedValue ?? key;
    }


    public static async Task<List<WordPair>> GetLocalizedWordPairs(List<string> keys)
    {
        var wordPairs = new List<WordPair>();

        await LocalizationSettings.InitializationOperation.Task;

        Locale currentLocale = LocalizationSettings.SelectedLocale;

        string languageStudying = PlayerPrefs.GetString(SelectedLanguageKey, ENGLISH);
        string languageStudyingCode = "en";
        switch (languageStudying)
        {
            case JAPANESE:
                languageStudyingCode = "ja";
                break;

            case SPANISH:
                languageStudyingCode = "es";
                break;

            default:
                break;
        }

        Locale studyingLanguageLocale = LocalizationSettings.AvailableLocales.GetLocale(languageStudyingCode);

        if (studyingLanguageLocale == null)
        {
            Debug.LogWarning("No se encontró el idioma inglés en las locales disponibles.");
            return wordPairs;
        }

        var currentTableTask = GetStringTable(UI_TEXTS_TABLE, currentLocale);
        var englishTableTask = GetStringTable(LEARNING_CONTENT_TABLE, studyingLanguageLocale);

        await Task.WhenAll(currentTableTask, englishTableTask);

        var currentTable = currentTableTask.Result;
        var englishTable = englishTableTask.Result;

        if (currentTable == null || englishTable == null)
        {
            Debug.LogError("No se pudieron cargar las tablas de localización.");
            return wordPairs;
        }

        foreach (string key in keys)
        {
            string nativeWord = currentTable.GetEntry(key)?.LocalizedValue ?? key;
            string translatedWord = englishTable.GetEntry(key)?.LocalizedValue ?? key;

            wordPairs.Add(new WordPair(nativeWord, translatedWord));
        }

        return wordPairs;
    }

    private static async Task<StringTable> GetStringTable(string tableName, Locale locale)
    {
        var tableOperation = LocalizationSettings.StringDatabase.GetTableAsync(tableName, locale);
        await tableOperation.Task;
        return tableOperation.Result;
    }

    private static async Task<string> GetLocalizedValueFromTable(string tableName, string key, Locale locale)
    {
        var table = await GetStringTable(tableName, locale);

        if (table == null)
        {
            Debug.LogError($"No se pudo cargar la tabla de localización: {tableName}");
            return null;
        }

        var entry = table.GetEntry(key);
        if (entry == null || string.IsNullOrEmpty(entry.LocalizedValue))
        {
            Debug.LogWarning($"No se encontró una entrada para la clave '{key}' en la tabla '{tableName}'");
            return null;
        }

        return entry.LocalizedValue;
    }
}
