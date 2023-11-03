using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class CoreGameSystem : MonoBehaviour
{
    public static CoreGameSystem Instance { get; private set; }

    [Header("Game Objects")]
    [SerializeField] private AudioClip gameWonSound;
    [SerializeField] private AudioClip gameLossSound;

    [Header("Wave Settings")] 
    [SerializeField]
    private int numberOfWaves = 4;
    [SerializeField]
    private int initialWaveSize = 8;
    [SerializeField]
    private int waveGrowthPerWave = 2;
    [SerializeField]
    private int spawnProgressGrowth = 15;
    [SerializeField]
    private int waveCooldown = 8;

    // Game constants.
    private const int MaxSpawnProgress = 100;
    private const int MinSpawnProgress = 0;

    // Chance to spawn an enemy each second.
    private int _spawnProgress;
    // How many enemies are left until the wave ends.
    private int _waveSize;
    private int MaxWaveSize => initialWaveSize + (waveGrowthPerWave * _wavesCompleted);

    private bool _waveActive;
    private int _wavesCompleted;
    private float _timeSinceLastWaveTick;
    private int _remainingCooldown;

    // Lists of objects.
    private List<SpawnerManager> _spawnerManagers;
    private List<EnemyManager> _livingEnemies;
    private AudioSource _source;

    // Internal state.
    private bool _gameActive;
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        if (scene.buildIndex == 1)
        {
            StartGame();
        }
        else
        {
            ReturnToMainMenu();
        }
    }

    private int LivingSpawnerCount
    {
        get
        {
            if (_spawnerManagers == null) return 0;
            _spawnerManagers.RemoveAll(i => i == null);
            return _spawnerManagers.Count;
        }
    }
    private int LivingEnemyCount
    {
        get
        {
            if (_livingEnemies == null) return 0;
            _livingEnemies.RemoveAll(i => i == null);
            return _livingEnemies.Count;
        }
    }

    // TODO Manager Waves
    // TODO Game won state
    // TODO Game loss state

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        // // Enforce singleton pattern.
        // CoreGameSystem[] objs = FindObjectsOfType<CoreGameSystem>();
        // if (objs.Length > 1)
        // {
        //     Destroy(gameObject);
        // }
        // // Ensure that this class is persistent between scenes.
        // DontDestroyOnLoad(gameObject);
        //
        // // Ensure that the cursor is visible initially
        // Cursor.visible = true;
    }

    private void ReturnToMainMenu()
    {
        // State lock.
        if (!_gameActive) { return; }
        _gameActive = false;

        Time.timeScale = 0;

        // Show the cursor again.
        Cursor.visible = true;
    }

    private void StartGame()
    {
        // State lock.
        if (_gameActive) { return; }
        _gameActive = true;

        Time.timeScale = 1;

        // Hide the cursor.
        Cursor.visible = false;

        // Fetch all enemies and spawners (portals) that started on the level.
        _livingEnemies = FindObjectsOfType<EnemyManager>().ToList();
        _spawnerManagers = FindObjectsOfType<SpawnerManager>().ToList();
        Debug.Log(_spawnerManagers.Count);
        
        // Fetch audio source reference.
        _source = GetComponent<AudioSource>();

        _spawnProgress = 0;
        _waveSize = MaxWaveSize;
        _wavesCompleted = 0;
        KillsIndicator.Instance.UpdateEnemyCount(MaxWaveSize);
        _waveActive = true;
    }

    public void TriggerGameEnd(bool win)
    {
        // State lock.
        if (!_gameActive) { return; }
        _gameActive = false;

        // Handle generic end-game effects.
        _livingEnemies = null;
        _spawnerManagers = null;

        // Handle win/loss specific end-game effects.
        if (win)
        {
            // TODO Show game won screen.
            // show game won text
            TextIndicator.Instance.SetGameWonVisibility(true);
            // Select game won sound to be played.
            _source.clip = gameWonSound;
        }
        else
        {
            // TODO Show game loss screen.
            TextIndicator.Instance.SetGameOverVisibility(true);
            // Select game loss sound to be played.
            _source.clip = gameLossSound;
        }
        _source.Play();
        // activate the in-game menu
        InputManager.Instance.ActivatePauseMenu();
    }

    private void Update()
    {
        // check if the player is dead or not
        if (_gameActive && PlayerManager.Instance.IsDead)
        {
            TriggerGameEnd(false);
        }
        // if not, then continue with game logic 
        if (LivingSpawnerCount > 0)
        {
            // Perform wave checks once per second.
            _timeSinceLastWaveTick += Time.deltaTime;
            if (_timeSinceLastWaveTick >= 1.0f)
            {
                _timeSinceLastWaveTick = 0f;
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
        if (_waveActive)
        {
            // If we're in a wave, increase SpawnProgress.
            _spawnProgress += spawnProgressGrowth;
            // Each second, attempt to spawn an enemy based on SpawnProgress.
            int roll = Random.Range(MinSpawnProgress, MaxSpawnProgress);
            if (roll >= _spawnProgress)
            {
                // Spawn an enemy with random patrol points at a random portal.
                int rand = Random.Range(0, LivingSpawnerCount);
                int point1 = Random.Range(0, 3);
                int point2 = Random.Range(4, 7);
                int point3 = Random.Range(8, 12);
                Transform[] patrolPoints = {
                    PatrolManager.Instance.patrolPoints[point1],
                    PatrolManager.Instance.patrolPoints[point2],
                    PatrolManager.Instance.patrolPoints[point3],
                };
                _spawnerManagers[rand].SpawnEnemy(patrolPoints);
                // When a spawn occurs, reduce SpawnProgress by 100; reduce WaveSize by 1.
                _spawnProgress -= 100;
                _waveSize -= 1;
                // Re-acquire references to all living enemies. This also purges dead enemies which are now null-references.
                _livingEnemies = FindObjectsOfType<EnemyManager>().ToList();
            }

            // If WaveSize hits 0, end the wave.
            if (_waveSize <= 0)
            {
                _remainingCooldown = waveCooldown;
                _waveActive = false;
            }
        }
        else 
        {
            // If we aren't in a wave, reduce WaveCooldown until it hits 0. If it does, start the next wave - if there are more.
            _remainingCooldown -= 1;
            if (_remainingCooldown <= 0 && _wavesCompleted < numberOfWaves)
            {
                _wavesCompleted++;
                _waveSize = MaxWaveSize;
                KillsIndicator.Instance.UpdateEnemyCount(MaxWaveSize);
                _waveActive = true;
            }
        }
    }
}
