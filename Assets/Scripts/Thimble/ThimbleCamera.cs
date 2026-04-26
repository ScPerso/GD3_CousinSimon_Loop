using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Caméra troisième personne pour le mini-jeu Dé à coudre.
/// - La souris tourne la caméra horizontalement autour du joueur.
/// - Le mouvement ZQSD est relatif au yaw de la caméra.
///
/// Lecture de la souris dans Update (Input System garantit la valeur une seule fois par frame).
/// Positionnement de la caméra dans LateUpdate (après que le joueur ait bougé).
/// </summary>
public class ThimbleCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Orbit")]
    [Tooltip("Sensibilité horizontale. 0.1 = lent, 0.3 = rapide.")]
    public float mouseSensitivity = 0.15f;

    [Header("Distance & Height")]
    public float distance = 7f;
    public float height   = 4f;

    [Header("Smoothing")]
    public float positionSmooth = 10f;

    /// <summary>Yaw courant en degrés. Lu par ThimblePlayer pour orienter le mouvement.</summary>
    public float Yaw { get; private set; }

    // Accumulé dans Update, consommé dans LateUpdate
    private float yawDelta;
    private bool  initialized;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void Start()
    {
        // Initialiser le yaw à l'angle actuel de la caméra pour éviter un snap au premier frame
        Yaw         = transform.eulerAngles.y;
        initialized = false; // on attend un frame pour ignorer le premier delta parasite
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    private void Update()
    {
        if (!initialized)
        {
            // Ignorer le premier frame : le curseur vient de se verrouiller,
            // le delta peut être parasité par le déplacement de centrage
            initialized = true;
            return;
        }

        if (Mouse.current == null) return;

        // delta.x est en pixels déplacés ce frame — pas de Time.deltaTime ici
        yawDelta = Mouse.current.delta.x.ReadValue() * mouseSensitivity;
    }

    private void LateUpdate()
    {
        // Auto-assign si non renseigné dans l'Inspector
        if (target == null)
        {
            if (ThimblePlayer.Instance != null) target = ThimblePlayer.Instance.transform;
            return;
        }

        Yaw += yawDelta;
        yawDelta = 0f; // consommé

        // Position désirée : derrière le joueur selon le yaw, décalée en hauteur
        Quaternion yawRot    = Quaternion.Euler(0f, Yaw, 0f);
        Vector3    desiredPos = target.position
                              - yawRot * Vector3.forward * distance
                              + Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, desiredPos, positionSmooth * Time.deltaTime);

        // Toujours regarder le joueur
        Vector3 lookDir = target.position - transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
