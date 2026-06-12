using System.Collections;
using TMPro;
using UnityEngine;
public class DialoguePlayer : MonoBehaviour
{
    [Header("UI Component Targets")]
    [SerializeField] TMP_Text m_Text;
    [SerializeField] GameObject textBg;

    [Header("Spatial Audio Setup")]
    [SerializeField] private AudioSource audioSourcePrefab;
    [SerializeField] private Transform playerCameraAnchor;

    [Header("Settings")]
    [SerializeField] private float textSpeed = 0.05f;          
    [SerializeField] private float nextDialogueDelay = 2.0f;

    private DialogueItem[] currentLines;
    private Coroutine currDialogueRoutine;
    private AudioSource currVoice;
    public bool IsPlaying { get; private set; }

    public static DialoguePlayer instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        IsPlaying = false;
        textBg.SetActive(false);
    }
    
    public void ForceEndDialogue()
    {
        if (!IsPlaying) return;

        if (currDialogueRoutine != null)
        {
            StopCoroutine(currDialogueRoutine);
            currDialogueRoutine = null;
        }

        EndDialogue();
    }

    public void PlayDialogueSequence(DialogueObject dialogueObj, float initDelay)
    {
        if (IsPlaying) return;

        DialogueItem[] newLines = dialogueObj.dialogue;

        if (newLines == null || newLines.Length == 0) return;

        if (currDialogueRoutine != null)
        {
            // Kill active voice lines instantly if a new interaction forces an interruption override
            StopCurrentVoiceLine();
            StopCoroutine(currDialogueRoutine);
        }

        currentLines = newLines;
        //Transform targetSpawnLocation = speakerTransform != null ? speakerTransform : playerCameraAnchor; if i ever need another place to say shit

        currDialogueRoutine = StartCoroutine(PlayAllDialoguesRoutine(playerCameraAnchor, initDelay));
    }

    private IEnumerator PlayAllDialoguesRoutine(Transform speakerAnchor, float initDelay)
    {
        IsPlaying = true;
        textBg.SetActive(false);
        m_Text.text = "";

        if (initDelay > 0f)
        {
            yield return new WaitForSeconds(initDelay);
        }

        textBg.SetActive(true);

        for (int i = 0; i < currentLines.Length; i++)
        {
            DialogueItem currentItem = currentLines[i];
            bool hasAudio = currentItem.dialogueAudio != null;

            Coroutine typeRoutine = StartCoroutine(TypeSentenceRoutine(currentItem.text));

            if (hasAudio && audioSourcePrefab != null && speakerAnchor != null)
            {
                // 1. Spawn a clean audio source prefab container directly at the speaker coordinates
                currVoice = Instantiate(audioSourcePrefab, speakerAnchor.position, speakerAnchor.rotation);

                // Keep the speaker instance glued to moving characters (like walking NPCs)
                currVoice.transform.SetParent(speakerAnchor);

                // 2. Assign the clip properties and hit play
                currVoice.clip = currentItem.dialogueAudio;
                currVoice.Play();

                // 3. Keep the frame ticker processing alive until the audio clip finishes playing entirely
                while (currVoice != null && currVoice.isPlaying)
                {
                    yield return null;
                }

                // 4. Clean the clip instance out of the game's active memory pool instantly
                if (currVoice != null)
                {
                    Destroy(currVoice.gameObject);
                    currVoice = null;
                }
            }

            yield return typeRoutine;
            yield return new WaitForSeconds(nextDialogueDelay);
        }

        EndDialogue();
    }
    private IEnumerator TypeSentenceRoutine(string text)
    {
        Debug.Log($"<color=grey>[Player: ]</color> {text}");

        m_Text.text = "";
        foreach (char letter in text.ToCharArray())
        {
            m_Text.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }
    }
    private void StopCurrentVoiceLine()
    {
        if (currVoice != null)
        {
            currVoice.Stop();
            Destroy(currVoice.gameObject);
            currVoice = null;
        }
    }

    private void EndDialogue()
    {
        StopCurrentVoiceLine();
        m_Text.text = "";
        IsPlaying = false;
        currDialogueRoutine = null;
        currentLines = null;
        textBg.SetActive(false);
    }
}
