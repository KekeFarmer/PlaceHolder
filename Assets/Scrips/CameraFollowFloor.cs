using UnityEngine;

/// <summary>Moves the camera together with the player through the corridor, keeping the same offset and angle.</summary>
public class CameraFollowFloor : MonoBehaviour
{
    [SerializeField] private Transform target;

    private Vector3 offset;
    private float fixedX;

    private void Awake()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollowFloor needs a target.", this);
            enabled = false;
            return;
        }

        offset = transform.position - target.position;
        fixedX = transform.position.x;
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(fixedX, target.position.y + offset.y, target.position.z + offset.z);
    }
}
