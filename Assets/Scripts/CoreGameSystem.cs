using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class CoreGameSystem : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] private AudioClip gameWonSound;
    [SerializeField] private AudioClip gameLossSound;

    [Header("Settings")]
    [SerializeField] private int enemiesPerWave;

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

    private void Update()
    {
        if (LivingSpawnerCount > 0)
        {
            // TODO Handle spawning enemies. Waves. Et cetera.
        }
        else
        {
            if (LivingEnemyCount <= 0)
            {                                                         
                TriggerGameEnd(true);
            }
        }
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
}
