using UnityEngine;

/// <summary>Added at runtime to bullets spawned by PlayerShooter. Hits enemies and destroys itself.</summary>
public class Projectile : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyFlashlightChase enemy = other.GetComponent<EnemyFlashlightChase>();
        if (enemy == null)
        {
            return;
        }

        Vector2 hitDirection = (other.transform.position - transform.position).normalized;
        enemy.Hit(hitDirection);
        Destroy(gameObject);
    }
}
