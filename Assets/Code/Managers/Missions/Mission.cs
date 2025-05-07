using System.Collections.Generic;

[System.Serializable]
public class Mission
{
    public string missionDescription;
    public List<int> objectValuesToPhotograph;

    public Mission(string description, List<int> objectValues)
    {
        missionDescription = description;
        objectValuesToPhotograph = objectValues;
    }
}
