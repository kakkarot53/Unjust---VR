using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.XR.Hands.XRHandSubsystemDescriptor;

public class Interactor : MonoBehaviour
{
    [SerializeField] private LayerMask interactingLayer;
    [SerializeField] private LayerMask highlightLayer;

    [SerializeField] private InteractorManager m_Interactor;

    private BaseInteractible target;
    private Collider myCollider;

    private void Start()
    {
        myCollider = this.GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        int objectLayerMask = 1 << other.gameObject.layer;

        // (Using a bitwise AND operator checks if the layer exists within the mask)
        bool isInteractingLayer = (interactingLayer.value & objectLayerMask) > 0;
        bool isHighlightLayer = (highlightLayer.value & objectLayerMask) > 0;

        if (isInteractingLayer || isHighlightLayer)
        {
            if (other.TryGetComponent<BaseInteractible>(out BaseInteractible interactibleTarget))
            {
                if (target == interactibleTarget) return;

                target = interactibleTarget;

                m_Interactor.RegisterHoverEnter(interactibleTarget, myCollider);
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        int objectLayerMask = 1 << other.gameObject.layer;

        // (Using a bitwise AND operator checks if the layer exists within the mask)
        bool isInteractingLayer = (interactingLayer.value & objectLayerMask) > 0;
        bool isHighlightLayer = (highlightLayer.value & objectLayerMask) > 0;

        if (isInteractingLayer || isHighlightLayer)
        {
            if (other.TryGetComponent<BaseInteractible>(out BaseInteractible interactibleTarget))
            {
                if (target == interactibleTarget)
                {
                    target = null;

                    m_Interactor.RegisterHoverExit(interactibleTarget, myCollider);
                }
            }
        }
    }
}

