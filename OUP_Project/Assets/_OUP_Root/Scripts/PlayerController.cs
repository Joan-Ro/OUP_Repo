using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("STATS DEL JUGADOR")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public int maxHealth = 1;

    [Header("WALL CHECK (solo para saltar)")]
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;

    [Header("ENEMIGOS")]
    public LayerMask enemyLayer;

    // ───── Privadas ─────
    private Rigidbody2D rb;
    private bool movingRight = true;
    private bool touchingWall;
    private int jumpsLeft;
    private int currentHealth;
    private bool isDead;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        jumpsLeft = 1;
    }

    void Update()
    {
        if (isDead) return;

        MoveSideways();
        CheckWallForJump();

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            Jump();
    }

    // ───────── MOVIMIENTO ─────────

    void MoveSideways()
    {
        float dir = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    // 👉 SOLO para saber si puede saltar
    void CheckWallForJump()
    {
        touchingWall = Physics2D.OverlapCircle(
            wallCheck.position,
            wallCheckRadius,
            wallLayer
        );

        if (touchingWall)
            jumpsLeft = 1;
    }

    // ───────── SALTO ─────────

    void Jump()
    {
        if (touchingWall)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        else if (jumpsLeft > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsLeft--;
        }
    }

    // ───────── COLISIONES ─────────

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Cambio de dirección al chocar con pared
        if (((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            ChangeDirection();
        }

        // Daño
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            TakeHit();
        }
    }

    void ChangeDirection()
    {
        movingRight = !movingRight;

        transform.localScale = new Vector3(
            movingRight ? 1 : -1,
            1,
            1
        );
    }

    // ───────── VIDA ─────────

    void TakeHit()
    {
        if (isDead) return;

        currentHealth--;

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        Debug.Log("Jugador muerto");
    }

    // ───────── GIZMOS ─────────

    void OnDrawGizmosSelected()
    {
        if (wallCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }
}


