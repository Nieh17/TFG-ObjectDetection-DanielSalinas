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
            Texture2D[] sourceImages = Resources.LoadAll<Texture2D>("Photos");

            foreach (Texture2D tex in sourceImages)
            {
                Texture2D readableTex = MakeTextureReadable(tex);

                byte[] bytes = readableTex.EncodeToJPG();
                string destPath = Path.Combine(imageDirectory, tex.name + ".jpg");
                File.WriteAllBytes(destPath, bytes);
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
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
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

    private void ResetGallery()
    {
        string path = Path.Combine(Application.persistentDataPath, "SavedImages");

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    public void ReturnToMainMenu(GameObject objectTOoDeactivate)
    {
        objectTOoDeactivate.SetActive(false);
    }
}
