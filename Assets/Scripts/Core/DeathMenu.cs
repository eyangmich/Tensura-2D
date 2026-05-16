using UnityEngine;

/// <summary>
/// Shows a death menu panel when the player dies.
/// Hook the two public methods up to the Restart and Main Menu buttons' OnClick events.
/// </summary>
public class DeathMenu : MonoBehaviour
{
    [Tooltip("Root GameObject of the death menu UI. Disabled at start, enabled on death.")]
    public GameObject menuPanel;

    void Awake()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOverEvent += Show;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOverEvent -= Show;
    }

    void Show()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    // ─── Button hooks ─────────────────────────────────────────────────────────

    public void OnRestartClicked()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        GameManager.Instance?.RespawnPlayer();
    }

    public void OnMainMenuClicked()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        GameManager.Instance?.LoadMainMenu();
    }
}
