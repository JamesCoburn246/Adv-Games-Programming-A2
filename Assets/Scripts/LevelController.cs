using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class LevelController : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] private string _entryScene;
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
        LevelController[] objs = FindObjectsOfType<LevelController>();
        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        // Ensure that this class is persistent between scenes.
        DontDestroyOnLoad(this.gameObject);
    }

    public void StartGame()
    {
        // State lock.
        if (gameActive) { return; }
        gameActive = true;

        // TODO Initialize game.
        SceneManager.LoadScene(_entryScene);

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

            // Play game won sound.
            source.clip = gameWonSound;
            source.Play();
        }
        else
        {
            // TODO Show game loss screen.

            // Play game loss sound.
            source.clip = gameLossSound;
            source.Play();
        }
    }
}
