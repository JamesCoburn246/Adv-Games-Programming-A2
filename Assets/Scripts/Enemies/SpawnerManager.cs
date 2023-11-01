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
        get => !stats.IsAlive();
        set
        {
            if (value)
            {
                Die();
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

    public void SpawnEnemy(Transform[] patrolPoints)
    {
        // Debug.Log("Spawned enemy!");
        EnemyManager enemy = Instantiate(objectToSpawn).GetComponent<EnemyManager>();
        enemy.patrolPoints = patrolPoints;
        enemy.transform.position = transform.position + offset;
        // Instantiate(objectToSpawn, pos, transform.rotation);
    }

    public void Die()
    {
        // TODO Consider changing portal texture.
        Destroy(gameObject);
    }
}
