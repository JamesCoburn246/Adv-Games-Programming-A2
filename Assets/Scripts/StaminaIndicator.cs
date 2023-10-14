using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaIndicator : MonoBehaviour
{
    public static StaminaIndicator Instance { get; private set; }
    private Slider _staminaSlider;
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
        _staminaSlider = GetComponentInChildren<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        _staminaSlider.value = Mathf.MoveTowards(_staminaSlider.value, _targetValue, 15 * Time.deltaTime);
    }
    
    public void SetMaxStamina(float maxStamina)
    {
        _staminaSlider.maxValue = maxStamina;
        _staminaSlider.value = maxStamina;
        _targetValue = maxStamina;
        _staminaSlider.minValue = 0;
    }


    public void SetCurrentStamina(float currentStamina)
    {
        _targetValue = currentStamina;
    }
    
    
}