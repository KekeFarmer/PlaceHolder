using UnityEngine;

/// <summary>Moves the 3D player over the floor with WASD or the arrow keys.</summary>
public class PlayerCtrl : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float movSpeed = 4f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        TryGetComponent(out spriteRenderer);
    }

    private void Update()
    {
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;

        if (spriteRenderer != null && horizontal != 0f)
        {
            spriteRenderer.flipX = horizontal < 0f;
        }

        float vertical = 0f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;

        Vector3 movement = new Vector3(horizontal, vertical, 0f);

        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }

        transform.position += movement * movSpeed * Time.deltaTime;
    }
}
