using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController2D : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    // ESTADOS
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
    public float wallJumpHorizontalForce = 18f;
    public float wallJumpVerticalForce = 16f;
    public int maxHealth = 1;

    [Header("WALL CHECK")]
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;

    [Header("ENEMIGOS")]
    public LayerMask enemyLayer;

    [Header("WALL JUMP LOCK")]
    public float wallJumpLockDuration = 0.2f;
    private float wallJumpLockTimer;

    // PRIVADAS
    private bool movingRight = true;
    private bool touchingWall;
    private int jumpsLeft;
    private int currentHealth;
    private bool isDead;
    private Vector2 startPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        jumpsLeft = 1;
    }

    void Start()
    {
        startPos = transform.position;
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

        // SALTO: espacio (PC) o toque (móvil)
        if (Input.GetKeyDown(KeyCode.Space) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            PlayerJump();
        }

        if (wallJumpLockTimer > 0f)
            wallJumpLockTimer -= Time.deltaTime;
    }

    // ================= MOVIMIENTO =================

    void MoveSideways()
    {
        if (rb == null || isDead || isWallSliding || wallJumpLockTimer > 0f)
            return;

        float dir = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    void CheckWallAndTurn()
    {
        if (!isGrounded || isWallSliding || wallJumpLockTimer > 0f)
            return;

        bool wallAhead = Physics2D.OverlapCircle(
            wallCheck.position,
            wallCheckRadius * 0.8f,
            wallLayer
        );

        if (wallAhead)
            ChangeDirection();
    }

    void ChangeDirection()
    {
        movingRight = !movingRight;
        transform.localScale = new Vector3(movingRight ? 1 : -1, 1, 1);
    }

    // ================= SALTO =================

    void PlayerJump()
    {
        if (isDead) return;

        // WALL JUMP
        if (isWallSliding)
        {
            rb.linearVelocity = Vector2.zero;

            bool wallOnRight = wallCheck.position.x > transform.position.x;

            // Dirección opuesta a la pared
            movingRight = !wallOnRight;

            // Girar sprite inmediatamente
            transform.localScale = new Vector3(movingRight ? 1 : -1, 1, 1);

            float jumpX = movingRight ? wallJumpHorizontalForce : -wallJumpHorizontalForce;

            rb.linearVelocity = new Vector2(jumpX, wallJumpVerticalForce);

            wallJumpLockTimer = wallJumpLockDuration;
            jumpsLeft = 1;
            isWallSliding = false;

            animator.SetBool("isJumping", true);
            return;
        }

        // SALTO NORMAL
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, jumpForce);
            jumpsLeft = 1;
            animator.SetBool("isJumping", true);
            return;
        }

        // DOBLE SALTO
        if (!isGrounded && jumpsLeft > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, doubleJumpForce);
            jumpsLeft--;
            StartCoroutine(RollAnimation());
        }
    }

    IEnumerator RollAnimation()
    {
        isRolling = true;
        yield return new WaitForSeconds(0.2f);
        isRolling = false;
    }

    // ================= WALL SLIDE =================

    void HandleWallSlide()
    {
        touchingWall = Physics2D.OverlapCircle(
            wallCheck.position,
            wallCheckRadius * 0.9f,
            wallLayer
        );

        if (touchingWall && !isGrounded && rb.linearVelocity.y < 0 && wallJumpLockTimer <= 0f)
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

        touchingWall = Physics2D.OverlapCircle(
            wallCheck.position,
            wallCheckRadius,
            wallLayer
        );
    }

    // ================= VIDA =================

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
            TakeHit();
    }

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
        animator.SetBool("isDead", true);
        rb.linearVelocity = Vector2.zero;
        StartCoroutine(Respawn(0.5f));
    }

    IEnumerator Respawn(float delay)
{
    yield return new WaitForSeconds(delay);
    // Reinicia la escena actual por completo
    SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
}

    // ================= ANIMACIONES =================

    void UpdateAnimator()
    {
        verticalVelocity = rb.linearVelocity.y;

        isJumping = verticalVelocity > 0.1f && !isGrounded && !isWallSliding;
        isFalling = verticalVelocity < -0.1f && !isGrounded && !isWallSliding;

        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("isRolling", isRolling);
        animator.SetBool("isOnWall", isWallSliding);
        animator.SetBool("isDead", isDead);
    }

    // ================= GROUND =================

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (isGrounded)
        {
            jumpsLeft = 1;
            isWallSliding = false;
        }
    }

    // ================= GIZMOS =================

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











