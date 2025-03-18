using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class SyllableGame : MonoBehaviour
{
    public GameObject syllablePrefab;
    public GameObject dropSlotPrefab;
    public Transform syllableSpawnZone;
    public Transform dropSlotZone;
    public string wordToSplit;

    private float spacing = 250f;  // Espaciado horizontal entre sílabas y drop slots
    private float lineHeight = 200f; // Separación entre líneas en el eje Y

    private RectTransform syllableSpawnZoneRect;
    private RectTransform dropSlotZoneRect;

    private List<DropSlot> dropSlots = new List<DropSlot>();

    [Header("General Canvas")]
    [SerializeField] GameObject generalCanvas;

    [Header("Introduction")]
    [SerializeField] GameObject introCanvas;

    [Header("Game's Canvas")]
    [SerializeField] GameObject gameCanvas;

    [Header("End Game's Canvas")]
    [SerializeField] GameObject endPanel;

    [Header("End Game's Canvas Texts")]
    [SerializeField] TMP_Text accuracyTitle;
    [SerializeField] TMP_Text accuracyText;
    [SerializeField] TMP_Text timeText;
    [SerializeField] TMP_Text xpText;

    private float timer;
    private float timeLeft;
    private int minutes;
    private int seconds;

    private int totalTries;
    private int correctTries;
    private float accuracyRate;

    private int score;
    private int totalXp;

    private bool gameActive;

    private void OnEnable()
    {
        // Suscribirse al evento de DropSlot
        DropSlot.OnSyllablePlacedCorrectly += CheckWinCondition;

        gameActive = false;
        timer = 0f;
        minutes = 0;
        seconds = 0;

        totalTries = 0;
        correctTries = 0;
        accuracyRate = 0f;

        score = 0;
        totalXp = 0;

        generalCanvas.SetActive(true);
        introCanvas.SetActive(true);
    }

    private void OnDisable()
    {
        // Desuscribirse del evento de DropSlot para evitar referencias nulas
        DropSlot.OnSyllablePlacedCorrectly -= CheckWinCondition;
    }

    private void Start()
    {
        syllableSpawnZoneRect = syllableSpawnZone.GetComponent<RectTransform>();
        dropSlotZoneRect = dropSlotZone.GetComponent<RectTransform>();
    }

    public void startGame()
    {
        introCanvas.SetActive(false);
        gameCanvas.SetActive(true);
        GenerateGame();

    }

    private void Update()
    {
        if (!gameActive) return;

        timer += Time.deltaTime;
    }

    private void GenerateGame()
    {
        gameActive = true;

        List<string> syllables = GetSyllables(wordToSplit);

        // Generar sílabas desordenadas en la zona inicial
        List<string> shuffledSyllables = syllables.OrderBy(s => Random.value).ToList();
        GenerateSyllables(shuffledSyllables, syllableSpawnZone);

        // Generar los drop slots en el orden correcto
        GenerateDropSlots(syllables, dropSlotZone);
    }

    private void GenerateSyllables(List<string> syllables, Transform parent)
    {
        float currentX = 0f;
        float currentY = 0f;
        int syllablesPerLine = Mathf.FloorToInt(syllableSpawnZoneRect.rect.width / spacing); // Cuántas sílabas caben por línea

        for (int i = 0; i < syllables.Count; i++)
        {
            if (currentX + spacing > syllableSpawnZoneRect.rect.width) // Si no cabe en la línea actual
            {
                currentX = 0f; // Reiniciamos el X
                currentY -= lineHeight; // Movemos la sílaba a la siguiente fila
            }

            GameObject syllableObj = Instantiate(syllablePrefab, parent);
            syllableObj.GetComponent<DraggableSyllable>().SetText(syllables[i]);
            syllableObj.GetComponent<RectTransform>().localPosition = new Vector3(currentX, currentY, 0);
            currentX += spacing; // Avanzamos el X para la siguiente sílaba
        }
    }

    private void GenerateDropSlots(List<string> syllables, Transform parent)
    {
        dropSlots.Clear();

        float currentX = 0f;
        float currentY = 0f;
        int slotsPerLine = Mathf.FloorToInt(dropSlotZoneRect.rect.width / spacing); // Cuántos drop slots caben por línea

        for (int i = 0; i < syllables.Count; i++)
        {
            if (currentX + spacing > dropSlotZoneRect.rect.width) // Si no cabe en la línea actual
            {
                currentX = 0f; // Reiniciamos el X
                currentY -= lineHeight; // Movemos el slot a la siguiente fila
            }

            GameObject dropSlot = Instantiate(dropSlotPrefab, parent);
            dropSlot.GetComponent<RectTransform>().localPosition = new Vector3(currentX, currentY, 0);
            
            DropSlot slotComponent = dropSlot.GetComponent<DropSlot>();
            slotComponent.expectedSyllable = syllables[i];  // Guardar la sílaba correcta

            currentX += spacing; // Avanzamos el X para el siguiente drop slot

            dropSlots.Add(slotComponent);
        }
    }

    public List<string> GetSyllables(string word)
    {
        List<string> syllables = new List<string>();

        // Expresión regular para separar sílabas (básico para español e inglés)
        string pattern = @"(kya|kyu|kyo|gya|gyu|gyo|sha|shu|sho|ja|ju|jo|cha|chu|cho|nya|nyu|nyo|hya|hyu|hyo|"
                        + @"bya|byu|byo|pya|pyu|pyo|mya|myu|myo|rya|ryu|ryo|"
                        + @"kk|ss|tt|pp|"
                        + @"ba|bi|bu|be|bo|ca|chi|da|de|do|fa|fi|fu|fe|fo|ga|gi|gu|ge|go|"
                        + @"ha|hi|fu|he|ho|ka|ki|ku|ke|ko|ma|mi|mu|me|mo|"
                        + @"na|ni|nu|ne|no|pa|pi|pu|pe|po|ra|ri|ru|re|ro|"
                        + @"sa|shi|su|se|so|ta|te|to|wa|wo|ya|yu|yo|za|ji|zu|ze|zo|n|"
                        + @"[aiueo])";
        MatchCollection matches = Regex.Matches(word.ToLower(), pattern);

        foreach (Match match in matches)
        {
            syllables.Add(match.Value);
        }

        // Si solo hay una sílaba, no es necesario desordenar
        if (syllables.Count <= 1)
        {
            return new List<string> { word }; // Devuelve la palabra completa
        }

        return syllables;
    }

    private void CheckWinCondition()
    {
        // Verifica si todos los drop slots están llenos correctamente
        foreach (DropSlot slot in dropSlots)
        {
            if (!slot.IsCorrectlyFilled())
            {
                return; // Si uno no está correcto, no hemos terminado
            }
        }

        // Si llegamos aquí, el jugador ha colocado todas las sílabas correctamente
        EndGame();
    }

    private void EndGame()
    {
        endPanel.SetActive(true);

        minutes = Mathf.FloorToInt(timer / 60);
        seconds = Mathf.FloorToInt(timer % 60);

        accuracyRate = (correctTries / totalTries) * 100f;
        gameActive = false;

        totalXp = Mathf.CeilToInt(score / 10f);
        if (totalXp < 0) totalXp = 1;

        if (accuracyRate > 50) accuracyTitle.text = "Good";
        else accuracyTitle.text = "Bad";
        accuracyText.text = accuracyRate.ToString() + "%";
        timeText.text = minutes + ":" + seconds.ToString("D2");
        xpText.text = totalXp.ToString() + "XP";
    }

    public void returnToMainMenu(GameObject objectToActivate)
    {
        endPanel.SetActive(false);
        gameCanvas.SetActive(false);
        introCanvas.SetActive(true);
        generalCanvas.SetActive(false);

        gameObject.SetActive(false);

        objectToActivate.SetActive(true);
    }
}
