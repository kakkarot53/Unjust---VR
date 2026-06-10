using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CourtRoomTransition : MonoBehaviour
{

    [Header("Transition Settings")]
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private float fadeInDuration = 0.6f;
    [SerializeField] private float maxPostValue = 10f;

    private bool isTransitioning = false;
    private Coroutine timeoutCoroutine;

    private void OnEnable()
    {
        // FIX 1: Reset state variables so the script knows it's allowed to run again
        isTransitioning = false;

        // FIX 2: Safety check to ensure we don't double-start a coroutine
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
        }

        timeoutCoroutine = StartCoroutine(TimeoutFallbackRoutine());
    }

    private void OnDisable()
    {
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null; // Clear reference
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the player walks into the zone manually before the 2 seconds are up
        if (!isTransitioning && other.CompareTag("Player"))
        {
            // Stop the ticking timeout fallback so it doesn't try to fire twice!
            if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);

            StartCoroutine(FlashAndTeleportRoutine());
        }
    }
    private IEnumerator TimeoutFallbackRoutine()
    {
        // 1. Wait for 2 seconds while the player stands around
        yield return new WaitForSeconds(2.0f);

        // 2. If they haven't touched the collider yet, force execution right now!
        if (!isTransitioning)
        {
            Debug.Log("<color=yellow>[Transition Timeout]</color> Player didn't enter zone. Forcing courtroom teleport!");
            StartCoroutine(FlashAndTeleportRoutine());
        }
    }

    private IEnumerator FlashAndTeleportRoutine()
    {
        isTransitioning = true;

        if (EnvironmentChange.instance == null || EnvironmentChange.instance.colorAdjustments == null)
        {
            Debug.LogError("[Transition] EnvironmentChange or ColorAdjustments is missing!");
            yield break;
        }

        float elapsed = 0f;
        float startPost = EnvironmentChange.instance.colorAdjustments.postExposure.value;

        // FLASH OUT: Blinding screen shift
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            EnvironmentChange.instance.colorAdjustments.postExposure.value = Mathf.Lerp(startPost, maxPostValue, t);
            yield return null;
        }
        EnvironmentChange.instance.colorAdjustments.postExposure.value = maxPostValue;

        // THE WARP: Force SetupRoom on index 3 (Courtroom)
        if (UnjustGameManager.instance != null)
        {
            UnjustGameManager.instance.RequestChangeRoom(3, true);
        }

        yield return new WaitForSeconds(0.1f);

        elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            EnvironmentChange.instance.colorAdjustments.postExposure.value = Mathf.Lerp(maxPostValue, startPost, t);
            yield return null;
        }
        EnvironmentChange.instance.colorAdjustments.postExposure.value = startPost;

        // Clean up this transition manager gameobject
        this.gameObject.SetActive(false);
    }
}
