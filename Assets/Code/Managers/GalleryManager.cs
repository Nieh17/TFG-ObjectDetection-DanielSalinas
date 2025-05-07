using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class GalleryManager : MonoBehaviour
{
    public GameObject gallery;
    public Transform galleryContainer;
    public GameObject imagePrefab;

    private string imageDirectory;

    public ScrollRect scrollRect;

    private void Start()
    {
        ResetGallery();


        imageDirectory = Path.Combine(Application.persistentDataPath, "SavedImages");


        if (!Directory.Exists(imageDirectory))
        {
            Directory.CreateDirectory(imageDirectory);
        }

        string[] existingImages = Directory.GetFiles(imageDirectory, "*.jpg");

        if (existingImages.Length == 0)
        {
            string sourceDirectory = Path.Combine(Application.dataPath, "Photos");

            if (Directory.Exists(sourceDirectory))
            {
                string[] sourceImages = Directory.GetFiles(sourceDirectory, "*.jpg");

                foreach (string sourcePath in sourceImages)
                {
                    string fileName = Path.GetFileName(sourcePath);
                    string destPath = Path.Combine(imageDirectory, fileName);

                    File.Copy(sourcePath, destPath, true);
                }
            }
            else
            {
                Debug.LogWarning("No se encontró la carpeta Assets/Photos.");
            }
        }
    }

    public void LoadGallery()
    {
        gallery.SetActive(true);

        foreach (Transform child in galleryContainer)
        {
            Destroy(child.gameObject);
        }

        string[] imagePaths = Directory.GetFiles(imageDirectory, "*.jpg");

        foreach (string path in imagePaths)
        {
            GameObject newCard = Instantiate(imagePrefab, galleryContainer);
            RawImage rawImage = newCard.transform.Find("CardImage").GetComponent<RawImage>();

            Texture2D texture = LoadTexture(path);
            rawImage.texture = texture;

            TextMeshProUGUI textComponent = newCard.transform.Find("CardText").GetComponent<TextMeshProUGUI>();

            string fileName = Path.GetFileNameWithoutExtension(path);
            textComponent.text = fileName;
            
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(galleryContainer.GetComponent<RectTransform>());
        scrollRect.verticalNormalizedPosition = 1;
    }

    private Texture2D LoadTexture(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(75, 75);
        texture.LoadImage(fileData);
        return texture;
    }

    private void ResetGallery()
    {
        string path = Path.Combine(Application.persistentDataPath, "SavedImages");

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
