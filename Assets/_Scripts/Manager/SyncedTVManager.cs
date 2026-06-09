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

    private int disabledCount;
    UnjustGameManager m_game;

    private void Start()
    {
        m_game = UnjustGameManager.instance;

        m_game.OnRoomChange += SetupHall;

        if(televisions.Length<=0)
            return;

        foreach (InteractibleTV t in televisions)
        {
            t.SetManager(this);
        }
    }

    private void OnDestroy()
    {
        if (m_game != null)
        {
            m_game.OnRoomChange -= SetupHall;
        }
    }

    private void SetupHall(int _i)
    {
        //make sure setup runs on room 1 so when player enter hall its all good
        if (_i != 1)
            return;

        disabledCount = 0;
        foreach (InteractibleTV t in televisions)
        {
            if (t != null) t.SetTvVisualState(false);
        }

        if (extraDecor.Length > 0)
            SetactiveStateExtraDecor(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        //if(m_game.currentRoomIndex == 2)
        //    return;

        if (other.CompareTag("Player"))
        {
            if (startDoor != null)
                startDoor.CloseDoor();

            m_game.InformRoomChange(2);

            foreach (InteractibleTV t in televisions)
            {
                t.StartCoroutine(t.HandlePowerTransition(true));
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
