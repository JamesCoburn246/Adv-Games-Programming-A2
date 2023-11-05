using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    private bool _staminaReplenishLock;

    // ---------- max stats ------------- //

    [SerializeField]
    private float maxHealth;
    [SerializeField]
    private float maxStamina;
    [SerializeField]
    private float staminaRegen;
    
    // ------ current stats ---------- //
    private float _currentHealth;
    private float _currentStamina;

    public HealthIndicator healthIndicator;
    public StaminaIndicator staminaIndicator;

    // Start is called before the first frame update
    private void Start()
    {
        _currentHealth = maxHealth;
        _currentStamina = maxStamina;
        healthIndicator = HealthIndicator.Instance;
        staminaIndicator = StaminaIndicator.Instance;
        healthIndicator.SetMaxHealth(_currentHealth);
        staminaIndicator.SetMaxStamina(_currentStamina);
    }

    // Update is called once per frame
    private void Update()
    {
        ReplenishStamina();

    }
    
    // --------------- Health Stuff ------------------- //

    public void DepleteHealth(float value)
    {
        if (PlayerManager.Instance.IsDead) return;
        _currentHealth -= value;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);
        healthIndicator.SetCurrentHealth(_currentHealth);
        if (_currentHealth <= 0)
        {
            PlayerManager.Instance.IsDead = true;
        }
    }
    
    // --------------- Stamina Stuff ------------------- //

    public bool HasStamina()
    {
        return _currentStamina > 0;
    }

    private void IncreaseStamina(float value)
    {
        _currentStamina += value;
        _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina);
        staminaIndicator.SetCurrentStamina(_currentStamina);
    }


    public void DepleteStamina(float value)
    {
        _currentStamina -= value;
        _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina);
        staminaIndicator.SetCurrentStamina(_currentStamina);
    }

    private void ReplenishStamina()
    {
        if (_currentStamina < maxStamina && !_staminaReplenishLock)
        {
            _staminaReplenishLock = true;
            StartCoroutine(ReplenishStaminaTask());
        }
        else
        {
            StopCoroutine(ReplenishStaminaTask());
        }
    }
    
    private IEnumerator<WaitForSeconds> ReplenishStaminaTask()
    {
        // wait for 10 seconds
        yield return new WaitForSeconds(10f);
        // then increase the stamina by 25 pts (to be changed)
        IncreaseStamina(staminaRegen);
        // release the lock because the coroutine has ended
        _staminaReplenishLock = false;
    }
}
