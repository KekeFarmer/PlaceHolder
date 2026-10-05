using UnityEngine;

/// <summary>Chases the player while lit by the flashlight, and keeps chasing for a short grace period after losing the light.</summary>
public class EnemyFlashlightChase : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float stopDistance = 0.7f;
    [SerializeField] private float detectionRadius = 0.3f;
    [SerializeField, Min(0f)] private float loseTargetDelay = 2f;
    [SerializeField, Min(0f)] private float illuminationDelay = 1.5f;

    [Header("On Hit")]
    [SerializeField] private float knockbackDistance = 1f;
    [SerializeField, Min(0.01f)] private float knockbackDuration = 0.15f;
    [SerializeField] private float stunDuration = 2f;

    [Header("Touch Damage")]
    [SerializeField] private float touchRange = 4f;
    [SerializeField] private int touchDamage = 10;
    [SerializeField, Min(0.1f)] private float damageCooldown = 1f;

    private FlashlightLight flashlight;
    private Transform player;
    private PlayerHealth playerHealth;
    private bool hasSeenFlashlight;
    private float secondsSinceLastIlluminated;
    private float secondsIlluminated;
    private float damageCooldownRemaining;

    private bool isStunned;
    private float stunTimeRemaining;
    private bool isKnockingBack;
    private float knockbackTimeRemaining;
    private Vector3 knockbackStart;
    private Vector3 knockbackTarget;

    private void Start()
    {
        FindFlashlight();
    }

    public void Hit(Vector2 fromDirection)
    {
        isStunned = true;
        stunTimeRemaining = stunDuration;
        isKnockingBack = true;
        knockbackTimeRemaining = knockbackDuration;
        knockbackStart = transform.position;
        knockbackTarget = transform.position + (Vector3)(fromDirection * knockbackDistance);
    }

    private void Update()
    {
        if (isStunned)
        {
            stunTimeRemaining -= Time.deltaTime;

            if (isKnockingBack)
            {
                knockbackTimeRemaining -= Time.deltaTime;
                float t = 1f - Mathf.Clamp01(knockbackTimeRemaining / knockbackDuration);
                transform.position = Vector3.Lerp(knockbackStart, knockbackTarget, t);
                if (knockbackTimeRemaining <= 0f)
                {
                    isKnockingBack = false;
                }
            }

            if (stunTimeRemaining <= 0f)
            {
                isStunned = false;
            }
            return;
        }

        if (damageCooldownRemaining > 0f)
        {
            damageCooldownRemaining -= Time.deltaTime;
        }

        if (player != null && playerHealth != null && damageCooldownRemaining <= 0f)
        {
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, touchRange);
            foreach (Collider2D nearbyCollider in nearbyColliders)
            {
                if (nearbyCollider.GetComponent<PlayerHealth>() != null)
                {
                    playerHealth.TakeDamage(touchDamage);
                    damageCooldownRemaining = damageCooldown;
                    break;
                }
            }
        }

        if (flashlight == null)
        {
            FindFlashlight();
            return;
        }

        bool isIlluminated = flashlight.IsPositionIlluminated(transform.position, detectionRadius);
        if (isIlluminated)
        {
            secondsSinceLastIlluminated = 0f;
            secondsIlluminated += Time.deltaTime;
            if (secondsIlluminated >= illuminationDelay)
            {
                hasSeenFlashlight = true;
            }
        }
        else
        {
            secondsIlluminated = 0f;
            if (hasSeenFlashlight)
            {
                secondsSinceLastIlluminated += Time.deltaTime;
                if (secondsSinceLastIlluminated >= loseTargetDelay)
                {
                    hasSeenFlashlight = false;
                }
            }
        }

        if (!hasSeenFlashlight)
        {
            return;
        }

        Vector3 playerPosition = player.position;
        playerPosition.z = transform.position.z;

        Vector3 directionToPlayer = playerPosition - transform.position;
        if (directionToPlayer.sqrMagnitude <= stopDistance * stopDistance)
        {
            return;
        }

        transform.position += directionToPlayer.normalized * moveSpeed * Time.deltaTime;
    }

    private void FindFlashlight()
    {
        flashlight = FindObjectOfType<FlashlightLight>();
        if (flashlight != null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            player = playerObject?.transform;
            playerHealth = playerObject?.GetComponent<PlayerHealth>();
        }
    }
}
