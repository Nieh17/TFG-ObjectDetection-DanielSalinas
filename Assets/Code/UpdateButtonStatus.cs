using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpdateButtonStatus : MonoBehaviour
{
    [SerializeField] Button buttonToUpdate;


    public void EnableButton()
    {
        if (buttonToUpdate != null)
        {
            buttonToUpdate.interactable = true;
            Debug.Log("Botón activado.");
        }
        else
        {
            Debug.LogWarning("No se ha asignado un botón al script ToggleUIButton.");
        }
    }

    public void DisableButton()
    {
        if (buttonToUpdate != null)
        {
            buttonToUpdate.interactable = false;
            Debug.Log("Botón desactivado.");
        }
        else
        {
            Debug.LogWarning("No se ha asignado un botón al script ToggleUIButton.");
        }
    }
}
