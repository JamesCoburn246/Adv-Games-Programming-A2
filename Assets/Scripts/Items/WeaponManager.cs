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
                    if (PlayerManager.Instance.IsDead)
                    {
                        if (!PlayerManager.Instance.StateManager.CheckState(PlayerManager.Instance.StateManager.deathState))
                        {
                            PlayerManager.Instance.StateManager.SwitchState(PlayerManager.Instance.StateManager.deathState);
                        }
                    }
                }
            } 
            else if (other.CompareTag("EnemyHitbox"))
            {
                Debug.Log("Hit Enemy!");
                EnemyManager enemy = other.GetComponentInParent<EnemyManager>();
                EnemyStats enemyStats = other.GetComponentInParent<EnemyStats>();
                if (enemy != null && enemyStats != null)
                {
                    enemyStats.DepleteHealth(weaponDamage);
                    if (enemy.IsDead)
                    {
                        if (!enemy.StateManager.CheckState(enemy.StateManager.deathState))
                        {
                            enemy.StateManager.SwitchState(enemy.StateManager.deathState);
                        }
                    }
                    else
                    {
                        if (!enemy.StateManager.CheckState(enemy.StateManager.damageState))
                        {
                            enemy.StateManager.SwitchState(enemy.StateManager.damageState);
                        }
                    }
                }
            }
            else if (other.CompareTag("SpawnerHitbow"))
            {
                Debug.Log("Hit Spawner!");
                SpawnerManager spawnerManager = other.GetComponentInParent<SpawnerManager>();
            }
        }
    }
}
