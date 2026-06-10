using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class InteractorManager : MonoBehaviour
{
    public static InteractorManager instance;
    private BaseInteractible currentTarget;

    private readonly HashSet<Collider> activeHoverColliders = new HashSet<Collider>();

    private InteractionInfoManager m_info;
    private InputSystem input;

    private Transform interactingTransform;

    public bool isInspecting = false;
    public bool isGrabbing = false;

    private void Awake()
    {
        if(instance == null)
            instance = this;

        input = new InputSystem();
        input.Interaction.Enable();

        input.Interaction.Right_Trigger.started += ctx => TryInteract(false);
        input.Interaction.Left_Trigger.started += ctx => TryInteract(false);

        // 2. NEW GRIP ACTION INPUTS: Tell the interaction method it's a grip squeeze!
        input.Interaction.Right_Grip.started += ctx => TryInteract(true);
        input.Interaction.Left_Grip.started += ctx => TryInteract(true);

        // Handle physical release execution if objects need drop confirmation
        input.Interaction.Right_Grip.canceled += ctx => TryRelease();
        input.Interaction.Left_Grip.canceled += ctx => TryRelease();
    }
    private void Start()
    {
        m_info = InteractionInfoManager.instance;
    }

    public void RegisterHoverEnter(BaseInteractible interactibleTarget, Collider interactorCollider, Transform interactorTransform)
    {
        if (activeHoverColliders.Contains(interactorCollider) && currentTarget == interactibleTarget)
            return;

        if (isInspecting)
            return;

        if (isGrabbing && currentTarget != null)
            return;

        // If shifting to a completely brand new object, force-clear the old tracker state
        if (currentTarget != null && currentTarget != interactibleTarget)
        {
            ForceClearTracking();
        }

        currentTarget = interactibleTarget;
        // Add this specific hand collider to our active tracking set
        activeHoverColliders.Add(interactorCollider);

        interactingTransform = interactorTransform;

        // Only fire the highlight and UI text initialization if this is the FIRST hand entering
        if (activeHoverColliders.Count == 1)
        {
            currentTarget.OnHoverEnter();

            if (m_info != null)
            {
                m_info.ClearAllSpawnedElements();

                // OPTIONAL: Let the object dictate its tooltips dynamically!
                if (currentTarget is InteractibleGrab)
                {
                    m_info.AddText("Hold");
                    m_info.AddSprite("rgrip");
                    m_info.AddText("or");
                    m_info.AddSprite("lgrip");
                    m_info.AddText("to grab object");
                }
                else
                {
                    m_info.AddText("press");
                    m_info.AddSprite("rtrigger");
                    m_info.AddText("or");
                    m_info.AddSprite("ltrigger");
                    m_info.AddText("to interact");
                }
                m_info.RequestInfoDisappear(5f, 1f);
            }
        }
    }

    public void RegisterHoverExit(BaseInteractible interactibleTarget, Collider interactorCollider, Transform interactorTransform)
    {
        if (currentTarget == null || currentTarget != interactibleTarget) return;

        if (!activeHoverColliders.Contains(interactorCollider)) return;

        if (isInspecting)
            return;
        if (interactibleTarget is InteractibleGrab grabComponent && grabComponent.IsCurrentlyHeld)
            return;
        // Remove this hand from the tracking set
        activeHoverColliders.Remove(interactorCollider);

        // ONLY fire the exit highlight/UI drop if ALL hands have left the object completely
        if (activeHoverColliders.Count == 0)
        {
            currentTarget.OnHoverExit();
            currentTarget = null;
            interactingTransform = null;

            if (m_info != null)
            {
                m_info.RequestInfoDisappear(0f, 0.1f);
            }
        }
    }

    public void RequestInspectStatusChange(bool _b)
    {
        isInspecting = _b;
    }

    private void TryInteract(bool isGripInput)
    {
        if (currentTarget == null)
            return;

        if(!CanInteract())
            return;

        if (currentTarget is InteractibleGrab grabComponent)
        {
            if (!isGripInput) return;
            isGrabbing = !grabComponent.IsCurrentlyHeld;
        }
        else
        {
            if (isGripInput) return;
            if (isGrabbing) return;
        }

        currentTarget.Interact(interactingTransform);

        if (m_info != null)
        {
            m_info.ClearAllSpawnedElements();
            m_info.RequestInfoDisappear(0f, 0f);
        }
    }

    private void TryRelease()
    {
        // If we are currently holding a grabbed item and release the physical grip mesh button
        if (currentTarget != null && currentTarget is InteractibleGrab grabComponent)
        {
            if (grabComponent.IsCurrentlyHeld)
            {
                grabComponent.Interact(null); // Forces drop sequence routing
                isGrabbing = false;
                ForceClearTracking();
            }
        }
    }

    private void ForceClearTracking()
    {
        if (currentTarget != null)
        {
            currentTarget.OnHoverExit();
        }
        activeHoverColliders.Clear();
        currentTarget = null;
        interactingTransform = null;
        isGrabbing = false;
    }

    public bool CanInteract()
    {
        return currentTarget != null;
    }
}
