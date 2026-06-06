using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Video;

public class SyncedTVManager : MonoBehaviour
{
    [SerializeField] private InteractibleTV[] televisions;
    [SerializeField] private InteractibleDoor startDoor;
    [SerializeField] private InteractibleDoor finishDoor;

    [Header("extra lights")]
    [SerializeField] private GameObject[] extraDecor;

    private EnvironmentChange m_post;

    private int disabledCount;

    UnjustGameManager m_game;

    private void Start()
    {
        m_game = UnjustGameManager.instance;
        m_post = EnvironmentChange.instance;

        m_game.OnRoomChange += SetupHall;
    }

    private void SetupHall(int _i)
    {
        //make sure setup runs on room 1 so when player enter hall its all good
        if (_i != 1)
            return;

        disabledCount = 0;
        m_post.shadows.active = false;
        foreach (InteractibleTV t in televisions)
        {
            if (t != null) t.SetTvVisualState(false);
        }

        if (extraDecor.Length > 0)
            SetactiveStateExtraDecor(false);
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

        m_post.shadows.active = true;
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

            SetactiveStateExtraDecor(true);
        }
    }

    private void SetactiveStateExtraDecor(bool active)
    {
        foreach(GameObject g in extraDecor)
        {
            g.SetActive(active);
        }
    }
}
