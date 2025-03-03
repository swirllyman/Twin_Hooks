using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOnCollide2D : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioSource myAudio;
    //public AudioClip damage;
    void Start()
    {
        if (!myAudio)
            myAudio = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Environment") || collision.gameObject.CompareTag("PhysObj") || collision.gameObject.CompareTag("Enemy"))
        {
            myAudio.Stop();
            myAudio.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
