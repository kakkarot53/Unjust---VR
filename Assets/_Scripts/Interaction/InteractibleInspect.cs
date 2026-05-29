using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class InteractibleInspect : BaseInteractible
{
    [Header("Inspection Overrides")]
    [SerializeField] private Transform inspectAnchor;
    [SerializeField] private Vector3 inspectRotationOffset;
    [SerializeField] private GameObject dimmingCanvas;

    [Header("Animation Settings")]
    [SerializeField] private float lerpSpeed = 8.0f;

    private Vector3 oripos;
    private Vector3 oriScale;
    private Quaternion oriRot;
    private PlayerMovementManager m_move;
    private InspectManager m_inspect;
    private InteractorManager m_interact;


    private Coroutine lerpRoutine;
    protected override void Start()
    {
        base.Start();

        m_interact = InteractorManager.instance;

        oripos = transform.position;
        oriRot = transform.rotation;
        oriScale = transform.localScale;
        m_move = PlayerMovementManager.instance;
        m_inspect = InspectManager.instance;
        m_interact = InteractorManager.instance;

    }

    public override void Interact()
    {
        if (lerpRoutine != null)
            return;

        base.Interact();

        if (isInteracting)
        {
            //make bg darker
            dimmingCanvas.SetActive(true);
            //disable movement
            m_move.RequestPlayerMovementEnable(false);

            //put the object in front of player
            Quaternion targetInspectRotation = Quaternion.Euler(inspectRotationOffset);
            lerpRoutine = StartCoroutine(TransitionObject(inspectAnchor.position, targetInspectRotation, true));

            m_interact.RequestInspectStatusChange(true);
        }
        else
        {
            m_inspect.StopInspection(this);

            //make bg normal
            dimmingCanvas.SetActive(false);

            //enable movement
            m_move.RequestPlayerMovementEnable(true);

            //put the object back
            lerpRoutine = StartCoroutine(TransitionObject(oripos, oriRot, false));
            m_interact.RequestInspectStatusChange(false);
        }
    }

    private IEnumerator TransitionObject(Vector3 targetPosition, Quaternion targetRotation, bool enteringInspection)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.001f ||
               Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * lerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * lerpSpeed);

            yield return null; // Wait for the next frame slice
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
        transform.localScale = oriScale;
        lerpRoutine = null;

        if (enteringInspection)
        {
            m_inspect.StartInspection(this);
        }
    }
}
