using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class AudioPlayAndDestroy : MonoBehaviour
{
    AudioSource myAudio;
    // Start is called before the first frame update
    void Awake()
    {
        myAudio = GetComponent<AudioSource>();
    }
    public void InitializeAudioSource(AudioClip _ac)
    {
        myAudio.clip = _ac;
    }
    public void InitializeAudioSource(AudioClip _ac, float _v, float _p)
    {
        myAudio.clip = _ac;
        myAudio.volume = _v;
        myAudio.pitch = _p;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (myAudio.time >= myAudio.clip.length)
            Destroy(gameObject);
    }
    
}
