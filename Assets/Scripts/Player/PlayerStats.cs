using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public int maxHP   = 100;
    public int maxMana = 15;

    [Header("Mana Regen")]
    [Tooltip("Seconds after last attack before regen starts.")]
    public float regenDelay    = 2f;
    [Tooltip("Mana restored per tick.")]
    public int   regenAmount   = 1;
    [Tooltip("Seconds between each regen tick.")]
    public float regenInterval = 2f;

    // Runtime values
    public int CurrentHP   { get; private set; }
    public int CurrentMana { get; private set; }

    // Events — hook these up to your UI
    public UnityEvent<int, int> onHPChanged;   // (current, max)
    public UnityEvent<int, int> onManaChanged; // (current, max)
    public UnityEvent           onDeath;

    private Coroutine regenCoroutine;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    void Awake()
    {
        CurrentHP   = maxHP;
        CurrentMana = maxMana;
    }

    // ─── HP ───────────────────────────────────────────────────────────────────

    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        onHPChanged?.Invoke(CurrentHP, maxHP);

        if (CurrentHP <= 0) Die();
    }

    public void HealHP(int amount)
    {
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        onHPChanged?.Invoke(CurrentHP, maxHP);
    }

    public void Kill()
    {
        if (CurrentHP <= 0) return;
        CurrentHP = 0;
        onHPChanged?.Invoke(CurrentHP, maxHP);
        Die();
    }

    public void Revive()
    {
        CurrentHP   = maxHP;
        CurrentMana = maxMana;
        onHPChanged?.Invoke(CurrentHP, maxHP);
        onManaChanged?.Invoke(CurrentMana, maxMana);

        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
    }

    void Die()
    {
        Debug.Log("Rimuru has fallen!");
        onDeath?.Invoke();
        // GameManager will handle scene reload / game over screen
        GameManager.Instance?.OnPlayerDeath();
    }

    // ─── Mana ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to spend <paramref name="cost"/> mana.
    /// Returns true and deducts mana if affordable; false otherwise.
    /// </summary>
    public bool SpendMana(int cost)
    {
        if (CurrentMana < cost) return false;

        CurrentMana -= cost;
        onManaChanged?.Invoke(CurrentMana, maxMana);

        // Restart regen delay every time mana is spent
        if (regenCoroutine != null) StopCoroutine(regenCoroutine);
        regenCoroutine = StartCoroutine(RegenRoutine());

        return true;
    }

    IEnumerator RegenRoutine()
    {
        // Wait before starting regen
        yield return new WaitForSeconds(regenDelay);

        while (CurrentMana < maxMana)
        {
            CurrentMana = Mathf.Min(maxMana, CurrentMana + regenAmount);
            onManaChanged?.Invoke(CurrentMana, maxMana);
            yield return new WaitForSeconds(regenInterval);
        }

        regenCoroutine = null;
    }

    // ─── Debug ────────────────────────────────────────────────────────────────

    GUIStyle debugStyle;

    void OnGUI()
    {
#if UNITY_EDITOR
        if (debugStyle == null)
            debugStyle = new GUIStyle(GUI.skin.label) { fontSize = 36 };

        Vector3 p = transform.position;
        GUI.Label(new Rect(10, 10,  600, 50), $"HP:   {CurrentHP} / {maxHP}",                  debugStyle);
        GUI.Label(new Rect(10, 60,  600, 50), $"Mana: {CurrentMana} / {maxMana}",              debugStyle);
        GUI.Label(new Rect(10, 110, 600, 50), $"Pos:  ({p.x:F2}, {p.y:F2})",                   debugStyle);
#endif
    }
}