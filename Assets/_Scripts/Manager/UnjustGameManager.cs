using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.XR.Hands.XRHandSubsystemDescriptor;

public class UnjustGameManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The tracking space root object (e.g., [XR Origin], OVRCameraRig, or your Player object)")]
    [SerializeField] private Transform playerXROrigin; //main player GameObject
    [Tooltip("The Main Camera tracking the player's headset coordinates.")]
    [SerializeField] private Transform playerCamera; //middle eye anchor

    [Header("Room Database Management")]
    [SerializeField] private List<RoomData> roomDatabase = new List<RoomData>();

    public int currentRoomIndex = 0;
    private EnvironmentChange m_env;
    private InteractionInfoManager m_info;

    public static UnjustGameManager instance;
    private InputSystem input;

    private int trialCounter = 0;

    public Action<int> OnRoomChange;
    private void Awake()
    {
        instance = this;

        input = new InputSystem();
        input.Interaction.Enable();

        input.Interaction.ChangeLevel.started += ctx => {
            trialCounter++;
            // Safety Check: Bounds loop control back to index 0 cleanly
            if (trialCounter >= roomDatabase.Count)
                trialCounter = 0;

            RequestChangeRoom(trialCounter, true);
        };
    }

    private void Start()
    {
        m_env = EnvironmentChange.instance;
        m_info = InteractionInfoManager.instance;

        trialCounter = 0;
        RequestChangeRoom(0, true);
        TriggerRoomTutorials(0);
    }

    // Safety fix: Unsubscribe or disable input to prevent garbage collection memory leaks
    private void OnDestroy()
    {
        if (input != null)
        {
            input.Interaction.Disable();
        }
    }

    public void UpdatePlayerLocationData(int index)
    {
        currentRoomIndex = index;
    }

    //used if player can NOT walk to the next room
    public void RequestChangeRoom(int roomIndex, bool teleport)
    {
        if (roomIndex < 0 || roomIndex >= roomDatabase.Count)
            return;

        // enable disable room
        for (int i = 0; i < roomDatabase.Count; i++)
        {
            if (i >= roomIndex - 1 && i <= roomIndex + 1)
            {
                roomDatabase[i].roomObject.SetActive(true);
            }
            else
            {
                roomDatabase[i].roomObject.SetActive(false);
            }
        }

        //teleporting logic
        if (teleport)
        {
            if (roomDatabase[roomIndex].roomStartAnchor != null)
                TeleportPlayer(roomDatabase[roomIndex].roomStartAnchor);
            else
                Debug.LogWarning($"[Room System] SetupRoom missing roomStartAnchor on index {roomIndex}!");
        }

        currentRoomIndex = roomIndex;
        trialCounter = roomIndex;

        TriggerRoomTutorials(roomIndex);

        if (roomDatabase[roomIndex].startRoomDialogue != null)
        {
            DialogueItem[] setupLines = roomDatabase[roomIndex].startRoomDialogue.dialogue;
            DialoguePlayer.instance.PlayDialogueSequence(setupLines, .5f);
        }

        OnRoomChange?.Invoke(currentRoomIndex);
    }

    //used if player can walk to the next room
    public void InformRoomChange(int roomIndex)
    {
        currentRoomIndex = roomIndex;
        trialCounter = roomIndex;
        OnRoomChange?.Invoke(currentRoomIndex);

    }

    //just in case i need this later
    private void TriggerRoomTutorials(int roomIndex)
    {
        if (m_info == null)
        {
            Debug.LogWarning("[UnjustGameManager] InteractionInfoManager instance is missing!");
            return;
        }

        if (roomIndex == 0)
        {
            m_info.ClearAllSpawnedElements();
            m_info.AddText("Use ");
            m_info.AddSprite("rstick");
            m_info.AddText(" to move around");
            m_info.RequestInfoDisappear(25f, 1f);
        }
        else
        {
            m_info.ClearAllSpawnedElements();
        }
    }

    public void TeleportPlayer(Transform targetLandingAnchor)
    {
        if (playerXROrigin == null || playerCamera == null || targetLandingAnchor == null)
        {
            Debug.LogError("[Teleport] Missing vital assignment references!");
            return;
        }

        // 1. Determine how far away the player's head currently is from their virtual tracking center
        Vector3 headOffset = playerCamera.position - playerXROrigin.position;

        // Flatten the vertical offset out so the player doesn't accidentally spawn in the floor or ceiling
        headOffset.y = 0;

        // 2. Calculate the perfect corrected root destination position
        Vector3 cleanDestination = targetLandingAnchor.position - headOffset;

        // 3. Temporarily disable physics engines or character controllers so Unity doesn't reject the warp
        CharacterController cc = playerXROrigin.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 4. Perform the coordinate warp snap
        playerXROrigin.position = cleanDestination;
        playerXROrigin.rotation = targetLandingAnchor.rotation;

        // 5. Turn physics systems safely back on
        if (cc != null) cc.enabled = true;

        Debug.Log($"<color=magenta>[Teleport] Player successfully warped to {targetLandingAnchor.name}</color>");
    }
}

[System.Serializable]
public class RoomData
{
    public string name;

    public GameObject roomObject;

    public Transform roomStartAnchor;

    public DialogueObject startRoomDialogue;
}