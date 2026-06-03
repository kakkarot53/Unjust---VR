using UnityEngine;
using UnityEngine.Video;

public class SyncedTVManager : MonoBehaviour
{
    [SerializeField] private InteractibleTV[] televisions;
    [SerializeField] private InteractibleDoor door;


    private void Start()
    {
        foreach (InteractibleTV t in televisions)
        {
            t.SetTvVisualState(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (door != null)
                door.CloseDoor();
            foreach (InteractibleTV t in televisions)
            {
                t.SetTvVisualState(true);

            }
        }
    }
}
