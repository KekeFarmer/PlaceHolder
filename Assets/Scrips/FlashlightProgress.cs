using UnityEngine;

/// <summary>
/// Shared flashlight state that survives scene loads: whether the player already has it,
/// whether it is on, and the battery charge (0..1). The battery drains while the light is on
/// and recharges by itself while it is off.
/// </summary>
public static class FlashlightProgress
{
    public static bool HasFlashlight;
    public static bool IsOn;

    public static float Charge = 1f;

    /// <summary>True once the battery ran out. The light can't be turned on again until it recharged a bit.</summary>
    public static bool Exhausted;

    /// <summary>Drains or recharges the battery. Returns true on the frame the battery runs out.</summary>
    public static bool Tick(bool isOn, float deltaTime, float drainSeconds, float rechargeSeconds, float restartCharge)
    {
        if (isOn)
        {
            Charge = Mathf.Max(0f, Charge - deltaTime / Mathf.Max(0.01f, drainSeconds));
            if (Charge <= 0f)
            {
                Exhausted = true;
                return true;
            }
        }
        else
        {
            Charge = Mathf.Min(1f, Charge + deltaTime / Mathf.Max(0.01f, rechargeSeconds));
            if (Exhausted && Charge >= restartCharge)
            {
                Exhausted = false;
            }
        }

        return false;
    }

    private static GUIStyle labelStyle;

    /// <summary>Draws the battery bar in the top-right corner, below the "Objetos" counter.</summary>
    public static void DrawBar()
    {
        const float width = 160f;
        const float height = 14f;
        Rect rect = new Rect(Screen.width - 20f - width, 60f, width, height);

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
        }

        Color previous = GUI.color;

        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        GUI.color = Exhausted ? new Color(0.9f, 0.2f, 0.2f) : new Color(1f, 0.9f, 0.4f);
        GUI.DrawTexture(new Rect(rect.x + 2f, rect.y + 2f, (rect.width - 4f) * Charge, rect.height - 4f), Texture2D.whiteTexture);

        GUI.color = previous;
        GUI.Label(new Rect(rect.x - 208f, rect.y - 5f, 200f, 24f), Exhausted ? "Linterna: recargando..." : "Linterna", labelStyle);
    }
}
