using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class SpawnerManager : MonoBehaviour
{
    private EnemyStats stats;

    // This state is now handled by the EnemyStats class.
    public bool IsDead
    {
        get
        {
            return !stats.IsAlive();
        }
        set
        {
            if (value)
            {
                this.Die();
                stats.Die();
            }
            else
            {
                stats.Revive();
            }
        }
    }

    private void Start()
    {
        stats = GetComponent<EnemyStats>();

    }

    private void Update()
    {

    }


    public void Die()
    {
        // TODO Consider changing portal texture.
        Destroy(gameObject);
    }
}
