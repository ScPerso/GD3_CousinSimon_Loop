using UnityEngine;

/// <summary>
/// Caméra top-down qui suit le joueur avec un décalage fixe.
/// </summary>
public class HideAndSeekTopDownCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    public float height = 18f;
    public float smoothSpeed = 8f;
    public Vector3 offset = new Vector3(0f, 0f, -2f);

    private void LateUpdate()
    {
        if (target == null)
        {
            // Tentative de trouver le joueur s'il n'est pas assigné
            if (HideAndSeekPlayer.Instance != null)
            {
                target = HideAndSeekPlayer.Instance.transform;
                Debug.Log("[HideAndSeekTopDownCamera] Target assigné automatiquement.");
            }
            return;
        }

        Vector3 desiredPos = target.position + offset + Vector3.up * height;
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
