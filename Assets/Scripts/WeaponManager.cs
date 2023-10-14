using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private Collider _damageHitbox;
    public float weaponDamage;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _damageHitbox = GetComponent<Collider>();
        _damageHitbox.gameObject.SetActive(true);
        _damageHitbox.isTrigger = true;
        _damageHitbox.enabled = false;
    }

    public void SetDamage(float damage)
    {
        weaponDamage = damage;
    }

    public void EnableDamage()
    {
        _damageHitbox.enabled = true;
    }

    public void DisableDamage()
    {
        _damageHitbox.enabled = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (_damageHitbox.enabled)
        {
            if (other.CompareTag("PlayerHitbox"))
            {
                Debug.Log("Hit Player!");
                PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.DepleteHealth(weaponDamage);
                }
            } 
            if (other.CompareTag("EnemyHitbox"))
            {
                Debug.Log("Hit Enemy!");
                // EnemyStats enemyStats = other.GetComponentInParent<EnemyStats>();
                // if (enemyStats != null)
                // {
                //     enemyStats.TakeDamage(weaponDamage);
                // }
            } 
        }
    }
}
