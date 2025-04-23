using System.Collections.Generic;

[System.Serializable]
public class MissionProgress
{
    public string missionDescription;
    public List<int> objectValuesToPhotograph;
    public List<int> photographedValues;
    public float completionPercentage;

    public MissionProgress(string description, List<int> objectValues)
    {
        missionDescription = description;
        objectValuesToPhotograph = objectValues;
        photographedValues = new List<int>();
        completionPercentage = 0f;
    }

    public void AddPhotographedValue(int value)
    {
        if (!photographedValues.Contains(value) && objectValuesToPhotograph.Contains(value))
        {
            photographedValues.Add(value);
            UpdateCompletionPercentage();
        }
    }

    private void UpdateCompletionPercentage()
    {
        completionPercentage = (float)photographedValues.Count / objectValuesToPhotograph.Count * 100f;
    }

    public bool IsMissionCompleted()
    {
        return photographedValues.Count >= objectValuesToPhotograph.Count;
    }
}
