using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

public class MissionManager : MonoBehaviour
{
    [Header("Mission Window")]
    [SerializeField] List<TextMeshProUGUI> missionsText;
    [SerializeField] List<TextMeshProUGUI> missionsPercentText;
    [SerializeField] List<Image> missionsPercent;
    [SerializeField] Animator animator;


    private int numberOfMissions = 2; // Número de misiones que se eligen al azar
    private List<Mission> allMissions = new List<Mission>(); // Lista para almacenar todas las misiones
    private List<MissionProgress> todaysMissionsProgress = new List<MissionProgress>(); // Progreso de misiones del día

    private async void Start()
    {
        ResetMissionProgress();

        string todayDate = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
        string savedDate = PlayerPrefs.GetString("LastMissionDate", "");

        await LoadMissions();

        if (todayDate != savedDate)
        {
            SelectMissions();

            PlayerPrefs.SetString("LastMissionDate", todayDate);
            SaveMissionProgress();
        }
        else
        {
            LoadMissionProgress();
        }

        if(todaysMissionsProgress.Count == 0)
        {
            SelectMissions();
        }

        SetUi();
    }

    private void OnEnable()
    {
        SettingsManager.OnSettingsSaved += OnSettingsChanged;
    }

    private void OnDisable()
    {
        SettingsManager.OnSettingsSaved -= OnSettingsChanged;
    }



    private async Task LoadMissions()
    {
        Dictionary<string, int> classLabelMap = ClassLabelsManager.GetClassLabelMap();

        string missionLocalized = await LocalizationManager.GetUILocalizedString("mission");


        foreach (var kvp in classLabelMap)
        {
            string objectName = kvp.Key;
            int objectValue = kvp.Value;

            string objectNameLocalized = await LocalizationManager.GetLearningLocalizedString(objectName);

            string description = missionLocalized + objectNameLocalized;
            Debug.Log(description);
            allMissions.Add(new Mission(description, new List<int> { objectValue }));
        }
    }



    private void SelectMissions()
    {
        // Limpiar las misiones del día anterior
        todaysMissionsProgress.Clear();

        // Asegurarse de no repetir misiones (si hay suficientes)
        List<Mission> availableMissions = new List<Mission>(allMissions);
        List<string> selectedDescriptions = new List<string>();

        for (int i = 0; i < numberOfMissions; i++)
        {
            if (availableMissions.Count == 0) break;

            int index = Random.Range(0, availableMissions.Count);
            Mission selected = availableMissions[index];
            availableMissions.RemoveAt(index);

            // Crear el progreso para esta misión
            MissionProgress progress = new MissionProgress(selected.missionDescription, selected.objectValuesToPhotograph);
            todaysMissionsProgress.Add(progress);

            // Guardar en PlayerPrefs
            string key = "MissionProgress_" + progress.missionDescription;
            string jsonProgress = JsonUtility.ToJson(progress);
            PlayerPrefs.SetString(key, jsonProgress);

            selectedDescriptions.Add(progress.missionDescription);
        }

        // Guardar la lista de descripciones para recuperar las misiones más tarde
        string joinedDescriptions = string.Join("|", selectedDescriptions);
        PlayerPrefs.SetString("TodaysMissionDescriptions", joinedDescriptions);

        PlayerPrefs.Save();

        // Mostrar misiones de hoy (solo para debug)
        foreach (MissionProgress progress in todaysMissionsProgress)
        {
            Debug.Log("Misión de hoy: " + progress.missionDescription);
        }
    }


    // Guardar el progreso de las misiones en PersistentData
    private void SaveMissionProgress()
    {
        foreach (var progress in todaysMissionsProgress)
        {
            string key = "MissionProgress_" + progress.missionDescription;
            string jsonProgress = JsonUtility.ToJson(progress);
            PlayerPrefs.SetString(key, jsonProgress); // Guardamos el progreso como un JSON
        }
        PlayerPrefs.Save();
    }


    private void LoadMissionProgress()
    {
        todaysMissionsProgress.Clear();

        if (PlayerPrefs.HasKey("TodaysMissionDescriptions"))
        {
            string descriptions = PlayerPrefs.GetString("TodaysMissionDescriptions");
            string[] descriptionArray = descriptions.Split('|');

            foreach (string desc in descriptionArray)
            {
                string key = "MissionProgress_" + desc;
                if (PlayerPrefs.HasKey(key))
                {
                    string jsonProgress = PlayerPrefs.GetString(key);
                    MissionProgress progress = JsonUtility.FromJson<MissionProgress>(jsonProgress);
                    todaysMissionsProgress.Add(progress);
                }
                else
                {
                    Debug.LogWarning($"No se encontró progreso guardado para la misión: {desc}");
                }
            }
        }
        else
        {
            Debug.LogWarning("No se encontraron descripciones de misiones guardadas.");
        }
    }


    public void RegisterPhotographedObject(int objectValue)
    {
        Debug.Log("Entro en registered photographed object");
        Debug.Log("Todays Mission Progress Size: " + todaysMissionsProgress.Count);
        foreach (MissionProgress mission in todaysMissionsProgress)
        {
            bool wasAlreadyCompleted = mission.IsMissionCompleted();

            mission.AddPhotographedValue(objectValue);

            Debug.Log($"Misión: {mission.missionDescription} - Progreso: {mission.completionPercentage}%");

            if (!wasAlreadyCompleted && mission.IsMissionCompleted())
            {
                Debug.Log("¡Misión completada! Descripción: " + mission.missionDescription);
                GetMissionPopUp();
            }
        }

        SaveMissionProgress();

        SetUi();
    }

    public void GetMissionPopUp()
    {
        animator.SetTrigger("appear");
        StartCoroutine(CloseMissionPopUp());
    }

    private IEnumerator CloseMissionPopUp()
    {
        yield return new WaitForSeconds(5f);
        animator.SetTrigger("disappear");
    }

    void SetUi()
    {
        for (int i=0; i< numberOfMissions;i++)
        {
            MissionProgress mission = todaysMissionsProgress[i];

            missionsText[i].text = mission.missionDescription;
            missionsPercentText[i].text = mission.completionPercentage.ToString() + " %";
            missionsPercent[i].fillAmount = mission.completionPercentage / 100;
        }
    }

    private async void OnSettingsChanged()
    {
        allMissions.Clear();
        await LoadMissions();

        foreach (var mission in todaysMissionsProgress)
        {
            foreach (var possible in allMissions)
            {
                bool sameValues = possible.objectValuesToPhotograph.Count == mission.objectValuesToPhotograph.Count &&
                                  possible.objectValuesToPhotograph.TrueForAll(v => mission.objectValuesToPhotograph.Contains(v));

                if (sameValues)
                {
                    mission.missionDescription = possible.missionDescription;
                    break;
                }
            }
        }

        SaveMissionProgress();

        SetUi();

        Debug.Log("[MissionManager] Misiones del día actualizadas al nuevo idioma.");
    }




    private void ResetMissionProgress()
    {
        foreach (MissionProgress mission in todaysMissionsProgress)
        {
            string key = "MissionProgress_" + mission.missionDescription;
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        PlayerPrefs.DeleteKey("TodaysMissionDescriptions");

        PlayerPrefs.DeleteKey("LastMissionDate");

        PlayerPrefs.Save();
    }
}
