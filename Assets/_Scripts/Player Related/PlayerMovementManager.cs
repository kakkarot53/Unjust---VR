using UnityEngine;

public class PlayerMovementManager : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform centerEyeCamera;

    [Header("Locomotion Configuration")]
    [SerializeField] private float moveSpeed = 2.2f;
    [SerializeField] private float gravity = -9.81f;

    private Vector2 inputVector = Vector2.zero;
    private float verticalVelocity = 0f;

    private bool canMove = true;

    public static PlayerMovementManager instance;
    private InputSystem input;

    private void Awake()
    {
        if(instance == null)
            instance = this;

        input = new InputSystem();
        input.Interaction.Enable();

        input.Interaction.Move.performed += ctx => inputVector = ctx.ReadValue<Vector2>();
        input.Interaction.Move.canceled += ctx => inputVector = Vector2.zero;
    }

    public void RequestPlayerMovementEnable(bool enable)
    {
        canMove = enable;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 1. Handle Gravity calculations first (Always running)
        if (controller.isGrounded)
        {
            // Small constant downward force keeps the capsule glued to slopes
            verticalVelocity = -2.0f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // 2. Process Horizontal Input direction (Only if player is NOT inspecting an object)
        Vector3 moveDirection = Vector3.zero;

        if (canMove)
        {
            Vector3 forward = centerEyeCamera.forward;
            Vector3 right = centerEyeCamera.right;

            // Flatten the vectors on the Y axis to keep horizontal speed uniform
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            moveDirection = (forward * inputVector.y) + (right * inputVector.x);
        }

        // 3. Combine horizontal movement vectors with vertical physics forces
        Vector3 finalVelocity = (moveDirection * moveSpeed) + (Vector3.up * verticalVelocity);

        // 4. Send finalized translation vectors to the controller component
        controller.Move(finalVelocity * Time.deltaTime);
    }
}
