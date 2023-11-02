using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0.0f, 5.0f)]
    public float volume = 1.0f;

    [Range(0.0f, 5.0f)]
    public float pitch = 1.0f;
    public float pitchVarience = 0.0f;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
