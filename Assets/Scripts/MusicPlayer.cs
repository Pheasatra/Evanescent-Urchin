using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioClip music1;
    public AudioClip music2;

    public float volume = 1.0f;

    public AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (source.isPlaying)
        {
            case false:
                switch (Random.Range(0, 2))
                {
                    case 0:
                        source.clip = music1;
                        break;

                    case 1:
                        source.clip = music2;
                        break;
                }
                source.volume = volume;

                source.Play();
                break;
        }
    }
}
