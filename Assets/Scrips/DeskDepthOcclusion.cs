using System.Collections.Generic;
using UnityEngine;


public class DeskDepthOcclusion : MonoBehaviour
{
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private string deskNamePrefix = "Desk";
    [SerializeField, Range(0.1f, 0.9f)] private float footBandHeight = 0.38f;

    private readonly List<Transform> desks = new List<Transform>();
    private Camera gameCamera;
    private Sprite fullSprite;
    private Sprite feetHiddenSprite;
    private bool areFeetHidden;

    private void Awake()
    {
        if (playerRenderer == null)
        {
            playerRenderer = GetComponent<SpriteRenderer>();
        }

        if (playerRenderer == null)
        {
            Debug.LogWarning("DeskDepthOcclusion needs a SpriteRenderer on the player.", this);
            enabled = false;
            return;
        }

        gameCamera = Camera.main;
        playerRenderer.maskInteraction = SpriteMaskInteraction.None;
        fullSprite = playerRenderer.sprite;
        feetHiddenSprite = CreateFeetHiddenSprite(fullSprite);
        FindDesks();
    }

    private void LateUpdate()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }

        SetFeetHidden(gameCamera != null && AreFeetOverDesk());
    }

    private void FindDesks()
    {
        HashSet<Transform> uniqueDesks = new HashSet<Transform>();
        foreach (MeshRenderer meshRenderer in FindObjectsOfType<MeshRenderer>(true))
        {
            Transform desk = FindDeskAncestor(meshRenderer.transform);
            if (desk != null)
            {
                uniqueDesks.Add(desk);
            }
        }

        desks.AddRange(uniqueDesks);
    }

    private Transform FindDeskAncestor(Transform currentTransform)
    {
        while (currentTransform != null)
        {
            if (currentTransform.name.StartsWith(deskNamePrefix))
            {
                return currentTransform;
            }

            currentTransform = currentTransform.parent;
        }

        return null;
    }

    private static bool TryGetDeskBounds(Transform desk, out Bounds deskBounds)
    {
        Renderer[] deskRenderers = desk.GetComponentsInChildren<Renderer>(true);
        if (deskRenderers.Length == 0)
        {
            deskBounds = default;
            return false;
        }

        deskBounds = deskRenderers[0].bounds;
        for (int index = 1; index < deskRenderers.Length; index++)
        {
            deskBounds.Encapsulate(deskRenderers[index].bounds);
        }

        return true;
    }

    private Sprite CreateFeetHiddenSprite(Sprite sourceSprite)
    {
        if (sourceSprite == null)
        {
            return null;
        }

        Rect sourceRect = sourceSprite.textureRect;
        float hiddenHeight = sourceRect.height * footBandHeight;
        float visibleHeight = sourceRect.height - hiddenHeight;
        if (visibleHeight <= 0f)
        {
            return null;
        }

        Rect visibleRect = new Rect(
            sourceRect.x,
            sourceRect.y + hiddenHeight,
            sourceRect.width,
            visibleHeight);
        Vector2 pivot = new Vector2(
            sourceSprite.pivot.x / sourceRect.width,
            Mathf.Clamp(sourceSprite.pivot.y - hiddenHeight, 0f, visibleHeight) / visibleHeight);

        return Sprite.Create(
            sourceSprite.texture,
            visibleRect,
            pivot,
            sourceSprite.pixelsPerUnit,
            0,
            SpriteMeshType.FullRect);
    }

    private void SetFeetHidden(bool shouldHideFeet)
    {
        if (areFeetHidden == shouldHideFeet || feetHiddenSprite == null)
        {
            return;
        }

        playerRenderer.sprite = shouldHideFeet ? feetHiddenSprite : fullSprite;
        areFeetHidden = shouldHideFeet;
    }

    private void OnDisable()
    {
        if (playerRenderer != null && fullSprite != null)
        {
            playerRenderer.sprite = fullSprite;
        }
    }

    private bool AreFeetOverDesk()
    {
        Rect footScreenRect = GetFootScreenRect(playerRenderer.bounds);
        foreach (Transform desk in desks)
        {
            if (desk != null && TryGetDeskBounds(desk, out Bounds deskBounds) &&
                footScreenRect.Overlaps(GetScreenRect(deskBounds)))
            {
                return true;
            }
        }

        return false;
    }

    private Rect GetFootScreenRect(Bounds playerBounds)
    {
        Rect playerScreenRect = GetScreenRect(playerBounds);
        return new Rect(
            playerScreenRect.xMin,
            playerScreenRect.yMin,
            playerScreenRect.width,
            playerScreenRect.height * footBandHeight);
    }

    private Rect GetScreenRect(Bounds worldBounds)
    {
        Vector3 min = worldBounds.min;
        Vector3 max = worldBounds.max;
        Vector3[] corners =
        {
            new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z)
        };

        Vector3 firstCorner = gameCamera.WorldToScreenPoint(corners[0]);
        float minX = firstCorner.x;
        float maxX = firstCorner.x;
        float minY = firstCorner.y;
        float maxY = firstCorner.y;

        for (int index = 1; index < corners.Length; index++)
        {
            Vector3 screenPoint = gameCamera.WorldToScreenPoint(corners[index]);
            minX = Mathf.Min(minX, screenPoint.x);
            maxX = Mathf.Max(maxX, screenPoint.x);
            minY = Mathf.Min(minY, screenPoint.y);
            maxY = Mathf.Max(maxY, screenPoint.y);
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }
}
