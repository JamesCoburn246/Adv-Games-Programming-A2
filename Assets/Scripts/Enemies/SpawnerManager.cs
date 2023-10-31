using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class SpawnerManager : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToSpawn;
    [SerializeField]
    private Vector3 offset;

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

    public void SpawnEnemy()
    {
        Vector3 pos = this.transform.position + offset;
        Instantiate(objectToSpawn, pos, this.transform.rotation);
    }

    public void Die()
    {
        // TODO Consider changing portal texture.
        Destroy(gameObject);
    }
}
