using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hides the scene's placeholder clues until the player's flashlight reaches
/// them. A clue remains hidden when it is outside the beam or obstructed.
/// </summary>
public class FlashlightRevealManager : MonoBehaviour
{
    [SerializeField] private PlayerFlashlight flashlight;
    [SerializeField] private string revealObjectPrefix = "PlaceHolder";

    private readonly List<Renderer> revealRenderers = new List<Renderer>();

    private void Awake()
    {
        if (flashlight == null)
        {
            flashlight = GetComponent<PlayerFlashlight>();
        }

        Renderer[] allRenderers = FindObjectsOfType<Renderer>(true);
        foreach (Renderer sceneRenderer in allRenderers)
        {
            if (BelongsToRevealObject(sceneRenderer.transform))
            {
                revealRenderers.Add(sceneRenderer);
            }
        }
    }

    private bool BelongsToRevealObject(Transform currentTransform)
    {
        while (currentTransform != null)
        {
            if (currentTransform.name.StartsWith(revealObjectPrefix))
            {
                return true;
            }

            currentTransform = currentTransform.parent;
        }

        return false;
    }

    private void LateUpdate()
    {
        if (flashlight == null)
        {
            return;
        }

        foreach (Renderer revealRenderer in revealRenderers)
        {
            if (revealRenderer == null)
            {
                continue;
            }

            float radius = revealRenderer.bounds.extents.magnitude;
            revealRenderer.enabled = flashlight.IsPositionIlluminated(revealRenderer.bounds.center, radius);
        }
    }
}
