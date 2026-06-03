using CS.AudioToolkit;
using UnityEngine;

public class InteractibleDoor : BaseInteractible
{
    [Tooltip("Leave it blank if this door is unlocked or doesnt have any key")]
    [SerializeField] private BaseInteractible m_Key;
    //please put the doors inside the doorframe here, 1 if its singular, and 2 if its a double door
    [SerializeField]
    private GameObject[] doors;

    //open door up to a certain angle
    [SerializeField]
    private float doorOpenAngle = 80f;

    //open door in n seconds, original is .5f
    [SerializeField]
    private float doorOpenTime = .5f;

    private InteractibleKeypad m_keypad;

    protected override void Start()
    {
        base.Start();

        m_Key.TryGetComponent<InteractibleKeypad>(out m_keypad);
        if(m_keypad != null)
        {
            this.SetCanInteract(false);

            m_keypad.OnCorrectPassword += OpenDoor;
        }
    }

    public override void Interact()
    {
        base.Interact();
        if (isInteracting)
        {
            OpenDoor();

            isInteracting = false;
            this.SetCanInteract(false);
        }
    }

    public void CloseDoor()
    {
        //AudioController.Play("door-close");

        foreach (GameObject d in doors)
        {
            LeanTween.rotateLocal(d, Vector3.zero, doorOpenTime).setEaseInOutQuad();
        }
        Debug.Log("Closing doors");
    }
    public void OpenDoor()
    {

        for (int i = 0; i < doors.Length; i++)
        {
            // Determine rotation angle based on door index
            float rotationAngle = doorOpenAngle * (i % 2 == 0 ? 1 : -1);

            // Rotate the door around its local Y-axis by the specified angle over the defined time
            LeanTween.rotateAroundLocal(doors[i], Vector3.up, rotationAngle, doorOpenTime).setEaseInOutQuad();
            this.SetCanInteract(false);

        }
    }
}
