using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFootstepsSounds : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClipArray;
    [SerializeField, Range(0.1f, 1f)] private float playbackWaitTime = 0.25f;
    [SerializeField, Range(0.1f, 1f)] private float playbackVolume = 1f;
    private bool isPlaying;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound()
    {
        if (audioSource == null || audioClipArray.Length <= 0)
            return;

        if (!isPlaying)
        {
            //Save the first position in the array (0) for the previous played sound.
            //This way we will never hear the same sound twice in sequence.
            int n = Random.Range(1, audioClipArray.Length);

            isPlaying = true;
            audioSource.clip = audioClipArray[n];
            audioSource.volume = playbackVolume;
            audioSource.PlayOneShot(audioSource.clip);

            //Wait a moment
            StartCoroutine(PlaySoundReadyRoutine());

            //Set the clip that just played into the 0 position of the array as to not get called twice
            audioClipArray[n] = audioClipArray[0];
            audioClipArray[0] = audioSource.clip;
        }
    }

    public void StopSound()
    {
        audioSource.Stop();
    }

    IEnumerator PlaySoundReadyRoutine()
    {
        //Reset isPlaying bool to allow the next sound playback
        yield return new WaitForSeconds(playbackWaitTime);
        isPlaying = false;
    }
}
