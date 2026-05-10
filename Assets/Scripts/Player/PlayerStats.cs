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

    void OnGUI()
    {
#if UNITY_EDITOR
        GUI.Label(new Rect(10, 10, 200, 20), $"HP:   {CurrentHP} / {maxHP}");
        GUI.Label(new Rect(10, 30, 200, 20), $"Mana: {CurrentMana} / {maxMana}");
#endif
    }
}