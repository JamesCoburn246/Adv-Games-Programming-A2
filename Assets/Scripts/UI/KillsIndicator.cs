using System;
using TMPro;
using UnityEngine;

public class KillsIndicator : MonoBehaviour
{
    public static KillsIndicator Instance { get; private set; }
    public TextMeshProUGUI killsText;
    public int killsCount;

    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        killsCount = 0;
        killsText = GetComponentInChildren<TextMeshProUGUI>();
        killsText.SetText("x " + killsCount);
    }

    public void IncrementCount()
    {
        killsCount++;
        killsText.SetText("x " + killsCount);
    }
}