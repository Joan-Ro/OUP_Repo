using System.Collections;
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

    [Header("INPUT")]

    // Privadas
    private Rigidbody2D rb;
    private bool movingRight = true;
    private bool touchingWall;
    private int jumpsLeft;
    private int currentHealth;
    private bool isDead;

    Vector2 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Die();
        }
    }

    void Awake()
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

        if (
            Input.GetKeyDown(KeyCode.Space) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        )
        {
            PlayerJump();
        }
    }


    // MOVIMIENTO

    void MoveSideways()
    {
        float dir = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }
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

    // SALTO

    void PlayerJump()
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

    // COLISIONES

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Cambiar dirección al chocar con pared
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

    // VIDA

    void TakeHit()
    {
        if (isDead) return;

        currentHealth--;

        if (currentHealth <= 0)
            Die();
    }
    void Die()
    {
        StartCoroutine(Respawn(0.5f));
    }
    IEnumerator Respawn(float durration)
    {
        yield return new WaitForSeconds(durration);
        transform.position = startPos;
    }

    // GIZMOS
    void OnDrawGizmosSelected()
    {
        if (wallCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }
}



