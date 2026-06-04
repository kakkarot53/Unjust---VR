using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class InteractibleGavel : BaseInteractible
{
    [Header("Timeline Settings")]
    [SerializeField] private PlayableDirector gavelTimeline;

    [SerializeField] private AudioClip gavelClip;

    private bool isAnimationPlaying = false;

    protected override void Start()
    {
        base.Start();

        if (gavelTimeline != null)
        {
            gavelTimeline.stopped += OnTimelineFinished;
        }
    }

    private void OnDestroy()
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
            isAnimationPlaying = false;

            EnvironmentChange.instance.TriggerDimensionShift();
            EnvironmentChange.instance.StartHeadacheEffect();

            //if(gavelClip != null) 
                //do smy here

            isInteracting = false;
            gameObject.layer = originalLayerIndex;

            SetCanInteract(false);
        }
    }
}
