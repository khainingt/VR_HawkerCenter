using System.Collections;
using UnityEngine;

public class NPCVoice : MonoBehaviour
{
    public AudioClip[] voiceLines;  
    private AudioSource audioSource;  
    private Animator animator;
    private bool hasPlayed = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Waving") && !audioSource.isPlaying && !hasPlayed)
        {
            PlayVoiceLine(0);
            hasPlayed = true;
        } else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Happy Idle"))
        {
            hasPlayed = false;
        }
    }

    void PlayVoiceLine(int index)
    {
        if (voiceLines.Length > index && voiceLines[index] != null)
        {
            audioSource.clip = voiceLines[index];  
            audioSource.Play(); 
        }
    }
}
