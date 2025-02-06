using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using NativeCameraNamespace;

public class CameraManager : MonoBehaviour
{
    public RawImage displayImage;
    private AspectRatioFitter aspectRatioFitter;

    private ModelInference modelInference;

    private void Start()
    {
        aspectRatioFitter = displayImage.GetComponent<AspectRatioFitter>();
        modelInference = GetComponent<ModelInference>();

        displayImage.gameObject.SetActive(false);
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

                        modelInference.Predict(texture);
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
}
