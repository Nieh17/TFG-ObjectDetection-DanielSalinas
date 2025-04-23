using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using NativeCameraNamespace;
using System.IO;
using System.Collections.Generic;
using System;

public class CameraManager : MonoBehaviour
{
    public RawImage displayImage;
    private AspectRatioFitter aspectRatioFitter;

    private ModelInference modelInference;

    private string imageDirectory;


    private void Start()
    {
        aspectRatioFitter = displayImage.GetComponent<AspectRatioFitter>();
        modelInference = GetComponent<ModelInference>();

        displayImage.gameObject.SetActive(false);

        imageDirectory = Path.Combine(Application.persistentDataPath, "SavedImages");
        if (!Directory.Exists(imageDirectory))
        {
            Directory.CreateDirectory(imageDirectory);
        }
    }

    public void OpenCamera()
    {
        StartCoroutine(CapturePhoto());
    }

    private IEnumerator CapturePhoto()
    { 
        if (!NativeCamera.IsCameraBusy())
        {
            yield return new WaitForEndOfFrame();

            NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {

                    Texture2D texture = NativeCamera.LoadImageAtPath(path, 1024);
                    if (texture != null)
                    {
                        displayImage.texture = texture;
                        displayImage.gameObject.SetActive(true);

                        if (aspectRatioFitter != null)
                        {
                            aspectRatioFitter.aspectRatio = (float)texture.width / texture.height;
                        }

                        StartCoroutine(HandlePrediction(texture));

                    }
                    else
                    {
                        Debug.LogError("No se pudo cargar la imagen");
                    }
                }
                else
                {
                    Debug.LogError("No se recibió ninguna imagen");
                }
            }, 1024);

            Debug.Log("Permiso de cámara: " + permission);
        }
    }

    private IEnumerator HandlePrediction(Texture2D texture)
    {
        var predictionTask = modelInference.Predict(texture);

        while (!predictionTask.IsCompleted)
            yield return null;

        string predictedClass = predictionTask.Result;
        SaveImage(texture, predictedClass);
    }


    private void SaveImage(Texture2D image, string className)
    {
        string fileName = className + ".jpg";
        string filePath = Path.Combine(imageDirectory, fileName);

        Texture2D readableTexture = MakeTextureReadable(image);

        byte[] bytes = readableTexture.EncodeToJPG(50);
        File.WriteAllBytes(filePath, bytes);

        Destroy(readableTexture);

        Debug.Log("Imagen guardada en: " + filePath);
    }

    private Texture2D MakeTextureReadable(Texture2D sourceTexture)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(sourceTexture, renderTex);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;

        Texture2D readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        readableTexture.ReadPixels(new Rect(0, 0, sourceTexture.width, sourceTexture.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);

        return readableTexture;
    }


    public string[] GetSavedImages()
    {
        return Directory.GetFiles(imageDirectory, "*.jpg"); // Devuelve las imágenes guardadas
    }
}
