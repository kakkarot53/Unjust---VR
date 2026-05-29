using UnityEngine;
using TMPro;
using System.Collections;
public class DialoguePlayer : MonoBehaviour
{
    [SerializeField] TMP_Text m_Text;
    [SerializeField] GameObject textBg;
    [SerializeField][TextArea(3, 5)] private string[] dialogues;

    [Header("Settings")]
    [SerializeField] private float textSpeed = 0.05f;          
    [SerializeField] private float nextDialogueDelay = 2.0f;

    private Coroutine currDialogueRoutine;
    public bool IsPlaying { get; private set; }
    private void Start()
    {
        IsPlaying = false;
        if (dialogues != null && dialogues.Length > 0)
        {
            PlayDialogue();
        }
        else
        {
            textBg.SetActive(false);
        }
    }

    public void InjectandPlay(string[] newDialogues)
    {
        // Safety check 
        if (newDialogues == null || newDialogues.Length == 0)
        {
            Debug.LogWarning("InjectandPlay called with empty or null dialogue array.");
            return;
        }

        // 1. Force stop any running type-writer effects or delay counts instantly
        if (currDialogueRoutine != null)
        {
            StopCoroutine(currDialogueRoutine);
            currDialogueRoutine = null;
        }

        // 2. Overwrite the old array reference with the new, injected dataset
        this.dialogues = newDialogues;

        // 3. Flip IsPlaying to false so PlayDialogue passes its guard clauses, then start it up
        IsPlaying = false;
        PlayDialogue();
    }

    private void PlayDialogue()
    {
        if (IsPlaying)
            return;

        textBg.SetActive(true);
        if (currDialogueRoutine != null)
        {
            StopCoroutine(currDialogueRoutine);
        }

        currDialogueRoutine = StartCoroutine(PlayAllDialoguesRoutine());
    }

    private IEnumerator PlayAllDialoguesRoutine()
    {
        IsPlaying = true;

        for (int i = 0; i < dialogues.Length; i++)
        {
            // Run the character-by-character animation and wait until it completely finishes typing
            yield return StartCoroutine(TypeSentenceRoutine(dialogues[i]));

            // Sentence is fully typed! Wait for the player to read it before clearing/moving on
            yield return new WaitForSeconds(nextDialogueDelay);
        }

        EndDialogue();
    }
    private IEnumerator TypeSentenceRoutine(string text)
    {
        m_Text.text = ""; // Clear the text field before typing starts

        foreach (char letter in text.ToCharArray())
        {
            m_Text.text += letter;

            // maybe add sound here

            yield return new WaitForSeconds(textSpeed);
        }
    }

    private void EndDialogue()
    {
        m_Text.text = "";
        IsPlaying = false;
        currDialogueRoutine = null;
        textBg.SetActive(false);
    }
}
