using UnityEngine;

/// <summary>
/// Makes an object visible only while it is inside the player's flashlight.
/// The object stays active so it can become visible again immediately.
/// </summary>
public class HideUnlessIlluminated : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float extraDetectionRadius = 0.1f;
    [SerializeField, Min(0f)] private float revealDelay = 1f;

    private FlashlightLight flashlight;
    private Renderer[] objectRenderers;
    private float illuminatedSeconds;

    private void Awake()
    {
        objectRenderers = GetComponentsInChildren<Renderer>(true);
        SetVisible(false);
    }

    private void Update()
    {
        if (flashlight == null)
        {
            flashlight = FindObjectOfType<FlashlightLight>();
        }

        if (flashlight == null)
        {
            return;
        }

        bool isIlluminated = false;
        foreach (Renderer objectRenderer in objectRenderers)
        {
            if (objectRenderer == null)
            {
                continue;
            }

            float radius = objectRenderer.bounds.extents.magnitude + extraDetectionRadius;
            if (flashlight.IsPositionIlluminated(objectRenderer.bounds.center, radius))
            {
                isIlluminated = true;
                break;
            }
        }

        illuminatedSeconds = isIlluminated
            ? illuminatedSeconds + Time.deltaTime
            : 0f;
        SetVisible(illuminatedSeconds >= revealDelay);
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer objectRenderer in objectRenderers)
        {
            if (objectRenderer != null)
            {
                objectRenderer.enabled = visible;
            }
        }
    }
}
