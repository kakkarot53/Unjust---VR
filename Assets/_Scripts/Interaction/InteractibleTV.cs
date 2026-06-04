using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class InteractibleTV : BaseInteractible
{
    [Header("TV light effects")]
    [SerializeField] private Light tvProjectionLight;   //this is the light that goes outwards
    [SerializeField] private Light tvEffectLight;       //this is the light that goes INwards
    [Header("TV effects")]
    [SerializeField] private GameObject videoEffect;       //this is the light that goes INwards
    [SerializeField] private VideoPlayer localVideoRenderer;       //this is the light that goes INwards
    [SerializeField] private AudioSource audSrc;       //this is the light that goes INwards

    private Coroutine powerRoutine;
    private SyncedTVManager m_sync;

    public void SetManager(SyncedTVManager mng)
    {
        m_sync = mng;
    }

    public void SetTvVisualState(bool state)
    {
        if (videoEffect != null) videoEffect.SetActive(state);
        if (localVideoRenderer != null) localVideoRenderer.enabled = state;
        if (audSrc != null) audSrc.mute = !state;
    }

    public override void Interact()
    {
        base.Interact();
        if (isInteracting)
        {
            m_sync.AddDisabledCount();
            if (powerRoutine != null) StopCoroutine(powerRoutine);
            powerRoutine = StartCoroutine(HandlePowerTransition(false));

            isInteracting = false;
            this.SetCanInteract(false);
        }
    }

    public IEnumerator HandlePowerTransition(bool turningOn)
    {
        if (turningOn)
        {
            // 1. Instantly spike the neon power up flash
            tvProjectionLight.intensity = 5.5f;
            tvProjectionLight.range = 3f;

            tvEffectLight.intensity = 0.5f;
            tvEffectLight.range = 0.55f; 

            // 2. Wait half a second for screen boot simulation
            yield return new WaitForSeconds(0.5f);

            SetTvVisualState(true);
        }
        else
        {
            // Turn off screen and visuals instantly
            SetTvVisualState(false);

            float elapsed = 0f;
            float duration = 0.3f;
            float startProjection = tvProjectionLight.intensity;
            float startEffect = tvEffectLight.intensity;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                tvProjectionLight.intensity = Mathf.Lerp(startProjection, 0f, t);
                tvEffectLight.intensity = Mathf.Lerp(startEffect, 0f, t);
                yield return null;
            }

            tvProjectionLight.intensity = 0f;
            tvProjectionLight.range = 0f;
            tvEffectLight.intensity = 0f;
            tvEffectLight.range = 0f;
        }

        powerRoutine = null;
    }
}
