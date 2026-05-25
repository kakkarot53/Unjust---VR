using UnityEngine;
using UnityEngine.Windows;

public class Interactor : MonoBehaviour
{
    [SerializeField] private LayerMask interactingLayer;
    [SerializeField] private LayerMask highlightLayer;

    private InputSystem input;
    private BaseInteractible target;
    private bool canInteract;
    private void Awake()
    {
        input = new InputSystem();
        input.Interaction.Enable();

        input.Interaction.Right_Trigger.started += ctx => TryInteract();
        input.Interaction.Left_Trigger.started += ctx => TryInteract();
    }

    private void TryInteract()
    {
        if (target == null || !canInteract)
            return;

        target.Interact();
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
                interactibleTarget.OnHoverEnter();
                
                canInteract = true;
                target = interactibleTarget;
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
                interactibleTarget.OnHoverExit();

                canInteract = false;
                target = null;


            }
        }
    }
}

