using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class VignettePulseMarker : Marker, INotification
{
    [Header("Pulse Settings Override")]
    [SerializeField] private float timeToPeak = 0.15f;
    [SerializeField] private float holdAtPeak = 0.15f;
    [SerializeField] private float timeToFade = 0.35f;

    [Range(0.25f, 1.0f)]
    [SerializeField] private float targetIntensity = 0.75f;
    [Range(0.35f, 1.0f)]
    [SerializeField] private float targetSmoothness = 0.85f;

    public PropertyName id => new PropertyName("VignettePulse");

    public float TimeToPeak => timeToPeak;
    public float TimeToHold => holdAtPeak;
    public float TimeToFade => timeToFade;
    public float Intensity => targetIntensity;
    public float Smoothness => targetSmoothness;
}
