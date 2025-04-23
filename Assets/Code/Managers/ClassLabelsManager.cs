using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ClassLabelsManager : MonoBehaviour
{

    public static Dictionary<string, int> classLabelMap;

    private void Awake()
    {
        TextAsset classLabelsJson = Resources.Load<TextAsset>("class_labels");

        if (classLabelsJson != null)
        {
            LabelData labelData = JsonUtility.FromJson<LabelData>(classLabelsJson.text);

            classLabelMap = new Dictionary<string, int>();
            foreach (var label in labelData.labels)
            {
                classLabelMap[label.key] = label.value;
            }
        }
        else
        {
            Debug.LogError("El archivo 'class_labels' no se encuentra en la carpeta Resources.");
        }
    }

    public static Dictionary<string, int> GetClassLabelMap()
    {
        return classLabelMap;
    }


    [System.Serializable]
    public class LabelData
    {
        public List<LabelItem> labels;
    }

    [System.Serializable]
    public class LabelItem
    {
        public string key;
        public int value;
    }
}
