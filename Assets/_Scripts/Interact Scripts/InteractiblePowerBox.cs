using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Probably don't need this, but the power box door shouldn't be able to close
// TOOD: Once the player opens the box, the interact button should then move the switch up
// TOOD: Then the object should no longer be interactible

public class InteractiblePowerBox : MonoBehaviour, IInteractible {
    [SerializeField] private GameObject[] powerDoor;
    [SerializeField] private float doorOpenAngle = 80f;
    [SerializeField] private float doorOpenTime = .5f;
    [SerializeField] private GameObject switchObject; // Reference to the switch object
    [SerializeField] private GameObject allLights;
    private Transform player;
    private bool isOpen = false;
    private bool isSwitchOn = false;

    private bool isReversed = false;
    private float openAngleThreshold = 90f;
    private bool locked = false;
    

    private void Start() {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (transform.eulerAngles.y < 180) {
            isReversed = true;
        }
    }

    public void Interact() {
        if (!this.IsWithinViewport(Camera.main, transform))
            return;
        if (locked)
            return;

        if (!isOpen) {
            OpenDoor();
            isOpen = true;
        } else if (!isSwitchOn) {
            MoveSwitchUp();
            isSwitchOn = true;
            locked = true; // Make the object no longer interactible
        }
    }

    private void OpenDoor() {
        float angleMultiplier = 1;

        for (int i = 0; i < powerDoor.Length; i++) {
            float rotationAngle = angleMultiplier * doorOpenAngle * (i % 2 == 0 ? 1 : -1);
            LeanTween.rotateAroundLocal(powerDoor[i], Vector3.up, rotationAngle, doorOpenTime).setEaseInOutQuad();
        }
    }

    private void MoveSwitchUp() {
        // Example: Move the switch up by changing its position or rotation
        LeanTween.moveLocalY(switchObject, switchObject.transform.localPosition.y + 0.2f, 0.5f).setEaseInOutQuad();
        allLights.SetActive(true);
        Debug.Log("Switch moved up");
    }

    public bool CanInteract() {
        if (!this.IsWithinViewport(Camera.main, transform))
            return false;
        else
            return true;
    }
}