using CS.AudioToolkit;
using System.Collections;
using TMPro;
using UnityEngine;
public class DialoguePlayer : MonoBehaviour
{
    [Header("UI Component Targets")]
    [SerializeField] TMP_Text m_Text;
    [SerializeField] GameObject textBg;

    [Header("Settings")]
    [SerializeField] private float textSpeed = 0.05f;          
    [SerializeField] private float nextDialogueDelay = 2.0f;

    private DialogueItem[] currentLines;
    private Coroutine currDialogueRoutine;
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
    public void PlayDialogueSequence(DialogueItem[] newLines, float initDelay)
    {
        if (newLines == null || newLines.Length == 0) return;

        if (currDialogueRoutine != null)
        {
            // Kill active voice lines instantly if a new interaction forces an interruption override
            StopCurrentVoiceLine();
            StopCoroutine(currDialogueRoutine);
        }

        currentLines = newLines;
        currDialogueRoutine = StartCoroutine(PlayAllDialoguesRoutine(initDelay));
    }

    private IEnumerator PlayAllDialoguesRoutine(float initDelay)
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
            bool hasAudio = !string.IsNullOrEmpty(currentItem.dialogueAudioName);

            // type the car one by one
            Coroutine typeRoutine = StartCoroutine(TypeSentenceRoutine(currentItem.text));

            // play audio
            if (hasAudio)
            {
                AudioController.Play(currentItem.dialogueAudioName);
                while (AudioController.IsPlaying(currentItem.dialogueAudioName))
                {
                    yield return null; // Wait until voice is finished
                }
            }

            // wait till it finished typing
            yield return typeRoutine;
            // wait till delay is finished
            yield return new WaitForSeconds(nextDialogueDelay);
        }

        EndDialogue();
    }
    private IEnumerator TypeSentenceRoutine(string text)
    {
        m_Text.text = "";
        foreach (char letter in text.ToCharArray())
        {
            m_Text.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }
    }
    private void StopCurrentVoiceLine()
    {
        if (currentLines == null || currDialogueRoutine == null) return;

        foreach (var line in currentLines)
        {
            if (!string.IsNullOrEmpty(line.dialogueAudioName) && AudioController.IsPlaying(line.dialogueAudioName))
            {
                AudioController.Stop(line.dialogueAudioName, 0.1f);
            }
        }
    }

    private void EndDialogue()
    {
        m_Text.text = "";
        IsPlaying = false;
        currDialogueRoutine = null;
        currentLines = null;
        textBg.SetActive(false);
    }
}
