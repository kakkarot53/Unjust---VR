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
        isTransitioning = false;

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
        if (!isTransitioning && other.CompareTag("Player"))
        {
            if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
            ExecuteTransition();
        }
    }

    private IEnumerator TimeoutFallbackRoutine()
    {
        yield return new WaitForSeconds(2.0f);

        if (!isTransitioning)
        {
            Debug.Log("<color=yellow>[Transition Timeout]</color> Player didn't enter zone. Forcing courtroom teleport!");
            ExecuteTransition();
        }
    }

    private void ExecuteTransition()
    {
        isTransitioning = true;

        if (EnvironmentChange.instance != null)
        {
            // Call the isolated flash method on EnvironmentChange
            EnvironmentChange.instance.PlayTransitionFlash(
                fadeOutDuration,
                fadeInDuration,
                maxPostValue,
                () => {
                    UnjustGameManager.instance.RequestChangeRoom(3, true);
                }
            );

            StartCoroutine(DisableAfterDelay(fadeOutDuration + fadeInDuration + 0.2f));
        }
        else
        {
            Debug.LogError("[Transition Manager] EnvironmentChange instance could not be found!");

            UnjustGameManager.instance.RequestChangeRoom(3, true);
            gameObject.SetActive(false);
        }
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
