using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class WordPreparationService
{
    public static async Task<List<WordPair>> PrepareWordQueueAsync(string imageDirectory, string tableName, int maxWords)
    {
        if (!Directory.Exists(imageDirectory))
        {
            Debug.LogWarning("No se encontró la carpeta de imágenes guardadas.");
            return new List<WordPair>();
        }

        string[] imagePaths = Directory.GetFiles(imageDirectory, "*.jpg");
        List<string> keys = imagePaths.Select(path => Path.GetFileNameWithoutExtension(path)).ToList();

        // Obtiene las palabras localizadas
        List<WordPair> loadedWords = await LocalizationManager.GetLocalizedWordPairs(keys, tableName);

        if (loadedWords == null || loadedWords.Count == 0)
        {
            Debug.LogWarning("No se encontraron palabras válidas.");
            return new List<WordPair>();
        }

        // Limitar el número de palabras a las máximas permitidas
        return loadedWords.OrderBy(w => Random.value).Take(maxWords).ToList();


        /* List<string> keys = imagePaths.Select(path => Path.GetFileNameWithoutExtension(path)).ToList();

        List<WordPair> loadedWords = await LocalizationManager.GetLocalizedWordPairs(keys, "Learning_Content");


         loadedWords = loadedWords
        .Where(w => GetSyllables(w.nativeWord).Count > 1)
        .OrderBy(w => Random.value)
        .Take(maxWords)
        .ToList();
        

        wordsToPlay = new Queue<WordPair>(loadedWords);
        
        return true;
         */
    }
}
