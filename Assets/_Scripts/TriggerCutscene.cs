using CS.AudioToolkit;
using UnityEngine;
using UnityEngine.Playables;

public class TriggerCutscene : MonoBehaviour
{
    [SerializeField] private PlayableDirector apartmentCutscene;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Player Hit ! playing cutscene");
            AudioController.StopCategory("SFX", 0.1f);
            PlayerMovementManager.instance.RequestPlayerMovementEnable(false);
            apartmentCutscene.Play();
        }
    }

    private void OnEnable()
    {
        if (apartmentCutscene != null)
        {
            apartmentCutscene.stopped += OnCutsceneFinish;
        }
    }

    private void OnDisable()
    {
        if (apartmentCutscene != null)
        {
            apartmentCutscene.stopped -= OnCutsceneFinish;
        }
    }

    private void OnCutsceneFinish(PlayableDirector director)
    {
        if (director == apartmentCutscene)
        {
            EnvironmentChange.instance.StartEyeCloseEffect(.1f, .8f, .1f, .85f, .95f, 5);
            LeanTween.delayedCall(gameObject, .5f, () =>
            {
                AudioController.Play("gavel-hit");
                LeanTween.delayedCall(gameObject, 0.2f, () =>
                {
                    PlayerMovementManager.instance.RequestPlayerMovementEnable(true);
                });
            });
        }
    }
}
