using UnityEngine;

/// <summary>
/// Floor (X/Z) version of FlashlightLight. Put this on the LuzDeLinterna object.
/// It stays off until Equip() is called (from FlashlightPickupFloor), then
/// follows the player and toggles with right click.
/// </summary>
public class FlashlightLightFloor : MonoBehaviour
{
    [Header("Bateria")]
    [SerializeField] private float drainSeconds = 20f;
    [SerializeField] private float rechargeSeconds = 10f;
    [Range(0f, 1f)]
    [SerializeField] private float restartCharge = 0.25f;

    private Light lightComponent;
    private Transform player;
    private Vector3 offsetFromPlayer;
    private bool equipped;

    private void Awake()
    {
        lightComponent = GetComponent<Light>();
        lightComponent.enabled = false;
    }

    private void Start()
    {
        if (FlashlightProgress.HasFlashlight && !equipped)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
            {
                Equip(found.transform);
                lightComponent.enabled = FlashlightProgress.IsOn;
            }
        }
    }

    private void Update()
    {
        if (!equipped) return;

        transform.position = player.position + offsetFromPlayer;

        if (Input.GetMouseButtonDown(1))
        {
            if (lightComponent.enabled)
            {
                SetOn(false);
            }
            else if (!FlashlightProgress.Exhausted)
            {
                SetOn(true);
            }
        }

        if (FlashlightProgress.Tick(lightComponent.enabled, Time.deltaTime, drainSeconds, rechargeSeconds, restartCharge))
        {
            SetOn(false);
        }
    }

    private void SetOn(bool on)
    {
        lightComponent.enabled = on;
        FlashlightProgress.IsOn = on;
    }

    private void OnGUI()
    {
        if (!equipped || InventoryUI.IsOpen) return;
        FlashlightProgress.DrawBar();
    }

    public void Equip(Transform carrier)
    {
        player = carrier;
        offsetFromPlayer = transform.position - carrier.position;
        equipped = true;
        lightComponent.enabled = true;
        FlashlightProgress.IsOn = true;
    }

    public bool IsPositionIlluminated(Vector3 position, float radius = 0.25f)
    {
        if (!equipped || !lightComponent.enabled)
        {
            return false;
        }

        Vector3 toTarget = position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;
        if (distance <= 0.001f || distance > lightComponent.range + radius)
        {
            return false;
        }

        Vector3 direction = toTarget / distance;
        float coneLimit = Mathf.Cos(lightComponent.spotAngle * 0.5f * Mathf.Deg2Rad);
        if (Vector3.Dot(transform.forward, direction) < coneLimit)
        {
            return false;
        }

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.distance + radius >= distance - 0.1f;
        }

        return true;
    }
}
