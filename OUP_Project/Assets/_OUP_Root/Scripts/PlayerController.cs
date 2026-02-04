using System.Collections;
using UnityEngine;

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
    public float wallJumpForce = 12f;
    public int maxHealth = 1;

    [Header("WALL CHECK")]
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;

    [Header("ENEMIGOS")]
    public LayerMask enemyLayer;

    // PRIVADAS
    private bool movingRight = true;
    private bool touchingWall;
    private int jumpsLeft;
    private int currentHealth;
    private bool isDead;
    Vector2 startPos;

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

        if (Input.GetKeyDown(KeyCode.Space) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            PlayerJump();
        }
    }

    void MoveSideways()
    {
        if (rb == null || isDead || isWallSliding) return;  // ← No corre en slide

        float dir = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    void CheckWallAndTurn()
    {
        if (!isGrounded || isWallSliding) return;  // ← Solo suelo, ignora en slide

        bool wallAhead = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius * 0.8f, wallLayer);  // ← Radius chico anti-prematuro
        if (wallAhead)
        {
            ChangeDirection();
            Debug.Log("Giro pared: " + (movingRight ? "derecha" : "izquierda"));
        }
    }

    void ChangeDirection()
    {
        movingRight = !movingRight;
        transform.localScale = new Vector3(movingRight ? 1 : -1, 1, 1);
    }

    void PlayerJump()
    {
        if (isDead) return;

        if (isWallSliding)
        {
            // FUERZA MASIVA + DESAPEGUE
            rb.linearVelocity = new Vector2(0, 0);  // ← RESET VELOCIDAD (desapega YA)
            bool wallOnRight = wallCheck.position.x > transform.position.x;
            float jumpX = wallOnRight ? -25f : 25f;  // ← FUERZA FIJA FUERTÍSIMA
            rb.linearVelocity = new Vector2(jumpX, 20f);

            jumpsLeft = 1;
            isWallSliding = false;
            animator.SetBool("isJumping", true);

            StartCoroutine(WallJumpCooldown());
            Debug.Log("DESAPEGUE FUERTE: " + (wallOnRight ? "→IZQ" : "→DER"));
            return;
        }


        // SALTO NORMAL suelo
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, jumpForce);
            jumpsLeft = 1;
            isRolling = false;
            animator.SetBool("isJumping", true);
            return;
        }

        // DOBLE SALTO
        if (!isGrounded && jumpsLeft > 0 && !isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, doubleJumpForce);
            jumpsLeft--;
            StartCoroutine(RollAnimation());
            return;
        }

        if (isGrounded) { /* ... */ }
        if (!isGrounded && jumpsLeft > 0 && !isWallSliding) { /* ... */ }
    }

    IEnumerator WallJumpCooldown()
    {
        yield return new WaitForSeconds(0.25f);  // ← +Largo anti-rebote
    }


    IEnumerator RollAnimation()
    {
        isRolling = true;
        yield return new WaitForSeconds(0.2f);
        isRolling = false;
    }

    void HandleWallSlide()
    {
        touchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius * 0.9f, wallLayer);  // ← Sensible post-salto

        if (touchingWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(0, -wallSlideSpeed);  // ← X=0 anti-mueve slide
                                                                  // resto...
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

    IEnumerator Respawn(float duration)
    {
        yield return new WaitForSeconds(duration);
        transform.position = startPos;
        isDead = false;
        animator.SetBool("isDead", false);
        animator.SetTrigger("respawn");
        currentHealth = maxHealth;
    }

    void UpdateAnimator()
    {
        verticalVelocity = rb.linearVelocity.y;
        isJumping = verticalVelocity > 0.1f && !isGrounded && !isWallSliding && !isRolling;
        isFalling = verticalVelocity < -0.1f && !isGrounded && !isWallSliding;

        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("isRolling", isRolling);
        animator.SetBool("isOnWall", isWallSliding);
        animator.SetBool("isDead", isDead);
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded)
        {
            jumpsLeft = 1;
            isWallSliding = false;  // ← Reset slide en suelo
        }
    }

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










