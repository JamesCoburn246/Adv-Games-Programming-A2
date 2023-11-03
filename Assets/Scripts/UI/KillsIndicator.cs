using System;
using TMPro;
using UnityEngine;

public class KillsIndicator : MonoBehaviour
{
    public static KillsIndicator Instance { get; private set; }
    public TextMeshProUGUI killsText;
    private int _killsCount;
    private int _enemiesCount;

    
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
    
    private void OnEnable()
    {
        _killsCount = 0;
        _enemiesCount = 0;
        killsText = GetComponentInChildren<TextMeshProUGUI>();
        killsText.SetText("x " + _killsCount + " / " + _enemiesCount);
    }

    public void UpdateEnemyCount(int enemyCount)
    {
        _enemiesCount += enemyCount;
        killsText.SetText("x " + _killsCount + " / " + _enemiesCount);
    }

    public void UpdateKillsCount()
    {
        _killsCount++;
        killsText.SetText("x " + _killsCount + " / " + _enemiesCount);
    }
}