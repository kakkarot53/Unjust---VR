using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
public class SubtitleTimelineMarker : Marker, INotification
{
    [SerializeField] private string characterName;
    [TextArea(2, 5)]
    [SerializeField] private string subtitleText;

    [SerializeField] private float cutsceneTextSpeed = 0.01f;
    [SerializeField] private float cutsceneTextDur = 2f;

    public PropertyName id => new PropertyName("TimelineSubtitle");

    public string CharName => characterName;
    public string SubtitleText => subtitleText;
    public float CutsceneTextSpeed => cutsceneTextSpeed;
    public float CutsceneTextDur => cutsceneTextDur;
}
