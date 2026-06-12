using UnityEngine;

[CreateAssetMenu(fileName = "DialogueObject", menuName = "Scriptable Objects/DialogueObject")]
public class DialogueObject : ScriptableObject
{
    public int ID;

    public DialogueItem[] dialogue;
}

[System.Serializable]
public class DialogueItem
{
    [TextArea(3, 5)] public string text;
    public AudioClip dialogueAudio;
}