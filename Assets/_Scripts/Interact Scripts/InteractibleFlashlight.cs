using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractibleFlashlight : MonoBehaviour, IInteractible
{
    [SerializeField]
    private GameObject flashlight;

    private void Start()
    {
        flashlight.SetActive(false);
    }

    public void Interact()
    {
        if (!this.IsWithinViewport(Camera.main, transform))
            return;

        Debug.Log(gameObject.name + " collected!");
        flashlight.SetActive(true);

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
