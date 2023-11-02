using UnityEngine.Audio;
using System;
using UnityEngine;

// -----------------------------------------------------------------------------------------------------

public class AudioManager : MonoBehaviour
{    
    // Only one of these so we can make it a singleton, also make sure we can't set this
    public static AudioManager audioManager { get; private set; }

    public AudioMixerGroup audioMixerGroup;
    public float rollOff = 5;

    [Range(0.01f, 1.0f)]
    public float volumeRange;
    public float globalVolumeMultiplyer = 1;

    public GameObject soundObject;

    // Our list of sounds
    public Sound[] sounds;


    // -----------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Awake()
    {
        // Set our singleton reference, we do this here for good reasons
        audioManager = this;

        for (int x = 0; x < sounds.Length; x++)
        {
            sounds[x].source = gameObject.AddComponent<AudioSource>();

            sounds[x].source.clip = sounds[x].clip;
            sounds[x].source.volume = sounds[x].volume * globalVolumeMultiplyer;
            sounds[x].source.pitch = sounds[x].pitch;
            sounds[x].source.loop = sounds[x].loop;
            sounds[x].source.outputAudioMixerGroup = audioMixerGroup;
            sounds[x].source.playOnAwake = false;
        }

        Play("Wind");
    }

    // -----------------------------------------------------------------------------------------------------

    public Sound GetSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        switch (s)
        {
            case null:
                Debug.LogWarning("'" + name + "'" + " Cannot be found, did you misspell it?");
                return null;
        }

        return s;
    }

    // -----------------------------------------------------------------------------------------------------

    public void PlayOneShot(string name)
    {
       // SoundPlayer sound =  Instantiate(soundObject).GetComponent<SoundPlayer>();

        Sound s = Array.Find(sounds, sound => sound.name == name);

        switch (s)
        {
            case null:
                Debug.LogWarning("'" + name + "'" + " Cannot be found, did you misspell it?");
                return;
        }

        // sound.PlayOneShot(s, volumeRange);
        // Play the note
        //s.source.Play();

        s.source.pitch = s.pitch + UnityEngine.Random.Range(-s.pitchVarience, s.pitchVarience);

        s.source.PlayOneShot(s.source.clip, Mathf.Clamp(volumeRange / rollOff, 0, volumeRange));

        //AudioSource.PlayClipAtPoint(s.source.clip, position + new Vector3(0, 0, -10));
    }

    // -----------------------------------------------------------------------------------------------------

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        switch (s)
        {
            case null:
                Debug.LogWarning("'" + name + "'" + " Cannot be found, did you misspell it?");
                return;
        }

        s.source.PlayOneShot(s.source.clip, volumeRange);
    }
}
