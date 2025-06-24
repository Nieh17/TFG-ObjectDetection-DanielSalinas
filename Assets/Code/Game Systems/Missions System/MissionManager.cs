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


    private int numberOfMissions = 2;
    private List<Mission> allMissions = new List<Mission>();
    private List<MissionProgress> todaysMissionsProgress = new List<MissionProgress>();

    private async void Start()
    {
        //ResetMissionProgress();

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
        ModelInference.OnObjectPredicted += RegisterPhotographedObject;
    }

    private void OnDisable()
    {
        SettingsManager.OnSettingsSaved -= OnSettingsChanged;
        ModelInference.OnObjectPredicted -= RegisterPhotographedObject;
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

            allMissions.Add(new Mission(description, new List<int> { objectValue }));
        }
    }



    /*private void SelectMissions()
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
    }*/
    private void SelectMissions()
    {
        todaysMissionsProgress.Clear();

        Mission requiredMission = null;
        // Find the mission that requires objectValue 1
        foreach (var mission in allMissions)
        {
            if (mission.objectValuesToPhotograph.Contains(1))
            {
                requiredMission = mission;
                break;
            }
        }

        if (requiredMission == null)
        {
            Debug.LogWarning("No mission found with objectValue 1. Selecting two random missions instead.");
            // Fallback: If no mission with objectValue 1 exists, select two random ones.
            List<Mission> availableMissionsFallback = new List<Mission>(allMissions);
            List<string> selectedDescriptionsFallback = new List<string>();

            for (int i = 0; i < numberOfMissions; i++)
            {
                if (availableMissionsFallback.Count == 0) break;

                int index = Random.Range(0, availableMissionsFallback.Count);
                Mission selected = availableMissionsFallback[index];
                availableMissionsFallback.RemoveAt(index);

                MissionProgress progress = new MissionProgress(selected.missionDescription, selected.objectValuesToPhotograph);
                todaysMissionsProgress.Add(progress);
                string key = "MissionProgress_" + progress.missionDescription;
                string jsonProgress = JsonUtility.ToJson(progress);
                PlayerPrefs.SetString(key, jsonProgress);
                selectedDescriptionsFallback.Add(progress.missionDescription);
            }
            string joinedDescriptionsFallback = string.Join("|", selectedDescriptionsFallback);
            PlayerPrefs.SetString("TodaysMissionDescriptions", joinedDescriptionsFallback);
            PlayerPrefs.Save();
            return; // Exit as missions are already selected
        }

        // Add the required mission first
        MissionProgress requiredMissionProgress = new MissionProgress(requiredMission.missionDescription, requiredMission.objectValuesToPhotograph);
        todaysMissionsProgress.Add(requiredMissionProgress);

        // Prepare a list of available missions for the second slot, excluding the required one
        List<Mission> availableMissionsForRandom = new List<Mission>(allMissions);
        availableMissionsForRandom.RemoveAll(m => m.missionDescription == requiredMission.missionDescription);

        // Select the second mission randomly from the remaining
        if (availableMissionsForRandom.Count > 0)
        {
            int randomIndex = Random.Range(0, availableMissionsForRandom.Count);
            Mission randomMission = availableMissionsForRandom[randomIndex];
            MissionProgress randomMissionProgress = new MissionProgress(randomMission.missionDescription, randomMission.objectValuesToPhotograph);
            todaysMissionsProgress.Add(randomMissionProgress);
        }
        else
        {
            Debug.LogWarning("Not enough unique missions to select a second random mission. Only the required mission will be set.");
        }

        // Save all selected missions' progress
        List<string> selectedDescriptions = new List<string>();
        foreach (var progress in todaysMissionsProgress)
        {
            string key = "MissionProgress_" + progress.missionDescription;
            string jsonProgress = JsonUtility.ToJson(progress);
            PlayerPrefs.SetString(key, jsonProgress);
            selectedDescriptions.Add(progress.missionDescription);
        }

        string joinedDescriptions = string.Join("|", selectedDescriptions);
        PlayerPrefs.SetString("TodaysMissionDescriptions", joinedDescriptions);
        PlayerPrefs.Save();
    }


    // Guardar el progreso de las misiones en PersistentData
    private void SaveMissionProgress()
    {
        foreach (var progress in todaysMissionsProgress)
        {
            string key = "MissionProgress_" + progress.missionDescription;
            string jsonProgress = JsonUtility.ToJson(progress);
            PlayerPrefs.SetString(key, jsonProgress);
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

        foreach (MissionProgress mission in todaysMissionsProgress)
        {
            bool wasAlreadyCompleted = mission.IsMissionCompleted();

            mission.AddPhotographedValue(objectValue);


            if (!wasAlreadyCompleted && mission.IsMissionCompleted())
            {
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
