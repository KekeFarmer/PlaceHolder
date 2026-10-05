using UnityEngine;

/// <summary>Floor version of WinCondition. Put this on the Player. Shows a blue win screen once all collectibles are picked up.</summary>
public class WinConditionFloor : MonoBehaviour
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

        if (CollectibleItemFloor.TotalCollectibles > 0 && CollectibleItemFloor.CollectedCount >= CollectibleItemFloor.TotalCollectibles)
        {
            hasWon = true;
            GetComponent<PlayerCtrlFloor>().enabled = false;
            GetComponent<PlayerShooterFloor>().enabled = false;
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
