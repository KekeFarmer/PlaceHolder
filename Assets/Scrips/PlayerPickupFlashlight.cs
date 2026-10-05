using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Complete flashlight gameplay for the 3D scene. Walking close to an object
/// whose name contains "Linterna" collects it, hides the scene object and
/// equips an aimable spotlight on the player.
/// </summary>
public class PlayerPickupFlashlight : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0.01f)] private float movementSpeed = 4f;

    [Header("Pickup")]
    [SerializeField, Min(0.1f)] private float pickupRange = 1.25f;
    [SerializeField] private bool collectAutomatically = true;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;

    [Header("Light")]
    [SerializeField, Min(0.1f)] private float lightRange = 6f;
    [SerializeField, Range(5f, 160f)] private float lightAngle = 42f;
    [SerializeField, Min(0f)] private float lightIntensity = 8f;
    [SerializeField] private KeyCode toggleKey = KeyCode.F;
    [SerializeField] private Color lightColor = new Color(1f, 0.94f, 0.72f, 1f);
    [SerializeField, Range(0f, 1f)] private float beamOpacity = 0.18f;
    [SerializeField, Min(0f)] private float beamStartWidth = 0.1f;
    [SerializeField] private LayerMask obstacleLayers = ~0;

    private readonly List<GameObject> pickupObjects = new List<GameObject>();
    private Camera gameCamera;
    private Light spotlight;
    private GameObject beamObject;
    private Mesh beamMesh;
    private MeshRenderer beamRenderer;
    private bool hasFlashlight;
    private bool lightIsOn;
    private Vector3 aimDirection = Vector3.forward;

    public bool IsLightOn => lightIsOn;

    private void Awake()
    {
        gameCamera = Camera.main;
        FindPickupObjects();
        CreateSpotlight();
        CreateBeamVisual();
        SetLight(false);
    }

    private void Update()
    {
        MovePlayer();

        if (!hasFlashlight)
        {
            GameObject pickup = GetNearbyPickup();
            if (pickup != null && (collectAutomatically || Input.GetKeyDown(pickupKey)))
            {
                CollectFlashlight();
            }
            return;
        }

        UpdateAim();
        if (Input.GetKeyDown(toggleKey) || Input.GetMouseButtonDown(1))
        {
            SetLight(!lightIsOn);
        }
    }

    private void MovePlayer()
    {
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;

        float vertical = 0f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;

        Vector3 movement = new Vector3(horizontal, 0f, vertical);
        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }

        transform.position += movement * movementSpeed * Time.deltaTime;
    }

    public bool IsPositionIlluminated(Vector3 position, float radius = 0.25f)
    {
        if (!lightIsOn)
        {
            return false;
        }

        Vector3 toTarget = position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;
        if (distance <= 0.001f || distance > lightRange + radius)
        {
            return false;
        }

        Vector3 direction = toTarget / distance;
        float coneLimit = Mathf.Cos(lightAngle * 0.5f * Mathf.Deg2Rad);
        if (Vector3.Dot(aimDirection, direction) < coneLimit)
        {
            return false;
        }

        Vector3 origin = transform.position + Vector3.up * 0.2f + direction * 0.1f;
        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, obstacleLayers, QueryTriggerInteraction.Ignore))
        {
            return hit.distance + radius >= distance - 0.1f;
        }

        return true;
    }

    private void CollectFlashlight()
    {
        hasFlashlight = true;
        DisableDroppedFlashlightLights();
        foreach (GameObject pickup in pickupObjects)
        {
            if (pickup != null)
            {
                pickup.SetActive(false);
            }
        }

        UpdateAim();
        SetLight(true);
        Debug.Log("Linterna recogida. Pulsa F o clic derecho para apagar/encender.", this);
    }

    private void DisableDroppedFlashlightLights()
    {
        foreach (Light sceneLight in FindObjectsOfType<Light>(true))
        {
            if (sceneLight != spotlight && sceneLight.name.ToLowerInvariant().Contains("linterna"))
            {
                sceneLight.enabled = false;
            }
        }
    }

    private GameObject GetNearbyPickup()
    {
        GameObject nearest = null;
        float shortestDistance = float.PositiveInfinity;
        foreach (GameObject pickup in pickupObjects)
        {
            if (pickup == null || !pickup.activeInHierarchy)
            {
                continue;
            }

            Vector3 difference = pickup.transform.position - transform.position;
            difference.y = 0f;
            float distance = difference.magnitude;
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = pickup;
            }
        }

        return shortestDistance <= pickupRange ? nearest : null;
    }

    private void FindPickupObjects()
    {
        foreach (Transform item in FindObjectsOfType<Transform>(true))
        {
            if (item != transform && item.name.ToLowerInvariant().Contains("linterna"))
            {
                pickupObjects.Add(item.gameObject);
            }
        }
    }

    private void CreateSpotlight()
    {
        GameObject lightObject = new GameObject("Flashlight Equipped");
        lightObject.transform.SetParent(transform, false);
        spotlight = lightObject.AddComponent<Light>();
        spotlight.type = LightType.Spot;
        spotlight.color = lightColor;
        spotlight.intensity = lightIntensity;
        spotlight.range = lightRange;
        spotlight.spotAngle = lightAngle;
        spotlight.innerSpotAngle = lightAngle * 0.65f;
        spotlight.renderMode = LightRenderMode.ForcePixel;
        spotlight.shadows = LightShadows.None;
    }

    private void CreateBeamVisual()
    {
        beamObject = new GameObject("Flashlight Visible Beam");
        beamMesh = new Mesh { name = "Flashlight Visible Beam Mesh" };
        beamMesh.vertices = new Vector3[4];
        // The two triangles face upward, toward the camera.
        beamMesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        beamMesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
        beamMesh.colors = new[] { Color.white, Color.white, Color.white, Color.white };

        beamObject.AddComponent<MeshFilter>().sharedMesh = beamMesh;
        beamRenderer = beamObject.AddComponent<MeshRenderer>();

        // This is the same transparent shader already used by the previous
        // flashlight visual, so it works with the project's PSX renderer.
        Shader shader = Shader.Find("Sprites/Default");
        beamRenderer.material = new Material(shader != null ? shader : Shader.Find("Unlit/Color"));
        beamRenderer.material.color = Color.white;
        beamRenderer.sortingOrder = 20;
    }

    private void UpdateAim()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }

        if (gameCamera == null)
        {
            return;
        }

        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        Plane floor = new Plane(Vector3.up, transform.position);
        if (!floor.Raycast(ray, out float rayDistance))
        {
            return;
        }

        Vector3 direction = ray.GetPoint(rayDistance) - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        aimDirection = direction.normalized;
        Vector3 lightPosition = transform.position + Vector3.up * 1.1f;
        Vector3 lightTarget = transform.position + aimDirection * (lightRange * 0.55f);
        spotlight.transform.position = lightPosition;
        spotlight.transform.rotation = Quaternion.LookRotation(lightTarget - lightPosition, Vector3.up);
        UpdateBeamVisual();
    }

    private void UpdateBeamVisual()
    {
        if (beamMesh == null)
        {
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.025f;
        float visibleDistance = GetBeamDistance(origin, aimDirection);
        Vector3 perpendicular = new Vector3(-aimDirection.z, 0f, aimDirection.x);
        float nearHalfWidth = beamStartWidth * 0.5f;
        float farHalfWidth = Mathf.Tan(lightAngle * 0.5f * Mathf.Deg2Rad) * visibleDistance;
        Vector3 farCenter = origin + aimDirection * visibleDistance;

        beamMesh.vertices = new[]
        {
            origin + perpendicular * nearHalfWidth,
            origin - perpendicular * nearHalfWidth,
            farCenter - perpendicular * farHalfWidth,
            farCenter + perpendicular * farHalfWidth
        };

        Color nearColor = new Color(lightColor.r, lightColor.g, lightColor.b, beamOpacity * 0.9f);
        Color farColor = new Color(lightColor.r, lightColor.g, lightColor.b, beamOpacity * 0.25f);
        beamMesh.colors = new[] { nearColor, nearColor, farColor, farColor };
        beamMesh.RecalculateBounds();
    }

    private float GetBeamDistance(Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, lightRange, obstacleLayers, QueryTriggerInteraction.Ignore))
        {
            return hit.distance;
        }

        return lightRange;
    }

    private void SetLight(bool enabled)
    {
        lightIsOn = enabled;
        spotlight.enabled = enabled;
        if (beamRenderer != null)
        {
            beamRenderer.enabled = enabled;
        }
    }

    private void OnDestroy()
    {
        if (beamMesh != null)
        {
            Destroy(beamMesh);
        }

        if (beamObject != null)
        {
            Destroy(beamObject);
        }
    }

    private void OnGUI()
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        string status = hasFlashlight
            ? "LINTERNA: " + (lightIsOn ? "ENCENDIDA" : "APAGADA") + "  |  F o clic derecho"
            : "Acércate a la linterna y pulsa E para recogerla";
        GUI.Label(new Rect(20f, 20f, 500f, 32f), status, labelStyle);
    }
}
