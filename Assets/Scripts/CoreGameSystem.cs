using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class CoreGameSystem : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] private AudioClip gameWonSound;
    [SerializeField] private AudioClip gameLossSound;

    [Header("Wave Settings")]
    [SerializeField]
    private int initialWaveSize = 8;
    [SerializeField]
    private int waveGrowthPerWave = 2;
    [SerializeField]
    private int spawnProgressGrowth = 15;
    [SerializeField]
    private int waveCooldown = 8;

    // Game constants.
    private const int maxSpawnProgress = 100;
    private const int minSpawnProgress = 0;

    // Chance to spawn an enemy each second.
    private int spawnProgress;
    // How many enemies are left until the wave ends.
    private int waveSize;
    private int MaxWaveSize
    {
        get
        {
            return initialWaveSize + (waveGrowthPerWave * wavesCompleted);
        }
        set { }
    }

    private bool waveActive;
    private int wavesCompleted;
    private float timeSinceLastWaveTick;
    private int remainingCooldown;

    // Lists of objects.
    private List<SpawnerManager> spawnerManagers;
    private List<EnemyManager> livingEnemies;
    private AudioSource source;

    // Internal state.
    private bool gameActive = false;

    private int LivingSpawnerCount
    {
        get
        {
            spawnerManagers.RemoveAll(i => i == null);
            return spawnerManagers.Count;
        }
    }
    private int LivingEnemyCount
    {
        get
        {
            livingEnemies.RemoveAll(i => i == null);
            return livingEnemies.Count;
        }
    }

    // TODO Manager Waves
    // TODO Game won state
    // TODO Game loss state

    private void Awake()
    {
        // Enforce singleton pattern.
        CoreGameSystem[] objs = FindObjectsOfType<CoreGameSystem>();
        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        // Ensure that this class is persistent between scenes.
        DontDestroyOnLoad(this.gameObject);

        // Ensure that the cursor is visible initially
        Cursor.visible = true;
    }

    public void ReturnToMainMenu()
    {
        // State lock.
        if (!gameActive) { return; }
        gameActive = false;

        SceneManager.LoadScene(0);
        Time.timeScale = 0;

        // Show the cursor again.
        Cursor.visible = true;
    }

    public void StartGame()
    {
        // State lock.
        if (gameActive) { return; }
        gameActive = true;

        SceneManager.LoadScene(1);
        Time.timeScale = 1;

        // Hide the cursor.
        Cursor.visible = false;

        // Fetch all enemies and spawners (portals) that started on the level.
        livingEnemies = FindObjectsOfType<EnemyManager>().ToList();
        spawnerManagers = FindObjectsOfType<SpawnerManager>().ToList();
        // Fetch audio source reference.
        source = GetComponent<AudioSource>();

        spawnProgress = maxSpawnProgress;
        waveActive = true;
    }

    public void TriggerGameEnd(bool win)
    {
        // State lock.
        if (!gameActive) { return; }
        gameActive = false;

        // Handle generic end-game effects.
        livingEnemies = null;
        spawnerManagers = null;

        // Handle win/loss specific end-game effects.
        if (win)
        {
            // TODO Show game won screen.

            // Select game won sound to be played.
            source.clip = gameWonSound;
        }
        else
        {
            // TODO Show game loss screen.

            // Select game loss sound to be played.
            source.clip = gameLossSound;
        }
        source.Play();
    }

    private void Update()
    {
        if (LivingSpawnerCount > 0)
        {
            // Perform wave checks once per second.
            timeSinceLastWaveTick += Time.deltaTime;
            if (timeSinceLastWaveTick >= 1.0f)
            {
                timeSinceLastWaveTick -= 1.0f;
                DoWaveChecks();
            }
        }
        else
        {
            // If there are no more spawner and no more enemies, the player wins.
            if (LivingEnemyCount <= 0)
            {
                TriggerGameEnd(true);
            }
        }
    }

    // This will only be called once per second.
    private void DoWaveChecks()
    {
        if (waveActive)
        {
            // If we're in a wave, increase SpawnProgress.
            spawnProgress += spawnProgressGrowth;
            // Each second, attempt to spawn an enemy based on SpawnProgress.
            int roll = Random.Range(minSpawnProgress, maxSpawnProgress - 1);
            if (roll >= spawnProgress)
            {
                // Spawn an enemy at a random portal.
                int rand = Random.Range(0, LivingSpawnerCount - 1);
                spawnerManagers[rand].SpawnEnemy();
                // When a spawn occurs, reduce SpawnProgress by 100; reduce WaveSize by 1.
                spawnProgress -= 100;
                waveSize -= 1;
                // Re-acquire references to all living enemies. This also purges dead enemies which are now null-references.
                livingEnemies = FindObjectsOfType<EnemyManager>().ToList();
            }

            // If WaveSize hits 0, end the wave.
            if (waveSize <= 0)
            {
                remainingCooldown = waveCooldown;
                waveActive = false;
            }
        }
        else
        {
            // If we aren't in a wave, reduce WaveCooldown until it hits 0. If it does, start a wave.
            remainingCooldown -= 1;
            if (remainingCooldown <= 0)
            {
                waveSize = MaxWaveSize;
                waveActive = true;
            }
        }
    }
}
