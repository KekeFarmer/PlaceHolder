using UnityEngine;

/// <summary>Moves the player over the floor (X/Z) with WASD or the arrow keys. Y stays fixed.</summary>
public class PlayerCtrlFloor : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float movSpeed = 4f;
    [SerializeField] private float movementAngle = 25f;
    [SerializeField] private float heightOffset = 0.3f;
    [SerializeField] private float groundRayHeight = 5f;

    private void Update()
    {
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;

        float forward = 0f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) forward -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) forward += 1f;

        Vector3 movement = new Vector3(horizontal, 0f, forward);
        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }

        movement = Quaternion.Euler(0f, movementAngle, 0f) * movement;

        Vector3 newPosition = transform.position + movement * movSpeed * Time.deltaTime;

        Vector3 rayOrigin = newPosition + Vector3.up * groundRayHeight;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundRayHeight * 2f))
        {
            newPosition.y = hit.point.y + heightOffset;
        }

        transform.position = newPosition;
    }
}
