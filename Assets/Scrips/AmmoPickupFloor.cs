using UnityEngine;

/// <summary>Floor (X/Z) version of AmmoPickup. Pulsa E cerca de él para recoger casquillos para recargar y desaparece.</summary>
public class AmmoPickupFloor : MonoBehaviour
{
    [SerializeField, Min(1)] private int casings = 1;
    [SerializeField] private Transform player;
    [SerializeField, Min(0.1f)] private float pickupRange = 1f;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private Vector3 pickupPointOffset;
    [SerializeField] private bool showDebugDistance = true;

    private static AmmoPickupFloor nearest;
    private static int nearestFrame = -1;

    private float currentDistance;

    private void Update()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found == null) return;
            player = found.transform;
        }

        Vector3 pickupPoint = transform.position + pickupPointOffset;
        currentDistance = Vector3.Distance(pickupPoint, player.position);
        if (nearestFrame != Time.frameCount || currentDistance < nearest.currentDistance)
        {
            nearest = this;
            nearestFrame = Time.frameCount;
        }

        if (currentDistance <= pickupRange && Input.GetKeyDown(pickupKey))
        {
            AmmoProgress.AddCasings(casings);
            gameObject.SetActive(false);
        }
    }

    private void OnGUI()
    {
        // With several casings in the scene only the nearest one writes its distance, so the texts don't overlap.
        if (!showDebugDistance || nearest != this) return;
        GUI.Label(new Rect(20f, 110f, 400f, 24f), $"Distancia a {name}: {currentDistance:F2} (rango: {pickupRange})");
    }
}
