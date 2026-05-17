using UnityEngine;

/// <summary>
/// Shows a panel when Rimuru dies. Wire the Restart and Main Menu
/// buttons to OnRestartClicked / OnMainMenuClicked in the inspector.
/// </summary>
public class DeathMenu : MonoBehaviour
{
    [Tooltip("Root panel to enable on death. Should start disabled.")]
    public GameObject panel;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    void OnEnable()
    {
        GameManager.OnGameOver += Show;
        GameManager.OnRespawn  += Hide;
    }

    void OnDisable()
    {
        GameManager.OnGameOver -= Show;
        GameManager.OnRespawn  -= Hide;
    }

    void Show()
    {
        if (panel != null) panel.SetActive(true);
    }

    void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    // ─── Button hooks (wire via Button.onClick in the inspector) ──────────────

    public void OnRestartClicked()
    {
        GameManager.Instance?.RestartLevel();
    }

    public void OnMainMenuClicked()
    {
        GameManager.Instance?.LoadMainMenu();
    }
}
