using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityToggler : MonoBehaviour
{
    [SerializeField] private GameObject gameObjectToToggle;

    public void ToggleVisibility()
    {
        bool active = gameObjectToToggle.activeSelf;
        gameObjectToToggle.SetActive(!active);
    }
}
