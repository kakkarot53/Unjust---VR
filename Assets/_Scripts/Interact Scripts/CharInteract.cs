using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharInteract : MonoBehaviour
{
    [SerializeField] private Player_UIManager playerUI;
    [SerializeField] private LayerMask interactibleLayer; 
    [SerializeField] private float interactionRange = 2f; 
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private IInteractible nearestInteractible;

    void Update()
    {
        DetectNearestInteractible();
        HandleInteraction();
    }

    // Detect nearest interactible objects within range
    private void DetectNearestInteractible()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange, interactibleLayer);

        float nearestDistance = Mathf.Infinity;
        nearestInteractible = null;

        foreach (Collider hitCollider in hitColliders)
        {
            IInteractible interactible = hitCollider.GetComponent<IInteractible>();
            if (interactible != null)
            {
                float distanceToInteractible = Vector3.Distance(transform.position, hitCollider.transform.position);

                if (distanceToInteractible < nearestDistance)
                {
                    nearestDistance = distanceToInteractible;
                    nearestInteractible = interactible;
                }
            }
        }

        if (nearestInteractible != null)
        {
            playerUI.ShowInteractText(nearestInteractible.CanInteract()); // show popup if there are an interactible near the player
            Debug.DrawLine(transform.position, ((MonoBehaviour)nearestInteractible).transform.position, Color.red);
        }
        //else
        //{
        //    playerUI.ShowInteractText(false);
        //}
    }

    // Handle interaction with the nearest interactible
    private void HandleInteraction()
    {
        if (nearestInteractible != null && Input.GetKeyDown(interactKey))
        {
            nearestInteractible.Interact(); // Call Collect() on the nearest interactible
        }
    }

    // Draw a wire sphere in the Scene view to visualize the interaction range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; // Set the color of the wire sphere
        Gizmos.DrawWireSphere(transform.position, interactionRange); // Draw the wire sphere around the player
    }

}
