using System.Collections;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    private Animator animator;

    // Estados
    float verticalVelocity;
    bool isJumping;
    bool isFalling;
    bool isRolling;
    bool isWallSliding;

    [Header("GROUND CHECK")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("STATS DEL JUGADOR")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float doubleJumpForce = 14f;
    public float wallSlideSpeed = 2f;
    public int maxHealth = 1;

    [Header("WALL CHECK")]
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;

    [Header("ENEMIGOS")]
    public LayerMask enemyLayer;

    [Header("INPUT")]
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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;
        jumpsLeft = 1;
    }

    void Update()
    {
        if (isDead) return;

        CheckGround();
        CheckWallForJump();
        CheckWallAndTurn();
        HandleWallSlide();

        MoveSideways();
        UpdateAnimator();

        if (Input.GetKeyDown(KeyCode.Space) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            PlayerJump();
        }
    }

    // ---------------------------------------------------------------
    // MOVIMIENTO
    // ---------------------------------------------------------------
    void MoveSideways()
    {
        if (rb == null || isDead) return;

        float dir = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    void CheckWallAndTurn()
    {
        if (!isGrounded) return;

        bool wallAhead = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
        if (wallAhead)
            ChangeDirection();
    }

    void ChangeDirection()
    {
        movingRight = !movingRight;
        transform.localScale = new Vector3(movingRight ? 1 : -1, 1, 1);
    }

    // ---------------------------------------------------------------
    // SALTO
    // ---------------------------------------------------------------
    void PlayerJump()
    {
        if (isDead) return;

        // Jump desde suelo
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsLeft = 1; // aún tiene la opción del doble salto
            isRolling = false;
            return;
        }

        // Doble salto (ROLL)
        if (!isGrounded && jumpsLeft > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
            jumpsLeft = 0;
            StartCoroutine(RollAnimation());
            return;
        }
    }

    IEnumerator RollAnimation()
    {
        isRolling = true;
        yield return new WaitForSeconds(0.25f);
        isRolling = false;
    }

    // ---------------------------------------------------------------
    // WALL SLIDE
    // ---------------------------------------------------------------
    void HandleWallSlide()
    {
        if (touchingWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
        else
        {
            isWallSliding = false;
        }
    }

    void CheckWallForJump()
    {
        if (isGrounded)
        {
            touchingWall = false;
            return;
        }

        touchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
    }

    // ---------------------------------------------------------------
    // VIDA / DAÑO
    // ---------------------------------------------------------------
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
            TakeHit();
    }

    void TakeHit()
    {
        if (isDead) return;

        currentHealth--;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetBool("isDead", true);
        rb.linearVelocity = Vector2.zero;

        StartCoroutine(Respawn(0.5f));
    }

    IEnumerator Respawn(float durration)
    {
        yield return new WaitForSeconds(durration);

        transform.position = startPos;
        isDead = false;
        animator.SetBool("isDead", false);
        animator.SetTrigger("respawn");
        currentHealth = maxHealth;
    }

    // ---------------------------------------------------------------
    // ANIMATOR
    // ---------------------------------------------------------------
    void UpdateAnimator()
    {
        // velocidad vertical
        verticalVelocity = rb.linearVelocity.y;
        isJumping = verticalVelocity > 0.1f && !isGrounded;
        isFalling = verticalVelocity < -0.1f && !isGrounded;

        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("isRolling", isRolling);
        animator.SetBool("isOnWall", isWallSliding);
        animator.SetBool("isDead", isDead);
    }

    // ---------------------------------------------------------------
    // CHECKS
    // ---------------------------------------------------------------
    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded)
        {
            jumpsLeft = 1;
            isWallSliding = false;
        }
    }

    // ---------------------------------------------------------------
    // GIZMOS
    // ---------------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}





