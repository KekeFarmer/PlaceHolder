using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Put this on a door. Pulsa la tecla cerca de ella para cargar otra escena.</summary>
public class DoorSceneTransition : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private Transform player;
    [SerializeField, Min(0.1f)] private float activationRange = 1.5f;
    [SerializeField] private KeyCode activationKey = KeyCode.J;
    [SerializeField] private Vector3 activationPointOffset;
    [SerializeField] private bool showDebugDistance = true;

    private float currentDistance;

    private void Update()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found == null) return;
            player = found.transform;
        }

        Vector3 activationPoint = transform.position + activationPointOffset;
        currentDistance = Vector3.Distance(activationPoint, player.position);
        if (currentDistance <= activationRange && Input.GetKeyDown(activationKey))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }

    private void OnGUI()
    {
        if (!showDebugDistance) return;
        GUI.Label(new Rect(20f, 200f, 400f, 24f), $"Distancia a {name}: {currentDistance:F2} (rango: {activationRange})");
    }
}
