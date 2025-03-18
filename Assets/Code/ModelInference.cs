using UnityEngine;
using Unity.Barracuda;
using System.Collections.Generic;
using TMPro;
using System.Xml.Schema;

public class ModelInference : MonoBehaviour
{
    public NNModel modelAsset;  // Modelo importado como NNModel
    private IWorker worker;

    private List<string> classLabels = new List<string> { "gato", "mesa" };

    public TextMeshProUGUI predictionText;

    // Cargar el modelo y preparar el worker
    private void Start()
    {
        var model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
    }

    // Función para predecir usando el modelo cargado
    public void Predict(Texture2D inputImage)
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

        // Obtener el nombre de la clase correspondiente
        string predictedClassName = classLabels[predictedClass];

        // Mostrar la predicción
        Debug.Log("Prediction: "+predictedClassName);
        predictionText.text = predictedClassName;

        tensor.Dispose();
    }

    // Limpiar cuando ya no se necesita el worker
    private void OnDestroy()
    {
        worker.Dispose();
    }
}
