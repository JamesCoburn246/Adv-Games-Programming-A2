using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class CoreGameSystem : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] private GameObject[] spawners;
    [SerializeField] private AudioClip gameWonSound;
    [SerializeField] private AudioClip gameLossSound;

    [Header("Settings")]
    [SerializeField] private int enemiesPerWave;

    // Internal objects.
    private bool gameActive = false;
    private int LivingSpawners
    {
        get { return spawners.Length; }
    }
    private GameObject[] livingEnemies;
    private int LivingEnemes
    {
        get { return livingEnemies.Length; }
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
        if (LivingSpawners > 0)
        {
            // TODO Handle spawning enemies. Waves. Et cetera.
        }
        else
        {
            if (LivingEnemes <= 0)
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

        // Fetch spawners (portals).
        //spawners = GameObject.FindObjectOfType<>();
    }

    public void TriggerGameEnd(bool win)
    {
        // State lock.
        if (!gameActive) { return; }
        gameActive = false;

        // Handle generic end-game effects.
        AudioSource source = GetComponent<AudioSource>();
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
