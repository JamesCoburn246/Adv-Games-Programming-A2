using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthIndicator : MonoBehaviour
{
    public static HealthIndicator Instance { get; private set; }
    private Slider _healthSlider;
    private float _targetValue;

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

    // Start is called before the first frame update
    void Start()
    {
        _healthSlider = GetComponentInChildren<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        _healthSlider.value = Mathf.MoveTowards(_healthSlider.value, _targetValue, 15 * Time.deltaTime);
    }
    
    public void SetMaxHealth(float maxHealth)
    {
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _targetValue = maxHealth;
        _healthSlider.minValue = 0;
    }


    public void SetCurrentHealth(float currentHealth)
    {
        _targetValue = currentHealth;
    }
    
    
}
