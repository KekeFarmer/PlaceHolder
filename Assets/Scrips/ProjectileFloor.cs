using UnityEngine;

/// <summary>Added at runtime to bullets spawned by PlayerShooterFloor. Hits enemies and destroys itself.</summary>
public class ProjectileFloor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        EnemyFlashlightChaseFloor enemy = other.GetComponent<EnemyFlashlightChaseFloor>();
        if (enemy == null)
        {
            return;
        }

        Vector3 hitDirection = other.transform.position - transform.position;
        hitDirection.y = 0f;
        enemy.Hit(hitDirection.normalized);
        Destroy(gameObject);
    }
}
