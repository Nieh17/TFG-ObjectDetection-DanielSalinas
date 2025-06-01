using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class WordPreparationService
{
    public static async Task<List<WordPair>> PrepareWordQueueAsync(string imageDirectory, int maxWords)
    {
        if (!Directory.Exists(imageDirectory))
        {
            Debug.LogWarning("No se encontró la carpeta de imágenes guardadas.");
            return new List<WordPair>();
        }

        string[] imagePaths = Directory.GetFiles(imageDirectory, "*.jpg");
        List<string> keys = imagePaths.Select(path => Path.GetFileNameWithoutExtension(path)).ToList();

        List<WordPair> loadedWords = await LocalizationManager.GetLocalizedWordPairs(keys);

        if (loadedWords == null || loadedWords.Count == 0)
        {
            Debug.LogWarning("No se encontraron palabras válidas.");
            return new List<WordPair>();
        }

        return loadedWords.OrderBy(w => Random.value).Take(maxWords).ToList();
    }
}
