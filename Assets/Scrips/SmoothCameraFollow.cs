using UnityEngine;

/// <summary>
/// Keeps the camera gently behind the player and looks slightly ahead while
/// the player is moving, so movement feels less rigid.
/// </summary>
public class SmoothCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0.01f)] private float smoothTime = 0.28f;
    [SerializeField, Min(0f)] private float lookAheadDistance = 0.65f;
    [SerializeField, Min(0f)] private float lookAheadSmoothTime = 0.18f;

    private Vector3 cameraOffset;
    private Vector3 positionVelocity;
    private Vector2 previousTargetPosition;
    private Vector2 movementDirection;
    private Vector2 movementDirectionVelocity;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        cameraOffset = transform.position - target.position;
        previousTargetPosition = target.position;
        enabled = true;
    }

    private void Awake()
    {
        if (target == null)
        {
            Debug.LogWarning("SmoothCameraFollow needs a target.", this);
            enabled = false;
            return;
        }

        cameraOffset = transform.position - target.position;
        previousTargetPosition = target.position;
    }

    private void LateUpdate()
    {
        Vector2 currentTargetPosition = target.position;
        Vector2 frameMovement = currentTargetPosition - previousTargetPosition;

        // Only update the direction while the player is actually moving. This
        // avoids the camera snapping back when the player releases the keys.
        if (frameMovement.sqrMagnitude > 0.000001f)
        {
            Vector2 desiredDirection = frameMovement.normalized;
            movementDirection = Vector2.SmoothDamp(
                movementDirection,
                desiredDirection,
                ref movementDirectionVelocity,
                lookAheadSmoothTime);
        }
        else
        {
            movementDirection = Vector2.SmoothDamp(
                movementDirection,
                Vector2.zero,
                ref movementDirectionVelocity,
                lookAheadSmoothTime);
        }

        // The camera follows only on X.
        Vector3 lookAhead = Vector3.right * movementDirection.x * lookAheadDistance;
        Vector3 desiredPosition = new Vector3(
            target.position.x + cameraOffset.x + lookAhead.x,
            transform.position.y,
            transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref positionVelocity, smoothTime);

        previousTargetPosition = currentTargetPosition;
    }
}
