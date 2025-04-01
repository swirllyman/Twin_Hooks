using UnityEngine;
using UnityEngine.Tilemaps;

public class GrabbableTerrain : MonoBehaviour
{
    //[SerializeField] protected SpriteRenderer myRend;
    [SerializeField] protected TilemapRenderer myRend;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] Color flashColor;
    [SerializeField] float flashTime = 1.0f;
    [SerializeField] LeanTweenType tweenType;
    [SerializeField] AudioClip hitGroundClip;
    [SerializeField] float minSoundHitSpeed = .01f;
    [SerializeField] float minSoundCD = .1f;
    public GameObject grabbablePrefab;

    float soundTimer = 0.0f;
    internal Collider2D myCollider;
    protected Color startColor;
    internal Rigidbody2D myBody;

    private void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        myRend = GetComponent<TilemapRenderer>();
        //startColor = myRend.color;
        //LeanTween.color(myRend.gameObject, flashColor, flashTime).setLoopPingPong().setEase(tweenType);
    }

    private void Update()
    {
        if (soundTimer > 0)
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
        myCollider.enabled = true;
        myBody.bodyType = RigidbodyType2D.Dynamic;
    }

    public virtual void Throw(Vector3 direction, float force)
    {
        Drop();
        myBody.AddForce(direction * force, ForceMode2D.Impulse);
        myBody.angularVelocity = -55 * force;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (myBody != null && (myBody.linearVelocity.magnitude > minSoundHitSpeed && soundTimer <= 0.0f))
        {
            audioSource.PlayOneShot(hitGroundClip);
            soundTimer = minSoundCD;
        }
    }
}