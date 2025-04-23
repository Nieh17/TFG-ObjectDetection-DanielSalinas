using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Localization.Tables;

public class LocalizationManager : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(SetLocaleToDeviceLanguage());
    }

    private IEnumerator SetLocaleToDeviceLanguage()
    {
        yield return LocalizationSettings.InitializationOperation;

        // Obtener idioma del sistema
        SystemLanguage systemLanguage = Application.systemLanguage;

        // Buscar Locale compatible
        Locale deviceLocale = LocalizationSettings.AvailableLocales.GetLocale(systemLanguage);

        if (deviceLocale != null)
        {
            LocalizationSettings.SelectedLocale = deviceLocale;
            Debug.Log($"Idioma del sistema detectado y aplicado: {deviceLocale.Identifier.Code}");
        }
        else
        {
            // Si no está disponible, usar uno por defecto
            Locale fallback = LocalizationSettings.AvailableLocales.GetLocale("en");
            LocalizationSettings.SelectedLocale = fallback;
            Debug.LogWarning("Idioma del sistema no soportado. Se aplica inglés por defecto.");
        }
    }



    public static async Task<string> GetLearningLocalizedString(string tableName, string key)
    {

        Locale englishLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
        if (englishLocale == null)
        {
            Debug.LogWarning("Idioma inglés no disponible.");
            return key;
        }

        var tableLoadingOperation = LocalizationSettings.StringDatabase.GetTableAsync(tableName, englishLocale);
        await tableLoadingOperation.Task;

        StringTable table = tableLoadingOperation.Result;
        if (table == null)
        {
            Debug.LogError($"No se pudo cargar la tabla de localización: {tableName}");
            return key;
        }

        StringTableEntry entry = table.GetEntry(key);
        if (entry == null || string.IsNullOrEmpty(entry.LocalizedValue))
        {
            Debug.LogWarning($"No se encontró una entrada para la clave '{key}' en la tabla '{tableName}'");
            return key;
        }

        return entry.LocalizedValue;
    }
}
