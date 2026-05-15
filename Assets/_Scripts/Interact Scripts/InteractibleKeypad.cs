using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InteractibleKeypad : MonoBehaviour, IInteractible
{
    [SerializeField]
    private CharMove playerMove;    
    [SerializeField]
    private FirstPersonCamera playerCam;

    [SerializeField]
    private GameObject KeypadPanel;
    [Tooltip("input button in order here")] 
    [SerializeField]
    private Button[] KeypadButtons;

    [SerializeField]
    private string pin;
    private string currPinInp;

    [SerializeField]
    private TMP_Text pinDisplayText;        

    [SerializeField]
    private Button enterButton;  
    [SerializeField]
    private Button clearButton; 

    private const int maxPinLength = 4;

    private void Start()
    {
        foreach (Button button in KeypadButtons)
        {
            string number = button.GetComponentInChildren<Text>().text;
            button.onClick.AddListener(() => AddDigit(number));
        }

        // Set up Enter and Clear button listeners
        enterButton.onClick.AddListener(CheckPin);
        clearButton.onClick.AddListener(ClearPin);
    }

    public void Interact()
    {
        if (!this.IsWithinViewport(Camera.main, transform))
            return;

        //enable and disable movement
        playerMove.enabled = !playerMove.isActiveAndEnabled;
        playerCam.enabled = !playerCam.isActiveAndEnabled;

        // Show or hide the keypad panel
        KeypadPanel.SetActive(!KeypadPanel.activeSelf);

        // Clear the current input if the keypad is being opened
        if (KeypadPanel.activeSelf)
        {
            ClearPin();
        }
    }
    private void AddDigit(string digit)
    {
        if (currPinInp.Length < maxPinLength)
        {
            currPinInp += digit;
            UpdatePinDisplay();
        }

        // If the input exceeds max length, reset
        if (currPinInp.Length > maxPinLength)
        {
            Debug.Log("PIN input exceeded 4 digits. Resetting PIN entry.");
            ClearPin();
        }
    }

    private void CheckPin()
    {
        if (currPinInp == pin)
        {
            // Close keypad panel and re-enable player controls
            KeypadPanel.SetActive(false);
            playerMove.enabled = true;
            playerCam.enabled = true;
            // Pin is correct, unlock or trigger desired action
            // open door or smt add later
        }
        else
        {
            Debug.Log("Incorrect PIN. Try again.");
            ClearPin(); // Reset input on incorrect entry
        }
    }

    public bool CanInteract()
    {
        if (!this.IsWithinViewport(Camera.main, transform))
            return false;
        else
            return true;
    }
    private void ClearPin()
    {
        currPinInp = "";
        UpdatePinDisplay();
    }

    private void UpdatePinDisplay()
    {
        pinDisplayText.text = currPinInp;
    }
}
