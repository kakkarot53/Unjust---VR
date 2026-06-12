using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InteractibleGrab : BaseInteractible
{
    //so it looks a bit better
    //[Header("Grab Configurations")]
    private float smoothSpeed = 240f;

    private Rigidbody rb;
    private Transform handHoldAnchor;
    private bool isGrabbed = false;
    private Vector3 oriPos;

    public bool IsCurrentlyHeld => isGrabbed;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
    }
    protected override void Start()
    {
        base.Start();
        oriPos = transform.position;
    }
    public override void Interact(Transform interactorTransform)
    {
        if (!CanInteract()) return;

        base.Interact();

        isGrabbed = isInteracting;

        if (isGrabbed)
        {
            handHoldAnchor = interactorTransform;

            // change physics param
            rb.useGravity = false;
            rb.linearDamping = 8f;
            rb.angularDamping = 8f;
        }
        else
        {
            // return physics param
            handHoldAnchor = null;
            rb.useGravity = true;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.05f;
        }
    }
    protected override void ResetInteractibleState(int _id)
    {
        base.ResetInteractibleState(_id);

        isGrabbed = false;
        handHoldAnchor = null;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.05f;
        }

        this.transform.position = oriPos;
    }
    private void FixedUpdate()
    {
        //dont run shit if not grabbing anything
        if (!isGrabbed || handHoldAnchor == null) return;

        // Velocity pull calculations to provide completely un-clippable tracking paths
        Vector3 targetDirection = handHoldAnchor.position - rb.position;
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, targetDirection * smoothSpeed * Time.fixedDeltaTime, smoothSpeed);

        // Rotation updates
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, handHoldAnchor.rotation, 15f * Time.fixedDeltaTime));
    }
}
