using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class HealthIndicator : MonoBehaviour
{
    public static HealthIndicator Instance { get; private set; }
    private Slider _healthSlider;
    private PostProcessingManager postProcessingManager;
    private float _targetValue;

    private float currentMaxHealth = 100;

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
        _healthSlider = GetComponentInChildren<Slider>();
        postProcessingManager = FindObjectOfType<PostProcessingManager>();
        if (postProcessingManager == null)
        {
            Debug.Log("Post processing manager was not found.");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        _healthSlider.value = Mathf.MoveTowards(_healthSlider.value, _targetValue, 15 * Time.deltaTime);
    }
    
    public void SetMaxHealth(float maxHealth)
    {
        currentMaxHealth = maxHealth;
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
        _targetValue = maxHealth;
        _healthSlider.minValue = 0;
    }


    public void SetCurrentHealth(float currentHealth)
    {
        _targetValue = currentHealth;
        postProcessingManager.setVignetteIntensity(currentHealth / currentMaxHealth);
    }
    
    
}
