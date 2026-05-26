using Unity.VisualScripting;
using UnityEngine;

public class InteractibleInspect : BaseInteractible
{
    //[Header("Inspection Overrides")]
    //[SerializeField] private Transform inspectAnchor;

    public override void Interact()
    {
        base.Interact();

        if (isInteracting)
        {
            //disable movement
        }
        else
        {
            //enable movement
        }
    }
}
