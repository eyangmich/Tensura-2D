using UnityEngine;

/// <summary>
/// Place this on a checkpoint GameObject with a trigger Collider2D.
/// When Rimuru passes through, it updates the GameManager's respawn point.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("State")]
    [Tooltip("Has this checkpoint been activated yet?")]
    public bool activated = false;

    [Header("Visual Feedback (optional)")]
    [Tooltip("Sprite shown when inactive (e.g. grey flag).")]
    public Sprite inactiveSprite;
    [Tooltip("Sprite shown once activated (e.g. blue flag).")]
    public Sprite activeSprite;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // Make sure the collider is a trigger.
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        // Start with the inactive look.
        if (sr != null && inactiveSprite != null)
            sr.sprite = inactiveSprite;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only react to the player.
        if (!other.CompareTag("Player")) return;

        // Don't re-trigger if already activated.
        if (activated) return;

        Activate();
    }

    void Activate()
    {
        activated = true;

        // Save this position as the respawn point.
        // Using transform.position so Rimuru respawns exactly here.
        GameManager.Instance?.SetRespawnPoint(transform.position);

        // Swap to the activated sprite if one is assigned.
        if (sr != null && activeSprite != null)
            sr.sprite = activeSprite;

        Debug.Log($"Checkpoint reached at {transform.position}");
    }

    // Draw a helper marker in the Scene view so checkpoints are easy to find.
    void OnDrawGizmos()
    {
        Gizmos.color = activated ? Color.cyan : Color.grey;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.5f);
    }
}