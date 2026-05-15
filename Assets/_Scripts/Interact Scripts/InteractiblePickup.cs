using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiblePickup : MonoBehaviour, IInteractible
{
    public void Interact()
    {
        if (!this.IsWithinViewport(Camera.main, transform))
            return;

        Debug.Log(gameObject.name + " collected!");

        Destroy(gameObject); // Example: Destroy the object when collected
    }

    public bool CanInteract()
    {
        if (!this.IsWithinViewport(Camera.main, transform))
            return false;
        else
            return true;
    }

}
