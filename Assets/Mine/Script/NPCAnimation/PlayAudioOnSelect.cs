using UnityEngine;

public class PlayAudioOnSelect : MonoBehaviour
{
    public AudioClip pokeSound;  
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void OnSelect()
    {
        if (pokeSound != null)
        {
            audioSource.clip = pokeSound;  
            audioSource.Play();
        }
    }
}

