using UnityEngine;
using UnityEngine.Playables;

public class SubtitleTimelineReceiver : MonoBehaviour, INotificationReceiver
{
    //i know its named subtitle but yeah im using it for Vignette to im sorry future me
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is SubtitleTimelineMarker subtitleMarker)
        {
            if (DialoguePlayer.instance != null)
            {
                DialogueItem cutsceneLine = new DialogueItem
                {
                    text = subtitleMarker.SubtitleText,
                    dialogueAudio = null // Timeline handles the audio waveforms separately!
                };

                DialogueItem[] payload = new DialogueItem[] { cutsceneLine };
                DialoguePlayer.instance.PlayCutsceneDialogue(subtitleMarker.CharName ,payload, subtitleMarker.CutsceneTextSpeed, subtitleMarker.CutsceneTextDur);
            }
        }
        if (notification is VignettePulseMarker pulseMarker)
        {
            if (EnvironmentChange.instance != null)
            {
                // Trigger a singular custom-timed breath pump contraction instantly!
                EnvironmentChange.instance.ExecuteSingleGaspPulse(
                    pulseMarker.TimeToPeak,
                    pulseMarker.TimeToHold,
                    pulseMarker.TimeToFade,
                    pulseMarker.Intensity,
                    pulseMarker.Smoothness
                );
            }
        }
    }
}
