using UnityEngine;

/// <summary>Tracks the player's health and shows it on screen, on the left.</summary>
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private bool isDead;
    private Texture2D blackTexture;

    private void Awake()
    {
        currentHealth = maxHealth;
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
    }

    public void TakeDamage(int amount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (currentHealth == 0)
        {
            isDead = true;
            GetComponent<PlayerCtrl>().enabled = false;
            GetComponent<PlayerShooter>().enabled = false;
        }
    }

    private void OnGUI()
    {
        if (isDead)
        {
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), blackTexture);

            GUIStyle deathStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 48,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            deathStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "Perdiste oño", deathStyle);
            return;
        }

        if (InventoryUI.IsOpen) return;

        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(20f, 80f, 300f, 30f), $"Vida: {currentHealth}/{maxHealth}", style);
    }
}
