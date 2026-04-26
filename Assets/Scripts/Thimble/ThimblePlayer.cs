using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Contrôle du joueur dans le mini-jeu Dé à coudre.
/// - ZQSD (layout AZERTY) pour se déplacer en espace monde.
/// - Espace pour sauter (saut unique, impossible en l'air).
/// - Gravité appliquée via CharacterController.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class ThimblePlayer : MonoBehaviour
{
    public static ThimblePlayer Instance { get; private set; }

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump & Gravity")]
    public float jumpForce = 7f;
    public float gravity   = -20f;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 spawnPoint;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance   = this;
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        spawnPoint = transform.position;
    }

    private void Update()
    {
        if (ThimbleManager.Instance != null && !ThimbleManager.Instance.IsPlaying)
            return;

        Move();
        HandleJump();
        ApplyGravity();
    }

    private void Move()
    {
        float h = 0f, v = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)    v += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)  v -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)  h -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
        }

        Vector3 move = new Vector3(h, 0f, v).normalized * moveSpeed;
        move.y = velocity.y;
        controller.Move(move * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (controller.isGrounded)
        {
            if (velocity.y < 0f) velocity.y = -2f;

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                velocity.y = jumpForce;
        }
    }

    private void ApplyGravity()
    {
        if (!controller.isGrounded)
            velocity.y += gravity * Time.deltaTime;
    }

    /// <summary>Téléporte le joueur au spawn (appelé par ThimbleWaterCube).</summary>
    public void Respawn()
    {
        controller.enabled = false;
        transform.position = spawnPoint;
        velocity           = Vector3.zero;
        controller.enabled = true;
    }
}
