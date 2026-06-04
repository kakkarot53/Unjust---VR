using UnityEngine;

public class InspectManager : MonoBehaviour
{
    public static InspectManager instance;

    [SerializeField] private GameObject infoCanvas;

    [Header("Manipulation Settings")]
    [SerializeField] private float rotateSpeed = 120f;
    [SerializeField] private float scaleSpeed = 2f; 
    [SerializeField] private float minScaleFactor = 0.4f; 
    [SerializeField] private float maxScaleFactor = 2.5f;

    [Header("FlashLight")]
    [SerializeField] private Light flashLight;

    private InteractibleInspect currTarget;
    private InputSystem input;
    private Transform cameraTransform;

    private Vector3 initialAnchorPosition;
    private float currentZoomOffset = 0f;
    private Vector3 originalObjectScale;

    private void Awake()
    {
        if (instance == null) 
            instance = this;

        input = new InputSystem();
    }
    private void OnEnable()
    {
        input.Interaction.Enable();
    }

    private void OnDisable()
    {
        input.Interaction.Disable();
    }

    private void Start()
    {
        infoCanvas.SetActive(false);
        flashLight.enabled = false;

    }

    public void StartInspection(InteractibleInspect target)
    {
        currTarget = target;
        infoCanvas.SetActive(true);
        originalObjectScale = currTarget.transform.localScale;
        flashLight.enabled = true;
    }

    public void StopInspection(InteractibleInspect target)
    {
        if (currTarget == target)
        {
            currTarget.transform.localScale = originalObjectScale;
            currTarget = null;

        }
        infoCanvas.SetActive(false);
        flashLight.enabled = false;

    }

    private void Update()
    {
        // If we aren't looking at anything, don't execute joystick manipulation physics
        if (currTarget == null) return;

        HandleRotation();
        HandleZoom();
    }

    private void HandleRotation()
    {
        // 1. Read Left Joystick Vector2 for Panning/Rotation
        Vector2 panInput = input.Interaction.Pan.ReadValue<Vector2>();

        if (panInput.sqrMagnitude > 0.01f)
        {
            // Rotate the object relative to the camera world space axes so it feels natural to the player
            Vector3 upAxis = cameraTransform != null ? cameraTransform.up : Vector3.up;
            Vector3 rightAxis = cameraTransform != null ? cameraTransform.right : Vector3.right;

            currTarget.transform.Rotate(upAxis, -panInput.x * rotateSpeed * Time.deltaTime, Space.World);
            currTarget.transform.Rotate(rightAxis, -panInput.y * rotateSpeed * Time.deltaTime, Space.World);
        }
    }

    private void HandleZoom()
    {
        Vector2 zoomInput = input.Interaction.Zoom.ReadValue<Vector2>();

        if (Mathf.Abs(zoomInput.y) > 0.01f)
        {
            // Calculate a scaling multiplier modification step based on joystick position or scroll ticks
            float scaleChange = zoomInput.y * scaleSpeed * Time.deltaTime;

            // Determine what the new scale would be across all 3 spatial axes uniformly
            Vector3 targetScale = currTarget.transform.localScale + (originalObjectScale * scaleChange);

            // Establish our absolute dimensional caps based on the initial starting model size bounds
            Vector3 minScale = originalObjectScale * minScaleFactor;
            Vector3 maxScale = originalObjectScale * maxScaleFactor;

            // Clamp the X, Y, and Z axes component limits smoothly so it doesn't break shape integrity
            targetScale.x = Mathf.Clamp(targetScale.x, minScale.x, maxScale.x);
            targetScale.y = Mathf.Clamp(targetScale.y, minScale.y, maxScale.y);
            targetScale.z = Mathf.Clamp(targetScale.z, minScale.z, maxScale.z);

            // Apply the final uniform scaling update directly to the inspected object
            currTarget.transform.localScale = targetScale;
        }
    }
}
