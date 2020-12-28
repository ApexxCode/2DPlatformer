using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRandomSounds : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClipArray;

    private bool isPlaying;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound()
    {
        if (audioSource == null || audioClipArray.Length <= 0)
            return;

        if (!isPlaying)
        {
            isPlaying = true;
            audioSource.clip = audioClipArray[Random.Range(0, audioClipArray.Length)];
            audioSource.Play();
            StartCoroutine(PlaySoundReadyRoutine());
        }
    }

    IEnumerator PlaySoundReadyRoutine()
    {
        yield return new WaitForSeconds(0.25f);
        isPlaying = false;
    }
}
