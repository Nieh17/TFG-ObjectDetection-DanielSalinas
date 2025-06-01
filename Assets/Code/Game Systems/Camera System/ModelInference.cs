using UnityEngine;
using Unity.Barracuda;
using System.Collections.Generic;
using TMPro;
using System.Xml.Schema;
using System.Linq;
using System.Threading.Tasks;

public class ModelInference : MonoBehaviour
{
    public NNModel modelAsset;
    private IWorker worker;

    Dictionary<string, int> classLabelsMap;

    public TextMeshProUGUI predictionText;


    public static event System.Action<int> OnObjectPredicted;


    private void Start()
    {
        var model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);

        classLabelsMap = ClassLabelsManager.GetClassLabelMap();

    }

    // Función para predecir usando el modelo cargado
    public async Task<string> Predict(Texture2D inputImage)
    {

        Color32[] pixels = inputImage.GetPixels32();
        float[] preprocessedPixels = new float[inputImage.width * inputImage.height * 3];

        for (int i = 0; i < pixels.Length; i++)
        {
            // Escalar de [0, 255] a [-1, 1]
            preprocessedPixels[i * 3 + 0] = (pixels[i].r / 127.5f) - 1.0f;
            preprocessedPixels[i * 3 + 1] = (pixels[i].g / 127.5f) - 1.0f;
            preprocessedPixels[i * 3 + 2] = (pixels[i].b / 127.5f) - 1.0f;
        }


        // Convertir la imagen a Tensor
        //Tensor tensor = new Tensor(inputImage, channels: 3);
        Tensor tensor = new Tensor(new TensorShape(1, inputImage.height, inputImage.width, 3), preprocessedPixels);

        // Ejecutar la inferencia
        worker.Execute(tensor);

        // Obtener los resultados de la inferencia
        Tensor output = worker.PeekOutput();

        // Obtener el índice de la clase con la mayor probabilidad
        int predictedClass = output.ArgMax()[0];

        Debug.Log("Predicted class: "+predictedClass);
        // Notificar al MissionManager
        OnObjectPredicted?.Invoke(predictedClass);


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

    private void OnDestroy()
    {
        worker.Dispose();
    }
}
