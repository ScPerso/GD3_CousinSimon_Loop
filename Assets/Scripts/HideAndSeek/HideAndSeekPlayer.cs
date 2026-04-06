using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Contrôle du joueur (cube) en vue du dessus avec ZQSD.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class HideAndSeekPlayer : MonoBehaviour
{
    public static HideAndSeekPlayer Instance { get; private set; }

    [Header("Movement")]
    public float moveSpeed = 5f;

    private CharacterController controller;
    private Vector3 velocity;
    private const float Gravity = -20f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        controller = GetComponent<CharacterController>();
        Debug.Log("[HideAndSeekPlayer] Awake — CharacterController trouvé.");
    }

    private void Update()
    {
        if (HideAndSeekManager.Instance != null && !HideAndSeekManager.Instance.IsPlaying)
            return;

        // Lecture clavier ZQSD (layout AZERTY)
        float h = 0f, v = 0f;

        if (Keyboard.current != null)
        {
            // wKey = position physique de Z sur AZERTY, aKey = Q, sKey = S, dKey = D
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)    v += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)  v -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)  h -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
        }

        Vector3 move = new Vector3(h, 0f, v).normalized * moveSpeed;

        // Gravité simple
        if (controller.isGrounded)
            velocity.y = -2f;
        else
            velocity.y += Gravity * Time.deltaTime;

        move.y = velocity.y;
        controller.Move(move * Time.deltaTime);

        Debug.Log($"[HideAndSeekPlayer] Move input h={h:F1} v={v:F1} pos={transform.position}");
    }
}
