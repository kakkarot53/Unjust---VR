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
            UnjustGameManager.instance.RequestChangeRoom(5, true);
            AudioController.Play("gavel-hit");
            PlayerMovementManager.instance.RequestPlayerMovementEnable(true);
        }
    }
}
