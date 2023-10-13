using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maximumMana = 100;
    [SerializeField] private int maximumStamina = 100;

    [Header("HUD Elements")]
    [SerializeField] private GameObject manaBar;
    [SerializeField] private GameObject staminaBar;

    private Slider _manaSlider;
    private Slider _staminaSlider;

    private int _mana;
    private int _stamina;

    private int Mana
    {
        get => _mana;
        set
        {
            _manaSlider.value = value;
            _mana = value;
        }
    }
    private int Stamina
    {
        get => _stamina;
        set
        {
            _staminaSlider.value = value;
            _stamina = value;
        }
    }

    private void OnEnable()
    {
        _manaSlider = manaBar.GetComponent<Slider>();
        _staminaSlider = staminaBar.GetComponent<Slider>();

        if (_manaSlider == null || _staminaSlider == null)
        {
            Debug.Log("Check PlayerEnergy.cs in the inspector. Null reference occurred.");
        } else
        {
            _manaSlider.maxValue = maximumMana;
            _staminaSlider.maxValue = maximumStamina;
            _manaSlider.minValue = 0;
            _staminaSlider.minValue = 0;
        }
    }

   
    /**
     * Call this function when trying to deplete mana and/or stamina to find out if enough exists.
     * If you want to pay the mana and/or stamina cost even when there isn't enough, call DepleteRemainingMana/Stamina on a false return.
     */
    public bool RequestResources(int reqMana, int reqStamina)
    {
        // Check if the player has enough resources.
        if (reqMana > Mana)
            return false;
        if (reqStamina > Stamina)
            return false;

        // Deplete the requested resources and approve the action.
        Mana -= reqMana;
        Stamina -= reqStamina;
        return true;
    }

    public void DepleteRemainingMana()
    {
        Mana = 0;
    }

    public void DepleteRemainingStamina()
    {
        Stamina = 0;
    }
}
