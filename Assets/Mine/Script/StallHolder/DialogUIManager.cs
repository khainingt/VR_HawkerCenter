using System.Collections;
using UnityEngine;
using TMPro;

public class DialogUIManager : MonoBehaviour
{
    [Header("References")]
    public NPCTalkManager talkManager;   // Provides text/voice lines
    public TMP_Text dialogText;          // TMP text UI
    public AudioSource audioSource;      // AudioSource for voices

    [Header("Typing Settings")]
    public float charInterval = 0.03f;

    private bool isTyping = false;
    private bool isFinished = false;     // Block input after dialog ends
    private Coroutine typingCoroutine;
    private string currentLine;
    private AudioClip currentVoice;

    [Header("Animation")]
    public Animator npcAnimator;
    public string[] talkAnimations; // fill with animation state names in Inspector


    public void StartDialog()
    {
        isFinished = false;   // reset finish lock
        ShowNextLine();       // start first line explicitly
    }

    void Update()
    {
        if (isFinished) return; // stop responding when finished

        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            if (isTyping)
            {
                FinishTypingInstant();
            }
            else
            {
                ShowNextLine();
            }
        }
    }

    private void ShowNextLine()
    {
        if (talkManager != null && talkManager.NextLine(out currentLine, out currentVoice))
        {
            // Play voice
            if (audioSource != null)
            {
                audioSource.Stop();
                if (currentVoice != null)
                {
                    audioSource.clip = currentVoice;
                    audioSource.Play();
                }
            }

            // Start typing
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeLine(currentLine));

            // Play Animation
            if (npcAnimator != null && talkAnimations != null && talkAnimations.Length > 0)
            {
                string clip = talkAnimations[Random.Range(0, talkAnimations.Length)];
                npcAnimator.CrossFadeInFixedTime(clip, 0.05f);
            }

        }
        else
        {
            EndDialog(); // nothing left
        }
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogText.text = "";
        foreach (char c in line)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(charInterval);
        }
        isTyping = false;
    }

    private void FinishTypingInstant()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        dialogText.text = currentLine;
        isTyping = false;
    }

    private void EndDialog()
    {
        isFinished = true;          // lock
        if (audioSource != null) audioSource.Stop();
        gameObject.SetActive(false);

        if (talkManager != null)
        {
            talkManager.RequestEndDialog();
            talkManager.ResetDialog();
        }
    }
}
