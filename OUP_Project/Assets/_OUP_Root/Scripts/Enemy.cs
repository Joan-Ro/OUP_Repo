using UnityEngine;

public class Enemy : MonoBehaviour
{
    private bool isDead;

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Aquí puedes:
        // - reproducir animación
        // - desactivar collider
        // - sumar puntos

        Destroy(gameObject);
    }
}
