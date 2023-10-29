using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] 
    private float maxHealth;

    private EnemyManager enemy;

    // public float maxDamage;
    
    // ------ current stats ---------- //
    private float _currentHealth;

    public EnemyHealthIndicator healthIndicator;

    // Start is called before the first frame update
    private void Start()
    {
        _currentHealth = maxHealth;
        healthIndicator = GetComponentInChildren<EnemyHealthIndicator>();
        enemy = GetComponent<EnemyManager>();
        healthIndicator.SetMaxHealth(_currentHealth);
    }

    // Update is called once per frame
    private void Update()
    {

    }
    
    // --------------- Health Stuff ------------------- //

    public void DepleteHealth(float value)
    {
        if (enemy.IsDead) return;
        _currentHealth -= value;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);
        healthIndicator.SetCurrentHealth(_currentHealth);
        if (_currentHealth <= 0)
        {
            enemy.IsDead = true;
        }
    }
}
