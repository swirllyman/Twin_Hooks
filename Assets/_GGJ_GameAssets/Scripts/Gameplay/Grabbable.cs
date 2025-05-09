using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer myRend;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] Color flashColor;
    [SerializeField] float flashTime = 1.0f;
    [SerializeField] LeanTweenType tweenType;
    [SerializeField] AudioClip hitGroundClip;
    [SerializeField] float minSoundHitSpeed = .01f;
    [SerializeField] float minSoundCD = .1f;

    float soundTimer = 0.0f;
    internal Collider2D myCollider;
    protected Color startColor;
    internal Rigidbody2D myBody;

    private void Start()
    {
        myBody = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        if(myRend == null)
        {
            myRend = GetComponentInChildren<SpriteRenderer>();
        }
        startColor = myRend.color;
        LeanTween.color(myRend.gameObject, flashColor, flashTime).setLoopPingPong().setEase(tweenType);
    }

    private void Update()
    {
        if(soundTimer > 0)
        {
            soundTimer -= Time.deltaTime;
        }
    }


    public virtual void PickUp()
    {
        myCollider.enabled = false;
        myBody.bodyType = RigidbodyType2D.Kinematic;
        myBody.linearVelocity = Vector2.zero;
        myBody.angularVelocity = 0.0f;
    }

    public virtual void Drop()
    {
        LeanTween.cancel(myRend.gameObject);
        myRend.color = startColor;
        myCollider.enabled = true;
        myBody.bodyType = RigidbodyType2D.Dynamic;
        Debug.Log("Dropped");
    }

    public virtual void Throw(Vector3 direction, float force)
    {
        Drop();
        myBody.AddForce(direction * force, ForceMode2D.Impulse);
        myBody.angularVelocity = -55 * force;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(myBody != null && (myBody.linearVelocity.magnitude > minSoundHitSpeed && soundTimer <= 0.0f))
        {
            audioSource.PlayOneShot(hitGroundClip);
            soundTimer = minSoundCD;
        }
    }
}
