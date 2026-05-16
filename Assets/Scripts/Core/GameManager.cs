using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton GameManager. Persists across scenes.
/// Handles game state: Playing, Paused, GameOver.
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
    }

    // ─── State ────────────────────────────────────────────────────────────────

    public enum GameState { Playing, Paused, GameOver }
    public GameState State { get; private set; } = GameState.Playing;

    // ─── Scene names (set these to match your Build Settings) ─────────────────

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string gameScene     = "Level_01";

    // ─── Respawn ──────────────────────────────────────────────────────────────

    [Header("Respawn")]
    [Tooltip("Where the player respawns. Replace with a checkpoint lookup later.")]
    public Vector3 respawnPoint = Vector3.zero;

    /// <summary>Called by checkpoints when the player passes through them.</summary>
    public void SetRespawnPoint(Vector3 point) => respawnPoint = point;

    // ─── Events ───────────────────────────────────────────────────────────────

    /// <summary>Fired when the player dies. UI/menus subscribe to this.</summary>
    public event Action OnGameOverEvent;

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
        if (State == GameState.GameOver) return;
        State = GameState.GameOver;
        Time.timeScale = 0f;
        Debug.Log("Game Over — Rimuru was defeated.");
        OnGameOverEvent?.Invoke();
    }

    /// <summary>Soft-respawn: teleport the player to respawnPoint and resume play.</summary>
    public void RespawnPlayer()
    {
        Time.timeScale = 1f;
        State = GameState.Playing;

        var player = FindAnyObjectByType<PlayerController>();
        if (player != null) player.Respawn(respawnPoint);
        else Debug.LogWarning("RespawnPlayer: no PlayerController found in scene.");
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