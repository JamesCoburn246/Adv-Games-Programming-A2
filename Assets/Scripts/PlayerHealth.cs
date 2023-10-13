using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maximumHealth = 100;
    [SerializeField] private int maximumArmour = 100;

    [Header("HUD Elements")]
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject armourBar;

    private Slider _healthSlider;
    private Slider _armourSlider;

    private int _health;
    private int _armour;

    private int Health
    {
        get => _health;
        set
        {
            _healthSlider.value = value;
            _health = value;
        }
    }
    private int Armour
    {
        get => _armour;
        set
        {
            _armourSlider.value = value;
            _armour = value;
        }
    }

    private void OnEnable()
    {
        _healthSlider = healthBar.GetComponent<Slider>();
        _armourSlider = armourBar.GetComponent<Slider>();

        if (_healthSlider == null || _armourSlider == null)
        {
            Debug.Log("Check PlayerHealth.cs in the inspector. Null reference occurred.");
        } else
        {
            _healthSlider.maxValue = maximumHealth;
            _armourSlider.maxValue = maximumArmour;
            _healthSlider.minValue = 0;
            _armourSlider.minValue = 0;
        }
    }

    private void Update()
    {

    }
}
