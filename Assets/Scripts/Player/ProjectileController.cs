using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileController : MonoBehaviour
{
    private float speed;
    private float maxRange;
    private Vector3 origin;
    private Vector2 direction;

    private Rigidbody2D rb;

    // ─── Initialisation (called by PlayerController) ──────────────────────────

    /// <summary>
    /// Sets up the Water Cutter projectile.
    /// </summary>
    /// <param name="dir">Normalised direction toward mouse.</param>
    /// <param name="spd">Travel speed in units/sec.</param>
    /// <param name="range">Max distance before destruction (4 × Rimuru width).</param>
    /// <param name="spawnPos">World position where projectile spawned.</param>
    public void Init(Vector2 dir, float spd, float range, Vector3 spawnPos)
    {
        direction = dir;
        speed     = spd;
        maxRange  = range;
        origin    = spawnPos;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Water Cutter travels flat
        rb.linearVelocity = direction * speed;
    }

    // ─── Lifetime check ───────────────────────────────────────────────────────

    void Update()
    {
        float travelled = Vector3.Distance(origin, transform.position);
        if (travelled >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    // ─── Collision ────────────────────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore the player who fired it
        if (other.CompareTag("Player")) return;

        // Future: damage enemies here
        // if (other.TryGetComponent(out EnemyStats enemy))
        //     enemy.TakeDamage(waterCutterDamage);

        Destroy(gameObject);
    }
}