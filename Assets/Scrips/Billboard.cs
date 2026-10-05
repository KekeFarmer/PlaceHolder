using UnityEngine;

/// <summary>Keeps a sprite always facing the camera directly, so perspective doesn't skew its shape.</summary>
public class Billboard : MonoBehaviour
{
    private Camera targetCamera;

    private void Awake()
    {
        targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        Vector3 lookDirection = transform.position - targetCamera.transform.position;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(lookDirection);
    }
}
