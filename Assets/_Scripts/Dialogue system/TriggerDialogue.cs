using Unity.VisualScripting;
using UnityEngine;

public class TriggerDialogue : MonoBehaviour
{
    [SerializeField] private DialogueObject _dialogueObject;
    [SerializeField] private float dialogueDelay = .1f;
    [SerializeField] private bool forceEnd;

    [SerializeField]private int levelID;

    private UnjustGameManager m_game;
    private Collider myColl;
    private void Start()
    {
        m_game = UnjustGameManager.instance;
        myColl = this.GetComponent<Collider>();
        this.myColl.enabled = (true);

        m_game.OnRoomChange += ResetCollider;
    }

    private void ResetCollider(int _i)
    {
        if(levelID == _i)
            this.myColl.enabled = (true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(forceEnd)
                DialoguePlayer.instance.ForceEndDialogue();
            DialoguePlayer.instance.PlayDialogueSequence(_dialogueObject, dialogueDelay);
            this.myColl.enabled = (false);
        }
    }
}
