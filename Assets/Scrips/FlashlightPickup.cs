using UnityEngine;

/// <summary>Put this on the flashlight object. Pulsa E cerca de ella para recogerla y desaparece.</summary>
public class FlashlightPickup : MonoBehaviour
{
    [SerializeField] private string itemName = "Linterna";
    [SerializeField] private Sprite icon;
    [SerializeField] private Transform player;
    [SerializeField, Min(0.1f)] private float pickupRange = 1f;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private Vector2 pickupPointOffset;
    [SerializeField] private bool showDebugDistance = true;
    [SerializeField] private GameObject flashlightLight;
    [SerializeField] private GameObject droppedLight;

    private float currentDistance;

    private void Awake()
    {
        if (FlashlightProgress.HasFlashlight)
        {
            gameObject.SetActive(false);
            return;
        }

        if (icon == null)
        {
            if (TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                icon = spriteRenderer.sprite;
            }
            else if (TryGetComponent(out Renderer meshRenderer) && meshRenderer.sharedMaterial != null)
            {
                icon = CreateSolidColorSprite(GetMaterialColor(meshRenderer.sharedMaterial));
            }
        }
    }

    private static Color GetMaterialColor(Material material)
    {
        if (material.HasProperty("_Color")) return material.GetColor("_Color");
        if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
        if (material.HasProperty("_MainColor")) return material.GetColor("_MainColor");
        return Color.white;
    }

    private static Sprite CreateSolidColorSprite(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found == null) return;
            player = found.transform;
        }

        Vector2 pickupPoint = (Vector2)transform.position + pickupPointOffset;
        currentDistance = Vector2.Distance(pickupPoint, player.position);
        bool inRange = currentDistance <= pickupRange;
        if (inRange && Input.GetKeyDown(pickupKey))
        {
            if (flashlightLight != null)
            {
                flashlightLight.GetComponent<FlashlightLight>()?.Equip(player);
            }
            if (droppedLight != null)
            {
                droppedLight.SetActive(false);
            }
            Inventory.Instance?.Add(itemName, icon);
            FlashlightProgress.HasFlashlight = true;
            gameObject.SetActive(false);
        }
    }

    private void OnGUI()
    {
        if (!showDebugDistance) return;
        GUI.Label(new Rect(20f, 20f, 400f, 24f), $"Distancia a linterna: {currentDistance:F2} (rango: {pickupRange})");
    }
}
