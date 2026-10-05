using UnityEngine;

/// <summary>
/// On-screen debug aid for pickups: a cross on the exact pickup point plus a text with the
/// numbers needed to tune it (object position, offset, player position, distance, range).
/// </summary>
public static class PickupDebug
{
    public static void Draw(string label, Vector3 objectPosition, Vector3 pickupPoint, Vector3 playerPosition, float distance, float range, string axes)
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 screen = cam.WorldToScreenPoint(pickupPoint);
            if (screen.z > 0f)
            {
                float x = screen.x;
                float y = Screen.height - screen.y;

                GUI.color = Color.yellow;
                GUI.DrawTexture(new Rect(x - 10f, y - 1f, 20f, 2f), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(x - 1f, y - 10f, 2f, 20f), Texture2D.whiteTexture);
                GUI.Label(new Rect(x + 12f, y - 12f, 300f, 24f), $"{label} (punto de recogida)");
                GUI.color = Color.white;
            }
        }

        Vector3 offset = pickupPoint - objectPosition;
        Vector3 missing = pickupPoint - playerPosition;
        string state = distance <= range ? "EN RANGO" : "fuera de rango";
        GUI.Label(new Rect(20f, Screen.height - 120f, 560f, 100f),
            $"{label}\n" +
            $"Objeto: {Format(objectPosition)}   Offset: {Format(offset)}\n" +
            $"Punto de recogida: {Format(pickupPoint)}\n" +
            $"Jugador: {Format(playerPosition)}   Falta: {Format(missing)}\n" +
            $"Distancia ({axes}): {distance:F2} / rango {range:F2}   {state}");
    }

    private static string Format(Vector3 v)
    {
        return $"({v.x:F2}, {v.y:F2}, {v.z:F2})";
    }
}
