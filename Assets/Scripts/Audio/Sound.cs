using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [HideInInspector ] public AudioSource source;
    [Range(0, 1)] public float volume;
    [Range(0, 5)] public float pitch;
    public bool loop;
}
