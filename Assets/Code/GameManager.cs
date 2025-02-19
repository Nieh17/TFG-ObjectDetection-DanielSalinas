using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void StartRandomMinigame(GameObject objectToDeactivate)
    {
        GameObject gamesParent = GameObject.FindGameObjectWithTag("GAMES");

        if (gamesParent == null)
        {
            Debug.LogError("No se encontró un objeto con la etiqueta 'GAMES'.");
            return;
        }

        // Obtener todos los minijuegos dentro de "GAMES"
        Transform[] minigames = gamesParent.GetComponentsInChildren<Transform>(true);

        // Filtrar solo los hijos directos que representen minijuegos
        List<GameObject> minigameObjects = new List<GameObject>();
        foreach (Transform child in minigames)
        {
            if (child.parent == gamesParent.transform) // Solo hijos directos
            {
                minigameObjects.Add(child.gameObject);
            }
        }

        // Verificar que haya minijuegos disponibles
        if (minigameObjects.Count == 0)
        {
            Debug.LogError("No hay minijuegos en 'GAMES'.");
            return;
        }

        // Escoger un minijuego aleatorio y activarlo
        GameObject selectedMinigame = minigameObjects[Random.Range(0, minigameObjects.Count)];
        selectedMinigame.SetActive(true);

        Debug.Log("Minijuego seleccionado: " + selectedMinigame.name);

        objectToDeactivate.SetActive(false);
    }
}

