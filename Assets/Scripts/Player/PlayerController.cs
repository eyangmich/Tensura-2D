using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;

    [Header("Death")]
    [Tooltip("If Rimuru's Y falls below this, he dies.")]
    public float fallDeathY = -1000f;

    [Header("Attack")]
    public GameObject waterCutterPrefab;
    public float projectileSpeed = 10f;
    // Range = 4x Rimuru's width. Set Rimuru's pixel width in units here.
    public float rimururWidth = 0.5f; // adjust to match your sprite size in Unity units
    public float attackRange => rimururWidth * 4f;

    // Components
    private Rigidbody2D rb;
    private Animator anim;
    private PlayerStats stats;

    // State
    private float moveInput;
    private bool isGrounded;
    private bool facingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        HandleInput();
        HandleFlip();
        UpdateAnimations();
        CheckFallDeath();
    }

    void CheckFallDeath()
    {
        if (transform.position.y < fallDeathY)
            stats.Kill();
    }

    public void Respawn(Vector3 position)
    {
        transform.position    = position;
        rb.linearVelocity     = Vector2.zero;
        rb.angularVelocity    = 0f;
        stats.Revive();
    }

    void FixedUpdate()
    {
        CheckGround();
        Move();
    }

    // ─── Input ────────────────────────────────────────────────────────────────

    void HandleInput()
    {
        // Horizontal: Arrow keys OR WASD
        moveInput = Input.GetAxisRaw("Horizontal"); // Unity maps both by default

        // Jump: Up arrow OR W OR Space
        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            && isGrounded)
        {
            Jump();
        }

        // Attack: Left mouse button
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    // ─── Movement ─────────────────────────────────────────────────────────────

    void Move()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        anim.SetTrigger("Jump");
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void HandleFlip()
    {
        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    // ─── Attack ───────────────────────────────────────────────────────────────

    void TryAttack()
    {
        if (!stats.SpendMana(5))
        {
            Debug.Log("Not enough mana!");
            return;
        }

        // Direction from Rimuru to mouse cursor in world space
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - transform.position).normalized;

        // Spawn projectile
        GameObject proj = Instantiate(waterCutterPrefab, transform.position, Quaternion.identity);
        proj.GetComponent<ProjectileController>().Init(dir, projectileSpeed, attackRange, transform.position);

        // Rotate sprite to face direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);

        anim.SetTrigger("Attack");
    }

    // ─── Animations ───────────────────────────────────────────────────────────

    void UpdateAnimations()
    {
        anim.SetFloat("Speed", Mathf.Abs(moveInput));
        anim.SetBool("IsGrounded", isGrounded);
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        // Visualise attack range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rimururWidth * 4f);
    }
}