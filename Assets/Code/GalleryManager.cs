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
        imageDirectory = Path.Combine(Application.persistentDataPath, "SavedImages");
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
}
