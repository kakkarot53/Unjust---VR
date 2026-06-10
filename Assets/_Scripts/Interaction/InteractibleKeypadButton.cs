using Unity.VisualScripting;
using UnityEngine;
using CS.AudioToolkit;
public class InteractibleKeypadButton : BaseInteractible
{
    [SerializeField] private string stringValue;

    private InteractibleKeypad m_keypad;
    private Collider myCollider;

    private Vector3 localOriginalPosition;

    protected override void Start()
    {
        base.Start();
        myCollider = GetComponent<Collider>();
        myCollider.enabled = false;
        localOriginalPosition = transform.localPosition;
    }

    public void ButtonSetup(InteractibleKeypad _k)
    {
        if (_k == null)
            return;
        SetCanInteract(false);
        m_keypad = _k;

        //Debug.Log($"{stringValue} is set up");

    }

    public override void SetCanInteract(bool state)
    {
        base.SetCanInteract(state);
        myCollider.enabled = (state);
    }


    public override void Interact()
    {
        if (!CanInteract() || m_keypad == null)
            return;

        base.Interact();

        if (isInteracting)
        {
            float pressedLocalZ = localOriginalPosition.z - m_keypad.PushEffectThreshold;
            LeanTween.cancel(this.gameObject);

            // Fire character data payload straight up into manager
            m_keypad.ButtonInteract(stringValue);

            //play audio
            AudioController.Play("keypad_press");

            LeanTween.moveLocalZ(this.gameObject, pressedLocalZ, 0.08f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                // Return smoothly to the precise original layout coordinates
                LeanTween.moveLocalZ(this.gameObject, localOriginalPosition.z, 0.12f)
                    .setEase(LeanTweenType.easeInQuad)
                    .setOnComplete(() =>
                    {
                        // Safely flip your interaction state flag back here once the movement cycle finishes
                        isInteracting = false;
                        gameObject.layer = originalLayerIndex;
                    });
            });

        }
    }

}
