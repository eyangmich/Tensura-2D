using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton GameManager. Persists across scenes.
/// Handles game state, player death, respawn point, and scene loading.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────────

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    // ─── Events ───────────────────────────────────────────────────────────────

    // Fires when Rimuru dies (DeathMenu listens to this).
    public static event Action OnGameOver;
    // Fires whenever Rimuru is (re)positioned on a level load — useful later
    // for resetting enemies, refilling HP, or playing a respawn effect.
    public static event Action OnRespawn;

    // ─── State ────────────────────────────────────────────────────────────────

    public enum GameState { Playing, Paused, GameOver }
    public GameState State { get; private set; } = GameState.Playing;

    // ─── Scene names (must match Build Settings exactly) ──────────────────────

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string gameScene     = "Level_01";

    // ─── Respawn ──────────────────────────────────────────────────────────────

    [Header("Respawn")]
    [Tooltip("Where Rimuru reappears. Auto-set to his start position on first "
           + "load, then overridden by checkpoints.")]
    public Vector2 respawnPoint = Vector2.zero;

    // True once a checkpoint has set the point, OR once we've captured
    // Rimuru's starting position on the first level load.
    private bool respawnPointSet = false;

    public void SetRespawnPoint(Vector2 point)
    {
        respawnPoint = point;
        respawnPointSet = true;
    }

    // ─── Pause ────────────────────────────────────────────────────────────────

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (State == GameState.Playing) PauseGame();
            else if (State == GameState.Paused) ResumeGame();
        }
    }

    public void PauseGame()
    {
        State = GameState.Paused;
        Time.timeScale = 0f;
        Debug.Log("Game Paused");
        // Future: show pause UI
    }

    public void ResumeGame()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;
        Debug.Log("Game Resumed");
        // Future: hide pause UI
    }

    // ─── Player Death ─────────────────────────────────────────────────────────

    public void OnPlayerDeath()
    {
        // Guard against double-trigger (e.g. fall + hazard same frame).
        if (State == GameState.GameOver) return;

        State = GameState.GameOver;
        Time.timeScale = 0f;
        Debug.Log("Game Over — Rimuru was defeated.");
        OnGameOver?.Invoke();
    }

    // ─── Scene Load Hook ──────────────────────────────────────────────────────

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != gameScene) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // First load: remember Rimuru's placed position instead of forcing
        // him to (0,0). Leave him exactly where the scene put him.
        if (!respawnPointSet)
        {
            respawnPoint = player.transform.position;
            respawnPointSet = true;
            OnRespawn?.Invoke();
            return;
        }

        // Subsequent loads (after death/restart): move him to the respawn point.
        player.transform.position = respawnPoint;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        OnRespawn?.Invoke();
    }

    // ─── Scene Loading ────────────────────────────────────────────────────────

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        State = GameState.Playing;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        State = GameState.Playing;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        State = GameState.Playing;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            LoadMainMenu(); // No more levels — return to menu
    }
}