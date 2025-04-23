using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour
{
    [Header("Mission Window")]
    [SerializeField] List<TextMeshProUGUI> missionsText;
    [SerializeField] List<TextMeshProUGUI> missionsPercentText;
    [SerializeField] List<Image> missionsPercent;
    [SerializeField] Animator animator;


    private int numberOfMissions = 2; // N�mero de misiones que se eligen al azar
    private List<Mission> allMissions = new List<Mission>(); // Lista para almacenar todas las misiones
    private List<MissionProgress> todaysMissionsProgress = new List<MissionProgress>(); // Progreso de misiones del d�a

    private void Start()
    {
        //ResetMissionProgress();

        string todayDate = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
        string savedDate = PlayerPrefs.GetString("LastMissionDate", "");

        LoadMissions();

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


    // Cargar todas las misiones posibles
    private void LoadMissions()
    {
        allMissions.Add(new Mission("Fotograf�a un flamenco", new List<int> { 7 }));
        allMissions.Add(new Mission("Fotograf�a un coche", new List<int> { 3 }));
    }


    private void SelectMissions()
    {
        // Limpiar las misiones del d�a anterior
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

            // Crear el progreso para esta misi�n
            MissionProgress progress = new MissionProgress(selected.missionDescription, selected.objectValuesToPhotograph);
            todaysMissionsProgress.Add(progress);

            // Guardar en PlayerPrefs
            string key = "MissionProgress_" + progress.missionDescription;
            string jsonProgress = JsonUtility.ToJson(progress);
            PlayerPrefs.SetString(key, jsonProgress);

            selectedDescriptions.Add(progress.missionDescription);
        }

        // Guardar la lista de descripciones para recuperar las misiones m�s tarde
        string joinedDescriptions = string.Join("|", selectedDescriptions);
        PlayerPrefs.SetString("TodaysMissionDescriptions", joinedDescriptions);

        PlayerPrefs.Save();

        // Mostrar misiones de hoy (solo para debug)
        foreach (MissionProgress progress in todaysMissionsProgress)
        {
            Debug.Log("Misi�n de hoy: " + progress.missionDescription);
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
                    Debug.LogWarning($"No se encontr� progreso guardado para la misi�n: {desc}");
                }
            }
        }
        else
        {
            Debug.LogWarning("No se encontraron descripciones de misiones guardadas.");
        }
    }


    // Comprobar si la misi�n ha sido completada (cuando el jugador haya fotografiado algo)
    public void CompleteMission(List<int> photographedValues)
    {
        foreach (MissionProgress mission in todaysMissionsProgress)
        {
            foreach (var value in photographedValues)
            {
                mission.AddPhotographedValue(value);
            }

            // Aqu� puedes mostrar el porcentaje de completado o activar alg�n evento
            Debug.Log($"Misi�n: {mission.missionDescription} - Progreso: {mission.completionPercentage}%");

            if (mission.completionPercentage >= 100)
            {
                Debug.Log("�Misi�n completada! Descripci�n: " + mission.missionDescription);
                SaveMissionProgress(); // Guardamos el progreso cuando se completa
            }
        }
    }

    public void RegisterPhotographedObject(int objectValue)
    {
        Debug.Log("Entro en registered photographed object");
        Debug.Log("Todays Mission Progress Size: " + todaysMissionsProgress.Count);
        foreach (MissionProgress mission in todaysMissionsProgress)
        {
            mission.AddPhotographedValue(objectValue);

            Debug.Log($"Misi�n: {mission.missionDescription} - Progreso: {mission.completionPercentage}%");

            if (mission.completionPercentage >= 100)
            {
                Debug.Log("�Misi�n completada! Descripci�n: " + mission.missionDescription);
            }
        }

        SaveMissionProgress();

        SetUi();
    }

    public void GetUi(Button buttonToDissappear)
    {
        Debug.Log("Entro al getUI");
        animator.SetTrigger("appear");

        buttonToDissappear.gameObject.SetActive(false);
    }

    public void CloseUI(Button buttonToAppear)
    {
        animator.SetTrigger("disappear");
        buttonToAppear.gameObject.SetActive(true);
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
