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
            return;
        }

        Transform[] minigames = gamesParent.GetComponentsInChildren<Transform>(true);

        List<GameObject> minigameObjects = new List<GameObject>();
        foreach (Transform child in minigames)
        {
            if (child.parent == gamesParent.transform)
            {
                minigameObjects.Add(child.gameObject);
            }
        }

        GameObject selectedMinigame = minigameObjects[Random.Range(0, minigameObjects.Count)];
        selectedMinigame.SetActive(true);


        objectToDeactivate.SetActive(false);
    }
}

