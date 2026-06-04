using UnityEngine;
using UnityEngine.Video;

public class SyncedTVManager : MonoBehaviour
{
    [SerializeField] private InteractibleTV[] televisions;
    [SerializeField] private InteractibleDoor startDoor;
    [SerializeField] private InteractibleDoor finishDoor;

    private int disabledCount;

    private void Start()
    {
        disabledCount = 0;
        foreach (InteractibleTV t in televisions)
        {
            t.SetTvVisualState(false);
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
