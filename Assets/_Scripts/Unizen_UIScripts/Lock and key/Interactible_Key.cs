using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactible_Key : MonoBehaviour, IInteractible
{
    [SerializeField]
    private string keyName;

    [SerializeField] private AudioSource keyPickup;

    public void Interact()
    {
        if (!this.IsWithinViewport(Camera.main, transform))
            return;

        SimpleInventory.instance.AddItem(keyName);
        keyPickup.Play();

        Destroy(gameObject);
    }

    public bool CanInteract()
    {
        if (!this.IsWithinViewport(Camera.main, transform))
            return false;
        else
            return true;
    }
}
