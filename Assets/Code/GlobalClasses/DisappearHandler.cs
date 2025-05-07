using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class DisappearHandler : MonoBehaviour
{
    [SerializeField] GameObject parentContainer;
    [SerializeField] RawImage imagePrediction;
    public void OnDisappearEnd()
    {
        imagePrediction.texture = null;
        imagePrediction.gameObject.SetActive(false);
        parentContainer.SetActive(false);
    }
}
