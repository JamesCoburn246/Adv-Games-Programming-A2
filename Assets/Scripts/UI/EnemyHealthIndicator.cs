using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EnemyHealthIndicator : MonoBehaviour
{
    [SerializeField]
    private Slider healthSlider;
    private float _targetValue;

    private void OnEnable()
    {
        healthSlider = GetComponentInChildren<Slider>();
    }

    // Update is called once per frame
    private void Update()
    {
        healthSlider.value = Mathf.MoveTowards(healthSlider.value, _targetValue, 15 * Time.deltaTime);
        transform.rotation = Quaternion.Euler(-CameraController.Instance.transform.eulerAngles.x, CameraController.Instance.transform.eulerAngles.y, 0f);
    }
    
    public void SetMaxHealth(float maxHealth)
    {
        if (healthSlider == null)
        {
            Debug.Log("health slider is null!");
        }
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        _targetValue = maxHealth;
        healthSlider.minValue = 0;
    }


    public void SetCurrentHealth(float currentHealth)
    {
        _targetValue = currentHealth;
    }
    
}
