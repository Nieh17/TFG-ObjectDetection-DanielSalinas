using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PairingGame : MonoBehaviour
{
    [System.Serializable]
    public class WordPair
    {
        public string nativeWord;
        public string translatedWord;
    }

    public List<WordPair> wordPairs;
    public GameObject buttonPrefab;
    public Transform nativeColumn;
    public Transform translatedColumn;

    private Dictionary<Button, string> buttonDictionary = new Dictionary<Button, string>();
    private List<Button> originalButtons = new List<Button>();
    private List<Button> translatedButtons = new List<Button>();
    private HashSet<Button> temporarilyDisabledButtons = new HashSet<Button>(); // Botones bloqueados temporalmente
    private HashSet<Button> pairedButtons = new HashSet<Button>(); // Botones emparejados permanentemente
    private Button firstSelected = null;

    private const int maxButtons = 5;
    private const float verticalSpacing = 16f;

    void Start()
    {
        SetupGame();
    }

    void SetupGame()
    {
        List<WordPair> selectedPairs = new List<WordPair>(wordPairs);
        if (selectedPairs.Count > maxButtons)
        {
            selectedPairs = selectedPairs.GetRange(0, maxButtons);
        }

        CreateButtons(selectedPairs);

        ShuffleButtons(originalButtons, nativeColumn);
        ShuffleButtons(translatedButtons, translatedColumn);
    }

    void CreateButtons(List<WordPair> selectedPairs)
    {
        originalButtons.Clear();
        translatedButtons.Clear();
        temporarilyDisabledButtons.Clear();
        pairedButtons.Clear();

        for (int i = 0; i < selectedPairs.Count; i++)
        {
            float yOffset = -i * verticalSpacing;

            Button originalBtn = CreateButton(selectedPairs[i].nativeWord, nativeColumn, yOffset);
            Button translatedBtn = CreateButton(selectedPairs[i].translatedWord, translatedColumn, yOffset);

            originalButtons.Add(originalBtn);
            translatedButtons.Add(translatedBtn);
        }
    }

    void ShuffleButtons(List<Button> buttons, Transform parent)
    {
        List<Vector3> positions = new List<Vector3>();

        foreach (Button btn in buttons)
        {
            positions.Add(btn.transform.position);
        }

        positions.Shuffle();

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].transform.position = positions[i];
        }
    }

    Button CreateButton(string text, Transform parent, float yOffset)
    {
        GameObject btnObj = Instantiate(buttonPrefab, parent);

        Button btn = btnObj.GetComponent<Button>();
        TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
        btnText.text = text;

        RectTransform rectTransform = btnObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition += new Vector2(0, yOffset);

        buttonDictionary.Add(btn, text);
        btn.onClick.AddListener(() => OnButtonClicked(btn));

        return btn;
    }

    void OnButtonClicked(Button btn)
    {
        // Si el botón ya ha sido emparejado, no hacer nada
        if (pairedButtons.Contains(btn) || temporarilyDisabledButtons.Contains(btn))
            return;

        if (firstSelected != null)
        {
            bool firstIsOriginal = originalButtons.Contains(firstSelected);
            bool currentIsOriginal = originalButtons.Contains(btn);

            if (firstIsOriginal == currentIsOriginal)
            {
                return; // No permite seleccionar dos botones de la misma columna
            }

            if (ArePair(firstSelected, btn))
            {
                DisableButton(firstSelected, true);
                DisableButton(btn, true);
                pairedButtons.Add(firstSelected);
                pairedButtons.Add(btn);
            }
            else
            {
                firstSelected.GetComponent<Image>().color = Color.white;
            }

            firstSelected = null;
            EnableAllButtons(); // Reactivar los botones bloqueados temporalmente
        }
        else
        {
            firstSelected = btn;
            firstSelected.GetComponent<Image>().color = Color.yellow;
            TemporarilyDisableButtons(firstSelected);
        }
    }

    void TemporarilyDisableButtons(Button selected)
    {
        List<Button> buttonsToDisable = originalButtons.Contains(selected) ? originalButtons : translatedButtons;

        foreach (Button btn in buttonsToDisable)
        {
            if (btn != selected && !pairedButtons.Contains(btn))
            {
                ColorBlock colors = btn.colors;
                colors.normalColor = new Color(0.85f, 0.85f, 0.85f, 1f); // Gris claro
                btn.colors = colors;
                temporarilyDisabledButtons.Add(btn);
            }
        }
    }

    void EnableAllButtons()
    {
        foreach (Button btn in temporarilyDisabledButtons)
        {
            if (!pairedButtons.Contains(btn))
            {
                ColorBlock colors = btn.colors;
                colors.normalColor = Color.white; // Restaurar el color a blanco
                btn.colors = colors;
            }
        }
        temporarilyDisabledButtons.Clear();
    }

    void DisableButton(Button btn, bool permanently)
    {
        btn.interactable = false;
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0f, 0.85f, 0f, 1f); // Verde claro
        colors.disabledColor = new Color(0f, 0.85f, 0f, 1f); // Verde claro cuando está deshabilitado
        btn.colors = colors;
    }

    bool ArePair(Button a, Button b)
    {
        if (!buttonDictionary.ContainsKey(a) || !buttonDictionary.ContainsKey(b))
            return false;

        string wordA = buttonDictionary[a];
        string wordB = buttonDictionary[b];

        return wordPairs.Exists(pair =>
            (pair.nativeWord == wordA && pair.translatedWord == wordB) ||
            (pair.nativeWord == wordB && pair.translatedWord == wordA));
    }
}

// Extensión para mezclar listas
public static class ListExtensions
{
    private static System.Random rng = new System.Random();
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
