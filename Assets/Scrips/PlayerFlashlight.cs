using UnityEngine;

/// <summary>
/// A directional flashlight controlled with the right mouse button. It draws a
/// filled cone (rather than a thin line) and uses that exact cone for gameplay.
/// </summary>
public class PlayerFlashlight : MonoBehaviour
{
    [Header("Beam")]
    [SerializeField, Min(0.1f)] private float maxDistance = 5f;
    [SerializeField, Range(5f, 150f)] private float beamAngle = 52f;
    [SerializeField, Min(0f)] private float beamStartWidth = 0.12f;
    [SerializeField] private Color flashlightColor = new Color(1f, 0.95f, 0.7f, 0.38f);
    [SerializeField] private LayerMask obstacleLayers = ~0;

    [Header("Battery")]
    [SerializeField, Min(0f)] private float batteryDuration = 10f;

    private Camera gameCamera;
    private Mesh beamMesh;
    private MeshRenderer beamRenderer;
    private bool isFlashlightOn;
    private float batteryRemaining;
    private Vector3 beamDirection = Vector3.right;

    public bool IsFlashlightOn => isFlashlightOn;

    private void Awake()
    {
        gameCamera = Camera.main;
        batteryRemaining = batteryDuration;
        CreateBeamVisual();
        SetFlashlightActive(false);
    }

    private void LateUpdate()
    {
        SetFlashlightActive(Input.GetMouseButton(1) && batteryRemaining > 0f);
        if (!isFlashlightOn) return;

        batteryRemaining = Mathf.Max(0f, batteryRemaining - Time.deltaTime);
        if (batteryRemaining <= 0f)
        {
            SetFlashlightActive(false);
            return;
        }

        if (gameCamera == null) gameCamera = Camera.main;
        if (gameCamera == null || !TryGetMouseWorldPosition(out Vector3 mouseWorldPosition)) return;

        Vector3 origin = transform.position;
        Vector3 direction = mouseWorldPosition - origin;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        beamDirection = direction.normalized;
        UpdateBeam(origin, beamDirection, GetBeamDistance(origin, beamDirection));
    }

    /// <summary>Checks range, cone angle, and line of sight for a target.</summary>
    public bool IsPositionIlluminated(Vector3 targetPosition, float targetRadius = 0.25f)
    {
        if (!isFlashlightOn) return false;

        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;
        float targetDistance = toTarget.magnitude;
        if (targetDistance <= 0.001f || targetDistance > maxDistance + targetRadius) return false;

        Vector3 targetDirection = toTarget / targetDistance;
        float minimumDot = Mathf.Cos(beamAngle * 0.5f * Mathf.Deg2Rad);
        if (Vector3.Dot(beamDirection, targetDirection) < minimumDot) return false;

        return GetBeamDistance(transform.position, targetDirection) + targetRadius >= targetDistance;
    }

    private void UpdateBeam(Vector3 origin, Vector3 direction, float distance)
    {
        Vector3 perpendicular = new Vector3(-direction.z, 0f, direction.x);
        float endHalfWidth = Mathf.Tan(beamAngle * 0.5f * Mathf.Deg2Rad) * distance;
        float y = origin.y + 0.02f;
        beamMesh.vertices = new[]
        {
            new Vector3(origin.x + perpendicular.x * beamStartWidth * .5f, y, origin.z + perpendicular.z * beamStartWidth * .5f),
            new Vector3(origin.x - perpendicular.x * beamStartWidth * .5f, y, origin.z - perpendicular.z * beamStartWidth * .5f),
            new Vector3(origin.x + direction.x * distance - perpendicular.x * endHalfWidth, y, origin.z + direction.z * distance - perpendicular.z * endHalfWidth),
            new Vector3(origin.x + direction.x * distance + perpendicular.x * endHalfWidth, y, origin.z + direction.z * distance + perpendicular.z * endHalfWidth)
        };
        beamMesh.RecalculateBounds();
    }

    private float GetBeamDistance(Vector3 origin, Vector3 direction)
    {
        float closestDistance = maxDistance;
        if (Physics.Raycast(origin, direction, out RaycastHit hit3D, maxDistance, obstacleLayers, QueryTriggerInteraction.Ignore))
            closestDistance = hit3D.distance;

        RaycastHit2D hit2D = Physics2D.Raycast(origin, direction, maxDistance, obstacleLayers);
        if (hit2D.collider != null) closestDistance = Mathf.Min(closestDistance, hit2D.distance);
        return closestDistance;
    }

    private void CreateBeamVisual()
    {
        GameObject beamObject = new GameObject("Flashlight Cone");
        beamObject.transform.SetParent(transform, true);
        beamMesh = new Mesh { name = "Flashlight Cone Mesh" };
        beamMesh.vertices = new Vector3[4];
        // The camera looks down at the floor, so this face must point upward.
        beamMesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        beamMesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };

        beamObject.AddComponent<MeshFilter>().sharedMesh = beamMesh;
        beamRenderer = beamObject.AddComponent<MeshRenderer>();
        Shader shader = Shader.Find("Sprites/Default");
        beamRenderer.material = new Material(shader != null ? shader : Shader.Find("Unlit/Color"));
        beamRenderer.material.color = flashlightColor;
        beamRenderer.sortingOrder = 15;
    }

    private void SetFlashlightActive(bool active)
    {
        isFlashlightOn = active;
        if (beamRenderer != null) beamRenderer.enabled = active;
    }

    private bool TryGetMouseWorldPosition(out Vector3 mouseWorldPosition)
    {
        Ray mouseRay = gameCamera.ScreenPointToRay(Input.mousePosition);
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        if (playerPlane.Raycast(mouseRay, out float rayDistance))
        {
            mouseWorldPosition = mouseRay.GetPoint(rayDistance);
            return true;
        }
        mouseWorldPosition = default;
        return false;
    }

    private void OnDestroy()
    {
        if (beamMesh != null) Destroy(beamMesh);
    }
}
