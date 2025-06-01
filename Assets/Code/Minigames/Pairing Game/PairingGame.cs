using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using Unity.VisualScripting;

public class PairingGame : GameBase
{
    public List<WordPair> wordPairs;

    [Header("General Canvas")]
    [SerializeField] private GameObject generalCanvas;
    protected override GameObject GeneralCanvas => generalCanvas;

    [Header("Introduction")]
    [SerializeField] private GameObject introCanvas;
    protected override GameObject IntroCanvas => introCanvas;

    [Header("Game's Canvas")]
    [SerializeField] private GameObject gameCanvas;
    protected override GameObject GameCanvas => gameCanvas;

    [Header("End Game's Canvas")]
    [SerializeField] private GameObject endPanel;
    protected override GameObject EndPanel => endPanel;

    [Header("Game's Objects")]
    public GameObject buttonPrefab;
    public Transform nativeColumn;
    public Transform translatedColumn;
    [SerializeField] TMP_Text currentLivesText;

    List<WordPair> selectedPairs;
    private Dictionary<Button, string> buttonDictionary = new Dictionary<Button, string>();
    private List<Button> originalButtons = new List<Button>();
    private List<Button> translatedButtons = new List<Button>();
    private HashSet<Button> temporarilyDisabledButtons = new HashSet<Button>();
    private HashSet<Button> pairedButtons = new HashSet<Button>();
    private Button firstSelected = null;

    private const int maxButtons = 5;
    private const float verticalSpacing = 16f;

    protected override void OnEnable()
    {
        base.OnEnable();
        //ClearPreviousButtons();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void StartGame()
    {
        base.StartGame();
    }

    protected override async void SetupGame()
    {
        string imageDirectory = Path.Combine(Application.persistentDataPath, "SavedImages");
        List<WordPair> wordPairs = await WordPreparationService.PrepareWordQueueAsync(imageDirectory, maxButtons);

        if (wordPairs == null || wordPairs.Count == 0)
        {
            Debug.LogWarning("No se pudieron cargar las palabras.");
            return;
        }

        selectedPairs = new List<WordPair>(wordPairs);

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
        buttonDictionary.Clear();

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
        if (pairedButtons.Contains(btn) || temporarilyDisabledButtons.Contains(btn))
            return;

        if (firstSelected != null)
        {
            bool firstIsOriginal = originalButtons.Contains(firstSelected);
            bool currentIsOriginal = originalButtons.Contains(btn);

            if (firstIsOriginal == currentIsOriginal)
            {
                return;
            }

            totalTries += 1;

            if (ArePair(firstSelected, btn))
            {
                DisableButton(firstSelected, true);
                DisableButton(btn, true);
                pairedButtons.Add(firstSelected);
                pairedButtons.Add(btn);

                correctTries += 1;
                score += 10;

                CheckGameEnd();
            }
            else
            {
                score -= 5;
                firstSelected.GetComponent<Image>().color = Color.white;

                base.LoseLife(currentLivesText);
            }

            firstSelected = null;
            EnableAllButtons();
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
                colors.normalColor = new Color(0.85f, 0.85f, 0.85f, 1f);
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
                colors.normalColor = Color.white;
                btn.colors = colors;
            }
        }
        temporarilyDisabledButtons.Clear();
    }

    void DisableButton(Button btn, bool permanently)
    {
        btn.interactable = false;
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0f, 0.85f, 0f, 1f);
        colors.disabledColor = new Color(0f, 0.85f, 0f, 1f);
        btn.colors = colors;
    }

    bool ArePair(Button a, Button b)
    {
        if (!buttonDictionary.ContainsKey(a) || !buttonDictionary.ContainsKey(b))
            return false;

        string wordA = buttonDictionary[a];
        string wordB = buttonDictionary[b];

        return selectedPairs.Exists(pair =>
            (pair.nativeWord == wordA && pair.translatedWord == wordB) ||
            (pair.nativeWord == wordB && pair.translatedWord == wordA));
    }

    void CheckGameEnd()
    {
        if (pairedButtons.Count == selectedPairs.Count * 2)
        {
            EndGame();
        }
    }

    protected override void ResetGameSpecificUI()
    {
        //ClearPreviousButtons();
    }

    private void ClearPreviousButtons()
    {
        foreach (Transform child in nativeColumn) Destroy(child.gameObject);
        foreach (Transform child in translatedColumn) Destroy(child.gameObject);
    }
}