using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//TODO: Temporarily disable joints and use linerenderer to find the two spots you actually want. Discern between local and world transforms. 
//Find where the two points are coming from. Investigate where the position is.
//Try setting connectedAnchor to hook inst position.
[RequireComponent(typeof(Rigidbody2D))]
public class GrapBulletScript : MonoBehaviour
{
    FeedbackHandler gunFeedbackTextObj;
    GrapGunSystem grapGun;
    bool isSticking = false;
    Vector2 stickingPosition;
    Rigidbody2D myRB;
    AudioSource myAudio;
    [SerializeField] Renderer m_Renderer;
    [SerializeField] Collider2D m_MyCollider;
    [Header("Audio Clips")]
    public AudioClip fireHookBullet;
    public AudioClip movingForward;
    public AudioClip pullingBack;
    public AudioClip pullObjectToPlayer;
    public AudioClip pullPlayerToObject;
    public AudioClip playerHoldObject;
    public AudioClip blocked;

    [Header("Sticking Audio Clips")]
    public AudioClip stickToSurfaceAndSolidifyBeam;
    public AudioClip stickToEnvSurface;
    public AudioClip stickToPhysObj;
    public AudioClip stickToEnemy;
    public AudioClip unStick;

    public Animation m_MyAnimation;

    public GameObject m_SpriteObj;
    // Start is called before the first frame update
    void Start()
    {
        m_Renderer = GetComponent<Renderer>();
        myRB = GetComponent<Rigidbody2D>();
        myAudio = GetComponent<AudioSource>(); 
    }


    private void Awake()
    {
        ToggleSpriteScale(true);
        m_MyAnimation = GetComponent<Animation>();
        //myAudio.pla
        //if (fireHookBullet != null)
        //    audioSource.PlayOneShot(fireHookBullet);
        //GrapGunEvents.shootHookEvent.AddListener(AssignGunSource());
        GrapGunEvents.hookAttachEvent.AddListener(PlaySurfaceStickSound);
        GrapGunEvents.hookPullTargetToPlayerEvent.AddListener(PlaySurfaceStickSound);
    }
    public void AssignGunSource(GrapGunSystem _grapGun)
    {
        grapGun = _grapGun;
        gunFeedbackTextObj = grapGun.feedbackTextObj;
    }

    void AnimateSticking()
    {
        m_MyAnimation.Play();
    }

    void ToggleSpriteScale(bool _b)
    {
        if(_b)
        {
            m_SpriteObj.transform.localScale = new Vector3(1.5f, .3f, 1);
        }
        else
        {
            m_SpriteObj.transform.localScale = new Vector3(1, 1, 1);
        }
    }
    public void PlayUnstickSound()
    {
        myAudio.Stop();
        myAudio.clip = unStick;
        myAudio.loop = false;
        myAudio.Play();
    }
    public void PlayPullingBackSound()
    {
        myAudio.Stop();
        myAudio.clip = pullingBack;
        myAudio.loop = true;
        myAudio.Play();
    }

    public void StopUnloopSound()
    {
        myAudio.Stop();
        myAudio.loop = false;
    }

    public void PlaySound(AudioClip _c, bool _isLoop)
    {
        myAudio.Stop();
        myAudio.clip = _c;
        myAudio.loop = _isLoop;
        myAudio.Play();
    }


    void Attach(Collision2D col)
    {
        if (isSticking)
            return;
        grapGun.SetTargetObj(col.gameObject);

        myRB.isKinematic = true;
        myRB.Sleep(); //Needed to make the hook stop on env. 

        stickingPosition = col.transform.position;
        //transform.SetParent(col.transform);
        grapGun.SetColliderConnectPoint(transform.position);

        PlaySurfaceStickSound();
        isSticking = true;
        if (col.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            col.gameObject.GetComponent<EnemyController>().AddListeners();
        //GrapGunEvents.hookAttachEvent.Invoke();
    }
    void AttachCollider(Collider2D col)
    {
        if (isSticking)
            return;
        grapGun.SetTargetObj(col.gameObject);

        myRB.isKinematic = true;
        myRB.Sleep(); //Needed to make the hook stop on env. 

        stickingPosition = col.transform.position;
        //transform.SetParent(col.transform);
        grapGun.SetColliderConnectPoint(transform.position);

        PlaySurfaceStickSound();
        isSticking = true;
        if (col.gameObject.layer == LayerMask.NameToLayer("Enemy")) ;
            col.gameObject.GetComponent<EnemyController>().AddListeners();
        //GrapGunEvents.hookAttachEvent.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (grapGun.targetObj == null)
        {            
            if (collision.gameObject.CompareTag("PhysObj"))
            {
                AttachCollider(collision);

                transform.SetParent(collision.transform);

                //GrapGunEvents.hookAttachEvent.Invoke();
            }
            else if (collision.gameObject.CompareTag("Environment") || 
                     collision.gameObject.layer == LayerMask.NameToLayer("Environment"))
            {                
                //GrapGunEvents.hookAttachEvent.Invoke();
                AttachCollider(collision);
            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))//Destroy(gameObject, 0.01f);
            {
                AttachCollider(collision);
                
                var eCon = collision.gameObject.GetComponent<EnemyController>();
                if (eCon.gameObject.GetComponent<AutoMoveSimple>())
                    collision.gameObject.GetComponent<AutoMoveSimple>().ChangeMove(true);
                
                transform.SetParent(collision.transform);

                eCon.SetMyState(EnemyState.Grappled);
            }
            else if(collision.gameObject.CompareTag("Ungrappable"))//TODO: When entering player trigger, Hook only releases when pulling object back
            {
                //Destroy bullet by release
                myAudio.Stop();
                myAudio.PlayOneShot(blocked);
                grapGun.CallRelease();
            }            
        }
        if (grapGun.targetObj)
        {
            ToggleCollider();
            ToggleSpriteScale(false);
            AnimateSticking();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (grapGun.targetObj == null)
        {
            if (collision.gameObject.CompareTag("PhysObj"))
            {

                transform.SetParent(collision.transform);
                Attach(collision);
            }
            else if (collision.gameObject.CompareTag("Environment") ||
                     collision.gameObject.layer == LayerMask.NameToLayer("Environment"))
            {
                Attach(collision);
            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))//Destroy(gameObject, 0.01f);
            {
                Attach(collision);
                var eCon = collision.gameObject.GetComponent<EnemyController>();
                
                if (eCon.gameObject.GetComponent<AutoMoveSimple>())
                    collision.gameObject.GetComponent<AutoMoveSimple>().ChangeMove(true);

                transform.SetParent(collision.transform);
 
                eCon.SetMyState(EnemyState.Grappled);
            }
            else if (collision.gameObject.CompareTag("Ungrappable"))//TODO: When entering player trigger, Hook only releases when pulling object back
            {
                //Destroy bullet by release
                myAudio.Stop();
                myAudio.PlayOneShot(blocked);
                grapGun.CallRelease();
            }
        }
       

        if (grapGun.targetObj)
        {
            GrapGunEvents.hookAttachEvent.Invoke();
            ToggleCollider();
            ToggleSpriteScale(false);
            AnimateSticking();
        }
    }

    void ToggleCollider()
    {
        m_MyCollider.isTrigger = true;
    }
    void PlaySurfaceStickSound()
    {
        myAudio.Stop();
        myAudio.clip = stickToSurfaceAndSolidifyBeam;
        myAudio.loop = false;
        myAudio.PlayDelayed(0.01f);
    }

    public Vector2 GetStickingPosition()
    {
        return stickingPosition;
    }

    // Update is called once per frame
    void Update()
    {
        /*
         Ideas on moving closer to hook:
            - add force on player to 
            - use a tractor beam - shoot towards a beam, player & point direction to move towards; add force & velocity to move towards point. Decide on if you want gravity to work.
            - One-time force for one frame in direction of hook, like a jump, 
                - or persistent force, when you disable jump and move until you get there. 
            - Holding left mouse button draws to position until letting go, adding nuance. 
         */
        //if (!m_Renderer.isVisible)
        //{
        //    //Debug.Log("Object is visible");
        //    grapGun.CallRelease();
        //}

    }
}
