using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] 
    private float maxHealth;
    [SerializeField]
    public EnemyHealthIndicator healthIndicator;

    // --- Current stats --- //

    private float _currentHealth;
    private bool _isAlive = true;

    private void Start()
    {
        _currentHealth = maxHealth;
        // Fetch indicator if one isn't specified.
        if (healthIndicator == null)
        {
            healthIndicator = GetComponentInChildren<EnemyHealthIndicator>();
            healthIndicator.SetMaxHealth(_currentHealth);
        }
    }

    // --- Health Getter Setter --- //

    public float GetCurrentHealth()
    {
        return _currentHealth;
    }

    public void DepleteHealth(float value)
    {
        if (!_isAlive) return;

        _currentHealth -= value;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);
        healthIndicator.SetCurrentHealth(_currentHealth);
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    // --- Misc --- //

    public bool IsAlive()
    {
        return _isAlive;
    }

    public bool IsDead()
    {
        return !_isAlive;
    }

    public void Revive()
    {
        if (IsAlive()) { return; }
        
        _isAlive = true;
        _currentHealth = maxHealth;
    }

    public void Die()
    {
        if (IsDead()) { return; }

        _isAlive = false;
        _currentHealth = 0;
    }
}
