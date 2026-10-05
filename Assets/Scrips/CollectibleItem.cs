using UnityEngine;

/// <summary>Put this on a collectible object. Pulsa E cerca de él para recogerlo y desaparece.</summary>
public class CollectibleItem : MonoBehaviour
{
    [SerializeField] private string itemName = "Objeto";
    [SerializeField] private Sprite icon;
    [SerializeField] private Transform player;
    [SerializeField, Min(0.1f)] private float pickupRange = 1f;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private Vector2 pickupPointOffset;

    private static int totalCollectibles;
    private static int collectedCount;

    public static int TotalCollectibles => totalCollectibles;
    public static int CollectedCount => collectedCount;

    private float currentDistance;

    private void Awake()
    {
        totalCollectibles++;
        if (icon == null && TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            icon = spriteRenderer.sprite;
        }
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
        if (currentDistance <= pickupRange && Input.GetKeyDown(pickupKey))
        {
            collectedCount++;
            Inventory.Instance?.Add(itemName, icon);
            gameObject.SetActive(false);
        }
    }

    private void OnGUI()
    {
        if (InventoryUI.IsOpen) return;

        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(Screen.width - 220f, 20f, 200f, 30f), $"Objetos: {collectedCount}/{totalCollectibles}", style);
    }
}
