using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class InteractibleKeypad : BaseInteractible
{
    [Header("Inspection Overrides")]
    [SerializeField] private Transform parentAnchor;
    [SerializeField] private Transform inspectAnchor;
    [SerializeField] private Vector3 inspectRotationOffset;
    [SerializeField] private GameObject dimmingCanvas;

    [Header("Animation Settings")]
    [SerializeField] private float lerpSpeed = 8.0f;
    [SerializeField] private Vector3 targetScale = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Keypad Values")]
    [SerializeField] private TMP_Text pinText;
    [SerializeField] private InteractibleKeypadButton[] buttons;
    public float PushEffectThreshold;
    [SerializeField] private string keypadValue;
    [SerializeField] private string key;

    [Header("Result Indicator")]
    [SerializeField] Light indicatorLight;
    [SerializeField] float lightRangeAfterZoom;
    [SerializeField] Color correctValue;
    [SerializeField] Color incorrectValue;

    private Collider myCollider;
    private Vector3 oripos;
    private Vector3 oriScale;
    private float orilightRange;
    private Quaternion oriRot;
    private PlayerMovementManager m_move;
    private Coroutine lerpRoutine;

    public Action OnCorrectPassword;

    protected override void Start()
    {
        base.Start();

        oripos = parentAnchor.transform.position;
        oriRot = parentAnchor.transform.rotation;
        oriScale = parentAnchor.transform.localScale;
        orilightRange = indicatorLight.range;

        m_move = PlayerMovementManager.instance;

        myCollider = GetComponent<Collider>();

        if (indicatorLight != null) indicatorLight.color = Color.black; // Keep light turned off initially

        //make sure all buttons subscribe to this so called manager
        foreach (InteractibleKeypadButton button in buttons)
        {
            button.ButtonSetup(this);
        }
    }

    public override void Interact()
    {
        if (lerpRoutine != null)
            return;

        base.Interact();

        if (isInteracting)
        {
            //make bg darker
            dimmingCanvas.SetActive(true);
            //disable movement
            m_move.RequestPlayerMovementEnable(false);

            myCollider.enabled = false;

            //put the object in front of player
            Quaternion targetInspectRotation = Quaternion.Euler(inspectRotationOffset);
            lerpRoutine = StartCoroutine(TransitionObject(inspectAnchor.position, targetInspectRotation, targetScale, lightRangeAfterZoom));
            
            //make sure buttons can interact
            SetButtonsInteractionState(true);
        }
        else
        {
            OnCorrectPassword?.Invoke();

            //make bg normal
            dimmingCanvas.SetActive(false);

            //enable movement
            m_move.RequestPlayerMovementEnable(true);
            myCollider.enabled = true;

            //put the object back
            lerpRoutine = StartCoroutine(TransitionObject(oripos, oriRot, oriScale, orilightRange));

            //make sure buttons colliders are disabled so doesnt distrupt this one
            SetButtonsInteractionState(false);
            this.SetCanInteract(false);
        }
    }

    private void SetButtonsInteractionState(bool state)
    {
        foreach (InteractibleKeypadButton button in buttons)
        {
            if (button != null)
            {
                button.SetCanInteract(state);
            }
        }
    }

    public void ButtonInteract(string stringValue)
    {
        string processedInput = stringValue.Trim().ToLower();

        // 1. Process Backspace / Clear Action
        if (processedInput == "back" || processedInput == "clear")
        {
            if (keypadValue.Length > 0)
            {
                keypadValue = keypadValue.Substring(0, keypadValue.Length - 1);
            }

            // Update the screen text instantly on delete before exiting!
            pinText.text = keypadValue;
            return;
        }

        // shouldve been enter but becomes quit
        if (processedInput == "enter")
        {
            Interact();
            return;
        }

        // 3. Process Regular Digit Addition
        if (keypadValue.Length < key.Length)
        {
            keypadValue += stringValue;
        }

        // Always keep the 3D world space text synced up
        pinText.text = keypadValue;

        if (keypadValue.Length >= key.Length)
        {
            CheckPasscode();
        }
    }

    private bool CheckPasscode()
    {
        if (keypadValue == key)
        {
            Debug.Log("<color=green>[Keypad Success] Passcode Correct!</color>");
            if (indicatorLight != null) indicatorLight.color = correctValue;

            // Lock inputs on buttons since puzzle is solved completely
            SetButtonsInteractionState(false);
            pinText.text = keypadValue;

            StartCoroutine(AutoQuitFocusDelay());

            return true;
        }
        else
        {
            Debug.Log("<color=red>[Keypad Denied] Incorrect Code. Wiping data buffer...</color>");
            if (indicatorLight != null) StartCoroutine(FlashIncorrectIndicator());

            keypadValue = ""; // Wipe the buffer string out for fresh attempts
            pinText.text = keypadValue;

            return false;
        }
    }

    private IEnumerator FlashIncorrectIndicator()
    {
        indicatorLight.color = incorrectValue;
        yield return new WaitForSeconds(0.75f);
        // Only return to black if a successful answer didn't overwrite it mid-flash
        if (keypadValue == "") indicatorLight.color = Color.black;
    }

    private IEnumerator AutoQuitFocusDelay()
    {
        // Wait 1.2 seconds so the player can see the light turn green and read their submitted code
        yield return new WaitForSeconds(1.2f);

        // If the player hasn't already closed it manually via enter key during the delay, run the close sequence
        if (isInteracting)
        {
            Interact();
        }
    }

    private IEnumerator TransitionObject(Vector3 targetPosition, Quaternion targetRotation, Vector3 destinationScale, float lightRange)
    {
        // FIX: Using Vector3.Distance for scaling instead of Quaternion.Lerp (which broke scale logic completely)
        while (Vector3.Distance(parentAnchor.transform.position, targetPosition) > 0.001f ||
               Quaternion.Angle(parentAnchor.transform.rotation, targetRotation) > 0.1f ||
               Vector3.Distance(parentAnchor.transform.localScale, destinationScale) > 0.001f)
        {
            parentAnchor.transform.position = Vector3.Lerp(parentAnchor.transform.position, targetPosition, Time.deltaTime * lerpSpeed);
            parentAnchor.transform.rotation = Quaternion.Lerp(parentAnchor.transform.rotation, targetRotation, Time.deltaTime * lerpSpeed);
            parentAnchor.transform.localScale = Vector3.Lerp(parentAnchor.transform.localScale, destinationScale, Time.deltaTime * lerpSpeed);
            indicatorLight.range = lightRange;
            yield return null;
        }

        parentAnchor.transform.position = targetPosition;
        parentAnchor.transform.rotation = targetRotation;
        parentAnchor.transform.localScale = destinationScale;
        indicatorLight.range = lightRange;

        lerpRoutine = null;
    }
}
