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
        State = GameState.GameOver;
        Time.timeScale = 0f;
        Debug.Log("Game Over — Rimuru was defeated.");
        // Future: show Game Over UI, then call RestartLevel() or LoadMainMenu()
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