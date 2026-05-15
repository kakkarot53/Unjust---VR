using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CS.AudioToolkit;
public class InteractibleDoor : MonoBehaviour, IInteractible
{
    [Tooltip("Leave it blank if this door is unlocked or doesnt have any key")]
    [SerializeField] private string keyName;
    //please put the doors inside the doorframe here, 1 if its singular, and 2 if its a double door
    [SerializeField]
    private GameObject[] doors; 

    //open door up to a certain angle
    [SerializeField]
    private float doorOpenAngle = 80f;

    //open door in ... seconds, original is .5f
    [SerializeField]
    private float doorOpenTime = .5f;

    private Transform player;
    private bool isOpen = false;

    //this is used to calculate how the door opening thing works better
    private bool isReversed = false;
    private float openAngleThreshold = 90f;
    private bool locked = false;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        CloseDoor();
        if(transform.eulerAngles.y < 180)
        {
            isReversed = true;
        }

        if (!string.IsNullOrEmpty(keyName))
            locked = true;
    }

    public void Interact()
    {
        if (!this.IsWithinViewport(Camera.main, transform))
            return;

        if (SimpleInventory.instance.CheckKey(keyName) == true)
            locked = false;

        if (locked)
            return;
        // Calculate the direction from the door to the player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Calculate the angle between the door's forward direction and the direction to the player
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Determine which side the player is on and play animation accordingly

        if (!isOpen)
        {
            if (!isReversed)
            {
                if (angleToPlayer < openAngleThreshold) // In front of the door
                {
                    OpenDoor(true);
                }
                else if (angleToPlayer > 180 - openAngleThreshold) // Behind the door
                {
                    OpenDoor(false);
                }
            }
            else
            {
                if (angleToPlayer > openAngleThreshold) // In front of the door
                {
                    OpenDoor(true);
                }
                else if (angleToPlayer < 180 - openAngleThreshold) // Behind the door
                {
                    OpenDoor(false);
                }
            }
            isOpen = true;
        }
        else
        {
            CloseDoor();
            isOpen = false;
        }
    }

    private void CloseDoor()
    {
        AudioController.Play("door-close");

        foreach (GameObject d in doors)
        {
            LeanTween.rotateLocal(d, Vector3.zero, doorOpenTime).setEaseInOutQuad();
        }
        Debug.Log("Closing doors");
    }
    private void OpenDoor(bool forward)
    {
        float angleMultiplier = forward ? 1 : -1;

        AudioController.Play("door-open");

        for (int i = 0; i < doors.Length; i++)
        {
            // Determine rotation angle based on door index
            float rotationAngle = angleMultiplier * doorOpenAngle * (i % 2 == 0 ? 1 : -1);

            // Rotate the door around its local Y-axis by the specified angle over the defined time
            LeanTween.rotateAroundLocal(doors[i], Vector3.up, rotationAngle, doorOpenTime).setEaseInOutQuad();
        }
    }

    public bool CanInteract()
    {
        if (!this.IsWithinViewport(Camera.main, transform))
            return false;
        else
            return true;
    }
}
