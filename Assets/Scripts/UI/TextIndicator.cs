using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextIndicator : MonoBehaviour
{
    public static TextIndicator Instance { get; private set; }

    private GameObject gameOverText, gameWonText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        gameOverText = transform.GetChild(0).gameObject;
        gameWonText = transform.GetChild(1).gameObject;
        gameOverText.SetActive(false);
        gameWonText.SetActive(false);
    }

    public void SetGameOverVisibility(bool visibility)
    {
        gameOverText.SetActive(visibility);
    }

}
