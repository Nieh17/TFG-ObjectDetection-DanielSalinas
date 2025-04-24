using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LocalizationManager : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(SetLocaleToDeviceLanguage());
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

    public static async Task<string> GetLearningLocalizedString(string tableName, string key)
    {
        var englishLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
        if (englishLocale == null)
        {
            Debug.LogWarning("Idioma inglés no disponible.");
            return key;
        }

        var localizedValue = await GetLocalizedValueFromTable(tableName, key, englishLocale);
        return localizedValue ?? key;
    }

    public static async Task<List<WordPair>> GetLocalizedWordPairs(List<string> keys, string tableName)
    {
        var wordPairs = new List<WordPair>();

        await LocalizationSettings.InitializationOperation.Task;

        Locale currentLocale = LocalizationSettings.SelectedLocale;
        Locale englishLocale = LocalizationSettings.AvailableLocales.GetLocale("en");

        if (englishLocale == null)
        {
            Debug.LogWarning("No se encontró el idioma inglés en las locales disponibles.");
            return wordPairs;
        }

        var currentTableTask = GetStringTable(tableName, currentLocale);
        var englishTableTask = GetStringTable(tableName, englishLocale);

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
