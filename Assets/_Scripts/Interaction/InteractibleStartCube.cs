using CS.AudioToolkit;
using UnityEngine;

public class InteractibleStartCube : BaseInteractible
{
    private Vector3 localOriginalPosition;

    protected override void Start()
    {
        base.Start();
        localOriginalPosition = transform.localPosition;
    }

    public override void Interact()
    {
        if (!CanInteract())
            return;

        base.Interact();

        if (isInteracting)
        {
            float pressedLocalX = localOriginalPosition.x - 0.2f;
            LeanTween.cancel(this.gameObject);

            //play audio
            AudioController.Play("keypad_press");

            LeanTween.moveLocalX(this.gameObject, pressedLocalX, 0.08f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                // Return smoothly to the precise original layout coordinates
                LeanTween.moveLocalX(this.gameObject, localOriginalPosition.x, 0.12f)
                    .setEase(LeanTweenType.easeInQuad)
                    .setOnComplete(() =>
                    {
                        // Safely flip your interaction state flag back here once the movement cycle finishes
                        isInteracting = false;
                        gameObject.layer = originalLayerIndex;

                        UnjustGameManager.instance.RequestChangeRoom(1, true);
                    });
            });

        }
    }
}
