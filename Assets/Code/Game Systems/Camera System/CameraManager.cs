using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using NativeCameraNamespace;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEngine.AdaptivePerformance.Provider;

public class CameraManager : MonoBehaviour
{
    [SerializeField] GameObject container;
    [SerializeField] RawImage displayImage;
    [SerializeField] Animator animator;


    private AspectRatioFitter aspectRatioFitter;

    private ModelInference modelInference;

    private string imageDirectory;


    private void Start()
    {
        aspectRatioFitter = displayImage.GetComponent<AspectRatioFitter>();
        modelInference = GetComponent<ModelInference>();

        container.SetActive(false);
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
                        Texture2D readableTexture = MakeTextureReadable(texture);

                        Texture2D modelInputTexture = ResizeTexture(readableTexture, 224, 224);

                        displayImage.texture = modelInputTexture;
                        
                        container.SetActive(true);
                        GetUi();
                        displayImage.gameObject.SetActive(true);

                        if (aspectRatioFitter != null)
                        {
                            aspectRatioFitter.aspectRatio = (float)modelInputTexture.width / modelInputTexture.height;
                        }

                        StartCoroutine(HandlePrediction(modelInputTexture));

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

    Texture2D ResizeTexture(Texture2D originalTexture, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight, 0);
        Graphics.Blit(originalTexture, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D newTexture = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, false);
        newTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        newTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return newTexture;
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

    public void GetUi()
    {
        Debug.Log("Entro al getUI");
        animator.SetTrigger("appear");
    }

    public void CloseUI()
    {
        animator.SetTrigger("disappear");
    }
}
