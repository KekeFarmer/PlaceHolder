using UnityEngine;

/// <summary>Put this on the Player. Opens/closes a grid inventory panel with a key.</summary>
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private int columns = 4;
    [SerializeField] private float slotSize = 64f;
    [SerializeField] private float slotSpacing = 10f;
    [Tooltip("Icon of the casings slot (e.g. ObjX/CajaMunicion). The slot only shows while you have casings left.")]
    [SerializeField] private Sprite casingIcon;

    public static bool IsOpen { get; private set; }

    private bool isOpen;
    private Texture2D panelTexture;
    private Texture2D slotTexture;

    private void Awake()
    {
        panelTexture = CreateSolidTexture(new Color(0f, 0f, 0f, 0.85f));
        slotTexture = CreateSolidTexture(new Color(1f, 1f, 1f, 0.15f));
    }

    private static Texture2D CreateSolidTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isOpen = !isOpen;
            IsOpen = isOpen;
            Time.timeScale = isOpen ? 0f : 1f;
        }
    }

    private void OnGUI()
    {
        if (!isOpen)
        {
            return;
        }

        var items = Inventory.Instance != null ? Inventory.Instance.Items : null;
        int casings = AmmoProgress.Reserve;
        int slotCount = (items != null ? items.Count : 0) + (casings > 0 ? 1 : 0);
        int rows = Mathf.Max(1, Mathf.CeilToInt(slotCount / (float)columns));

        float panelWidth = columns * slotSize + (columns + 1) * slotSpacing;
        float panelHeight = 50f + rows * slotSize + (rows + 1) * slotSpacing;
        Rect panelRect = new Rect(20f, 20f, panelWidth, panelHeight);

        GUI.color = Color.white;
        GUI.DrawTexture(panelRect, panelTexture);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
        titleStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 8f, panelRect.width - 24f, 30f), "Inventario", titleStyle);

        int index = 0;
        if (items != null)
        {
            foreach (var pair in items)
            {
                DrawSlot(SlotRect(panelRect, index), pair.Value.Icon, pair.Value.Count, null);
                index++;
            }
        }

        // Casings live in AmmoProgress (reloading spends them), so the slot just mirrors that counter
        // and disappears by itself when it reaches 0.
        if (casings > 0)
        {
            DrawSlot(SlotRect(panelRect, index), casingIcon, casings, "Casquillos");
        }
    }

    private Rect SlotRect(Rect panelRect, int index)
    {
        int col = index % columns;
        int row = index / columns;
        float x = panelRect.x + slotSpacing + col * (slotSize + slotSpacing);
        float y = panelRect.y + 44f + slotSpacing + row * (slotSize + slotSpacing);
        return new Rect(x, y, slotSize, slotSize);
    }

    private void DrawSlot(Rect slotRect, Sprite icon, int count, string fallbackLabel)
    {
        GUI.DrawTexture(slotRect, slotTexture);

        if (icon != null)
        {
            Rect texCoords = new Rect(
                icon.rect.x / icon.texture.width,
                icon.rect.y / icon.texture.height,
                icon.rect.width / icon.texture.width,
                icon.rect.height / icon.texture.height);
            Rect iconRect = new Rect(slotRect.x + 6f, slotRect.y + 6f, slotRect.width - 12f, slotRect.height - 12f);
            GUI.DrawTextureWithTexCoords(iconRect, icon.texture, texCoords);
        }
        else if (!string.IsNullOrEmpty(fallbackLabel))
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.UpperCenter };
            labelStyle.normal.textColor = Color.white;
            GUI.Label(slotRect, fallbackLabel, labelStyle);
        }

        if (count > 1)
        {
            GUIStyle countStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerRight
            };
            countStyle.normal.textColor = Color.white;
            GUI.Label(slotRect, $"x{count}", countStyle);
        }
    }
}
