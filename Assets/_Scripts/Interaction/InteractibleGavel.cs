using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using CS.AudioToolkit;
public class InteractibleGavel : BaseInteractible
{
    [Header("Timeline Settings")]
    [SerializeField] private PlayableDirector gavelTimeline;

    [SerializeField] private DialogueObject dialogueObj;

    private bool isAnimationPlaying = false;

    //protected override void Start()
    //{
    //    base.Start();

    //    if (gavelTimeline != null)
    //    {
    //        gavelTimeline.stopped += OnTimelineFinished;
    //    }
    //}

    private void OnEnable()
    {
        if (gavelTimeline != null)
        {
            gavelTimeline.stopped += OnTimelineFinished;
        }
    }

    private void OnDisable()
    {
        if (gavelTimeline != null)
        {
            gavelTimeline.stopped -= OnTimelineFinished;
        }
    }
    public override void Interact()
    {
        if (!CanInteract() || isAnimationPlaying || gavelTimeline == null)
            return;

        base.Interact();

        if (isInteracting)
        {
            isAnimationPlaying = true;

            gavelTimeline.Play();
        }
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        if (director == gavelTimeline)
        {
            DialoguePlayer.instance.ForceEndDialogue();

            isAnimationPlaying = false;

            EnvironmentChange.instance.TriggerDimensionShift();
            EnvironmentChange.instance.StartHeadacheEffect();

            AudioController.Play("gavel-hit");

            DialoguePlayer.instance.PlayDialogueSequence(dialogueObj, 0);

            isInteracting = false;
            gameObject.layer = originalLayerIndex;

            SetCanInteract(false);
        }
    }
}
