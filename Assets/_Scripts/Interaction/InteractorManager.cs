using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class InteractorManager : MonoBehaviour
{
    public static InteractorManager instance;
    private BaseInteractible currentTarget;

    private readonly HashSet<Collider> activeHoverColliders = new HashSet<Collider>();
    private InteractionInfoManager m_info;
    private InputSystem input;

    private bool isInspecting = false;

    private void Awake()
    {
        if(instance == null)
            instance = this;

        input = new InputSystem();
        input.Interaction.Enable();

        input.Interaction.Right_Trigger.started += ctx => TryInteract();
        input.Interaction.Left_Trigger.started += ctx => TryInteract();
    }
    private void Start()
    {
        m_info = InteractionInfoManager.instance;
    }

    public void RegisterHoverEnter(BaseInteractible interactibleTarget, Collider interactorCollider)
    {
        if (activeHoverColliders.Contains(interactorCollider) && currentTarget == interactibleTarget)
            return;

        if (isInspecting)
            return;

        // If shifting to a completely brand new object, force-clear the old tracker state
        if (currentTarget != null && currentTarget != interactibleTarget)
        {
            ForceClearTracking();
        }

        currentTarget = interactibleTarget;
        // Add this specific hand collider to our active tracking set
        activeHoverColliders.Add(interactorCollider);

        // Only fire the highlight and UI text initialization if this is the FIRST hand entering
        if (activeHoverColliders.Count == 1)
        {
            currentTarget.OnHoverEnter();

            if (m_info != null)
            {
                m_info.ClearAllSpawnedElements();
                m_info.AddText("press");
                m_info.AddSprite("rtrigger");
                m_info.AddText("or");
                m_info.AddSprite("ltrigger");
                m_info.AddText("to interact");
                m_info.RequestInfoDisappear(5f, 1f);
            }
        }
    }

    public void RegisterHoverExit(BaseInteractible interactibleTarget, Collider interactorCollider)
    {
        if (currentTarget == null || currentTarget != interactibleTarget) return;

        if (!activeHoverColliders.Contains(interactorCollider)) return;

        if (isInspecting)
            return;

        // Remove this hand from the tracking set
        activeHoverColliders.Remove(interactorCollider);

        // ONLY fire the exit highlight/UI drop if ALL hands have left the object completely
        if (activeHoverColliders.Count == 0)
        {
            currentTarget.OnHoverExit();
            currentTarget = null;

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

    private void TryInteract()
    {
        if (currentTarget == null)
            return;

        if(!CanInteract())
            return;

        currentTarget.Interact();
        if (m_info != null)
        {
            m_info.ClearAllSpawnedElements();
            m_info.RequestInfoDisappear(0f, 0f);
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
    }

    public bool CanInteract()
    {
        return currentTarget != null;
    }
}
