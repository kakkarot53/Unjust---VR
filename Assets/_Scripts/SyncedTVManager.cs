using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Video;

public class SyncedTVManager : MonoBehaviour
{
    [SerializeField] private InteractibleTV[] televisions;
    [SerializeField] private InteractibleDoor startDoor;
    [SerializeField] private InteractibleDoor finishDoor;

    [SerializeField] private Volume vol;
    private ShadowsMidtonesHighlights shadows;

    private int disabledCount;
    private void Start()
    {
        disabledCount = 0;

        // FIX: Extract the effect safely directly from the Volume's Profile Asset
        if (vol != null && vol.profile != null)
        {
            vol.profile.TryGet<ShadowsMidtonesHighlights>(out shadows);
        }

        // Safely verify we found the asset override profile before altering flags
        if (shadows != null)
        {
            shadows.active = false;
        }
        else
        {
            Debug.LogError("<color=red>[Post-Processing Error]</color> ShadowsMidtonesHighlights override not found inside the assigned Volume Profile!");
        }

        foreach (InteractibleTV t in televisions)
        {
            if (t != null) t.SetTvVisualState(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (startDoor != null)
                startDoor.CloseDoor();
            foreach (InteractibleTV t in televisions)
            {
                t.SetTvVisualState(true);
                t.SetManager(this);
            }
        }

        if (shadows != null)
        {
            shadows.active = true;
        }
    }

    public void AddDisabledCount()
    {
        disabledCount++;
        CheckCount();
    }

    private void CheckCount()
    {
        if(disabledCount >= televisions.Length && finishDoor != null)
        {
            finishDoor.OpenDoor();
        }
    }
}
