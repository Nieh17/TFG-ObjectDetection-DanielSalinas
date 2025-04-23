using System.Collections.Generic;

[System.Serializable]
public class Mission
{
    public string missionDescription; // Texto visible para el jugador
    public List<int> objectValuesToPhotograph; // Lista de valores (IDs) que representan objetos

    public Mission(string description, List<int> objectValues)
    {
        missionDescription = description;
        objectValuesToPhotograph = objectValues;
    }
}
