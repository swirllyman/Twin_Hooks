using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

[RequireComponent (typeof(AudioSource))]

public class BasicElevator : MonoBehaviour
{
    AutoMoveAndRotate myAutoMove;
    ElevatorState myState = ElevatorState.UpOrForward;
    BasicElevator myElevator;
    public float moveSpeed;
    public bool isStopped;
    AudioSource myAudio;
    //public 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        myAudio = GetComponent<AudioSource>();
    }

    void MoveInDirection()
    {
        
        //if uporforward, move positive, else if downorback, move negative
    }
    void ReverseDirection()
    {
        myAutoMove.moveUnitsPerSecond.value *= -1;
    }
    void StopMoving()
    {
        ReverseDirection();
        ToggleMyAutoMove(false);
        //set speed to 0, lerp smoothly to destination
        //set state to stopped
    }

    public void ToggleMyAutoMove(bool _b)
    {
        myAutoMove.enabled = _b;
        if(myAutoMove.enabled)
        {

        }
    }

    private void FixedUpdate()
    {
        //call moveindirection in frame   
    }

    void PlaySoundWhileInside()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!myAudio.isPlaying)
        {
            myAudio.Play();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (myAudio.isPlaying)
        {
            myAudio.Pause();
        }
    }

    // Update is called once per frame
    void Update()
    {
    //    float deltaTime = Time.deltaTime;

    //    if (!isStopped)
    //    {
    //        transform.Translate(direction * moveSpeed * deltaTime, direction.);
    //    }
    }
}
