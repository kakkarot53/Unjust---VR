using UnityEngine;

public class FPSCamera : MonoBehaviour
{
    public float mouseSensitivity = 5f;
    public Transform playerBody, head;
    float xRotation = 0f;

    private InteractorManager m_interact;

    private InputSystem input;
    private void Awake()
    {
        input = new InputSystem();
        input.Interaction.Enable();
    }
    private void Start()
    {
        m_interact = InteractorManager.instance;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_interact.isInspecting)
            return;

        Vector2 lookInp = input.Interaction.Look.ReadValue<Vector2>();

        float mouseX = lookInp.x * mouseSensitivity;
        float mouseY = lookInp.y * mouseSensitivity;

        //get rotation and limits it
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 45f);

        //moves camera
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);

        //rotates head up n down
        if (xRotation > -90 && xRotation < 45)
        {
            head.Rotate(-Vector3.right * mouseY);
        }
    }
}
