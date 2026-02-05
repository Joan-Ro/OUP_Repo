using UnityEngine;

public class EnemyPatrol2D : MonoBehaviour
{
    [Header("PATRULLA")]
    public float speed = 2f;
    public float leftLimit;
    public float rightLimit;

    [Header("MUERTE")]
    public float deathDelay = 0.5f;
    public float jumpForce = 5f; // <--- 1. NUEVA VARIABLE

    private bool movingRight = true;
    private bool isDead;

    private Animator animator;
    private Collider2D col;

    void Awake()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isDead) return;
        Patrol();
    }

    void Patrol()
    {
        float dir = movingRight ? 1 : -1;
        transform.Translate(Vector2.right * dir * speed * Time.deltaTime);

        if (movingRight && transform.position.x >= rightLimit)
            ChangeDirection();
        else if (!movingRight && transform.position.x <= leftLimit)
            ChangeDirection();
    }

    void ChangeDirection()
    {
        movingRight = !movingRight;
        transform.localScale = new Vector3(movingRight ? 1 : -1, 1, 1);
    }

    // 2. SUSTITUYE TU OnTriggerEnter2D POR ESTE:
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("PlayerFeet"))
        {
            // Buscamos el Rigidbody2D en el objeto que colisionó o en su padre
            Rigidbody2D playerRb = other.GetComponentInParent<Rigidbody2D>();
            
            if (playerRb != null)
            {
                // Aplicamos el impulso hacia arriba (Vector2.up)
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, jumpForce);
            }

            Die();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetBool("IsDead", true);
        col.enabled = false; 
        Destroy(gameObject, deathDelay);
    }
}


