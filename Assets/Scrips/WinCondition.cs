using UnityEngine;

/// <summary>Put this on the Player. Shows a blue win screen once all collectibles are picked up.</summary>
public class WinCondition : MonoBehaviour
{
    private bool hasWon;
    private Texture2D blueTexture;

    private void Awake()
    {
        blueTexture = new Texture2D(1, 1);
        blueTexture.SetPixel(0, 0, new Color(0.15f, 0.35f, 0.85f));
        blueTexture.Apply();
    }

    private void Update()
    {
        if (hasWon)
        {
            return;
        }

        if (CollectibleItem.TotalCollectibles > 0 && CollectibleItem.CollectedCount >= CollectibleItem.TotalCollectibles)
        {
            hasWon = true;
            GetComponent<PlayerCtrl>().enabled = false;
            GetComponent<PlayerShooter>().enabled = false;
        }
    }

    private void OnGUI()
    {
        if (!hasWon)
        {
            return;
        }

        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), blueTexture);

        GUIStyle winStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 48,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        winStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "Ganaste Causa", winStyle);
    }
}
