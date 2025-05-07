using UnityEngine;
using Unity.Barracuda;
using System.Collections.Generic;
using TMPro;
using System.Xml.Schema;
using System.Linq;
using System.Threading.Tasks;

public class ModelInference : MonoBehaviour
{
    public NNModel modelAsset;  // Modelo importado como NNModel
    private IWorker worker;

    //private List<string> classLabels = new List<string> { "gato", "mesa" };

    Dictionary<string, int> classLabelsMap;

    public TextMeshProUGUI predictionText;

    private MissionManager missionManager;

    // Cargar el modelo y preparar el worker
    private void Start()
    {
        var model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);

        classLabelsMap = ClassLabelsManager.GetClassLabelMap();

        missionManager = FindObjectOfType<MissionManager>();
    }

    // Función para predecir usando el modelo cargado
    public async Task<string> Predict(Texture2D inputImage)
    {
        float startTime = Time.realtimeSinceStartup;
        // Convertir la imagen a Tensor
        Tensor tensor = new Tensor(inputImage, channels: 3);

        // Ejecutar la inferencia
        worker.Execute(tensor);

        // Obtener los resultados de la inferencia
        Tensor output = worker.PeekOutput();

        // Obtener el índice de la clase con la mayor probabilidad
        int predictedClass = output.ArgMax()[0];


        float endTime = Time.realtimeSinceStartup;
        Debug.Log("Total time Prediction: "+ (endTime-startTime));

        Debug.Log("PREDICTED CLASS: "+predictedClass);

        // Notificar al MissionManager
        missionManager?.RegisterPhotographedObject(predictedClass);


        // Obtener el nombre de la clase correspondiente
        string predictedClassName = classLabelsMap.FirstOrDefault(kvp => kvp.Value == predictedClass).Key;

        string localizedPrediction = await LocalizationManager.GetLearningLocalizedString(predictedClassName);

        // Mostrar la predicción
        Debug.Log("Prediction: "+predictedClassName);

        Debug.Log("Prediction Localized: " + localizedPrediction);


        predictionText.text = localizedPrediction;
        predictionText.gameObject.SetActive(true);

        tensor.Dispose();

        return predictedClassName;
    }

    // Limpiar cuando ya no se necesita el worker
    private void OnDestroy()
    {
        worker.Dispose();
    }
}
