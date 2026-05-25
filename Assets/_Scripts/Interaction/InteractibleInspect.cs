using Unity.VisualScripting;
using UnityEngine;

public class InteractibleInspect : BaseInteractible
{
    [Header("Inspection Overrides")]
    [SerializeField] private Transform inspectAnchor;

    public override void Interact()
    {
        isInteracting = !isInteracting;

        if (isInteracting)
        {
            // 1. Move object in front of player face
            // 2. Disable movement
            gameObject.layer = originalLayerIndex; // Remove outline while looking close
        }
        else
        {
            // 1. Put object back on desk
            // 2. Enable movement
            gameObject.layer = originalLayerIndex;
        }
    }
}
