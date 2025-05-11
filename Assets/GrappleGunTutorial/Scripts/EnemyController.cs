//using Boo.Lang.Environments;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public GameObject tempAudioPrefab;
    public int hp = 2,
               damageToPlayer = 1;
    bool invincible = false;
    public bool hurtsPlayer = true, 
                hurtsEnemy = false;
    public float defaultInvincibleTimer = 0.5f;
    float invincibleTime;
    public GameObject carriedObject;
    Rigidbody2D myRB;
    AudioSource myAudio;
    [SerializeField] EnemyState myEnemyState;
    [SerializeField] ParticleSystem m_DamageParticles;
    public GameObject m_NormalHeadObj, m_DeadHeadObj;
    [Header("Audio")]
    [SerializeField] AudioClip a_TakeDamage;
    [SerializeField] AudioClip a_death;
    [SerializeField] AudioClip a_thrownCollide;
    [SerializeField] AudioClip a_unStunned;
    private bool m_IsTurningUpright;

    private void Awake()
    {
        myAudio = GetComponent<AudioSource>();
        myEnemyState = EnemyState.Normal;
        invincibleTime = defaultInvincibleTimer;
        myRB = GetComponent<Rigidbody2D>();
        EnemyBase e = GetComponent<EnemyBase>();
        if (e != null)
        {
            e.onPull += OnGrabberPulled;
            e.onHold += OnHeldByPlayer;
            e.onDrop += OnThrownByPlayer;
            //e.onDrop += OnRecovered;
        }
        if (m_NormalHeadObj && m_DeadHeadObj)
        {
            m_NormalHeadObj.SetActive(true);
            m_DeadHeadObj.SetActive(false);
        }
    }
    
    public EnemyState GetState()
    {        return myEnemyState;    }
    void ToggleInvincible()
    {
        if (!invincible)
            invincible = true;
        else
            invincible = false;
    }

    public void ToggleHurtsPlayer(bool _b)
    {        hurtsPlayer = _b;    }

    void ToggleConstraints(bool _b)
    {
        if (_b)
        {
            myRB.constraints = RigidbodyConstraints2D.FreezePositionY;
            myRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
            myRB.constraints = RigidbodyConstraints2D.None;
    }

    void ChangeColliderWhenThrown()
    {

    }

    void ResetCollider()
    {

    }

    public void SetTagToPhysObj()
    { gameObject.tag = "PhysObject"; }

    public void SetTagToEnemy()
    { gameObject.tag = "Enemy"; }

    public void SetTagToGrabbable()
    { gameObject.tag = "Grabbable"; }

    public void OnGrabberPulled()
    {
        myEnemyState = EnemyState.Pulled;
        ChangeLayer();
        SetTagToPhysObj();
        ToggleHurtsPlayer(false);
        hurtsEnemy = true;
        ToggleConstraints(false);
        if(GetComponent<AutoMoveSimpleGrabbable>())
        {
            var autoMove = GetComponent<AutoMoveSimpleGrabbable>();
            autoMove.enabled = false;
        }
        GrabberGunEvents.grabberHoldObjectEvent.AddListener(OnHeldByPlayer);
    }

    public void OnHeldByPlayer()
    {
        myEnemyState = EnemyState.HeldByPlayer;
        hurtsEnemy = false;
        GrabberGunEvents.grabberThrowObjectEvent.AddListener(OnThrownByPlayer);
        GrabberGunEvents.grabberHoldObjectEvent.RemoveListener(OnHeldByPlayer);

        //if (GetComponent<AutoMoveSimple>().isMoving)
        //GetComponent<AutoMoveSimple>().ChangeMove(false);
    }

    public void OnThrownByPlayer()
    {
        SetTagToPhysObj();
        myEnemyState = EnemyState.Dropped; //Since Throw is called and calls Drop anyway, keep state at Dropped.
        hurtsEnemy = true;
        GrabberGunEvents.grabberThrowObjectEvent.RemoveListener(OnThrownByPlayer);
    }

    public void OnRecovered()
    {
        //SetTagToEnemy();
        SetTagToGrabbable();
        ToggleHurtsPlayer(true);
        hurtsEnemy = false;
        ToggleConstraints(true);
        myEnemyState = EnemyState.Normal;
        if (GetComponent<AutoMoveSimple>())
        {
            var autoMove = GetComponent<AutoMoveSimple>();
            autoMove.enabled = true;

            if (autoMove.isMoving)
                autoMove.MoveToggle(false);
        }
        m_IsTurningUpright = false;
    }

    void ChangeLayer()
    {
        //Debug.Break();
        if(gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            gameObject.layer = LayerMask.NameToLayer("PhysObj");
            return;
        }
        else if (gameObject.layer == LayerMask.NameToLayer("PhysObj"))
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
            return;
        }
    }

    void StartRecover()
    {
        //Check if grounded
        m_IsTurningUpright = true;
        //Return to upright position
        //if shooting, reactivate gun
        //reactivate damagesPlayer
        //deactivate damagesEnemy
    }

    void AddTempAudio(AudioClip _audio)
    {
        if(myAudio.isPlaying)
        {
            var _tempAudio = GameObject.Instantiate(tempAudioPrefab);
            _tempAudio.GetComponent<AudioPlayAndDestroy>().InitializeAudioSource(_audio);
        }
    }

    public void DamageEnemy(int _dmg = 1)
    {
        hp -= _dmg;
        //m_DamageParticles.Stop();
        m_DamageParticles.Play();
        if (hp <= 0)
        {
            myAudio.Stop();
            myAudio.PlayOneShot(a_death);
            Debug.Log("I died!");
            //GameObject.Destroy(this.gameObject, 0.5f);
            KillEnemy();
        }
        else
        {
            myAudio.PlayOneShot(a_TakeDamage);
            ToggleInvincible();
            Debug.Log("OW! This enemy got hit for " + _dmg
                      + "! I'm now at " + hp + "HP");
        }
    }
    void KillEnemy() 
    {
        //Add a force, increase mass and set collider to Trigger 
        GetComponent<Rigidbody2D>().AddForce(new Vector2(( transform.right.x * -100f), 150f), ForceMode2D.Impulse);
        myRB.mass = 15;
        GetComponent<Collider2D>().enabled = false;

        if (m_NormalHeadObj && m_DeadHeadObj)
        { 
            m_NormalHeadObj.SetActive(false); 
            m_DeadHeadObj.SetActive(true);
        }
        //Disable AutoMove
        if (GetComponent<AutoMoveSimple>() != null)
        {
            GetComponent<AutoMoveSimple>().MoveToggle(false);
        }

        Invoke("SetInactiveInvoking", .4f);

    }

    void SetInactiveInvoking()
    {
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Damage and toggle invincible when hit by object with PhysicsDamageHandler
        if (collision.gameObject.GetComponent<PhysicsDamageHandler>())
        {
            var physDmg = collision.gameObject.GetComponent<PhysicsDamageHandler>();
            if (physDmg.doesDamage && !invincible)
            {
                int dmg = physDmg.damageValue;

                if (collision.gameObject.GetComponent<EnemyController>())                    
                {
                    if (collision.gameObject.GetComponent<EnemyController>().hurtsEnemy)
                    { 
                        DamageEnemy(dmg);
                        collision.gameObject.GetComponent<EnemyController>().DamageEnemy(dmg);
                    }
                    else
                        return;
                }               
            
                DamageEnemy(dmg);
            }
        }
        // Damage Player on collision if my state is Normal or Grappled
        else if (collision.gameObject.CompareTag("Player"))
        {
            if ((myEnemyState == EnemyState.Normal /*|| myEnemyState == EnemyState.Grappled*/) &&
                hurtsPlayer /*&& collision.gameObject.GetComponent<PlayerHealth>().GetInvincibleState() == false*/)
            { MyEventsManager.OnPlayerDamaged(); }

            //else if (myEnemyState == EnemyState.Pulled)
            //{
            //    //Attach to grapple gun

            //}
        }        
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if ((collision.gameObject.CompareTag("Environment") || collision.gameObject.GetComponent<GrabbableTerrain>()) 
            && (myEnemyState == EnemyState.Damaged || myEnemyState == EnemyState.Dropped || myEnemyState == EnemyState.Thrown)
            && myRB.linearVelocity.magnitude <= 1f 
            && (myRB.linearVelocityY <= 0.001 && myRB.linearVelocityY >= -0.001) )
        {
            /*myRB.linearVelocity = Vector2.zero;
            //what if object hits a wall/ceiling instead of ground?
            Debug.Log("Invoking Recover");
            Invoke("OnRecovered", 0.7f);*/
        }
    }

    public void AddListeners()
    {
        //GrapGunEvents.hookAttachEvent.AddListener(ChangeLayer);
        GrabberGunEvents.hookPullTargetToPlayerEvent.AddListener(OnGrabberPulled);
        GrabberGunEvents.grabberHoldObjectEvent.AddListener(OnThrownByPlayer);
    }
    public void RemoveListeners()
    {
        //GrapGunEvents.hookAttachEvent.RemoveListener(ChangeLayer);
        GrabberGunEvents.hookPullTargetToPlayerEvent.RemoveListener(OnGrabberPulled);
        GrabberGunEvents.grabberHoldObjectEvent.RemoveListener(OnThrownByPlayer);
    }

    public EnemyState GetMyState()
    {
        return myEnemyState;
    }
    public void SetMyState(EnemyState _e)
    {
        myEnemyState = _e;
    }

    // Update is called once per frame
    void Update()
    {     
        if (invincible)
        {
            invincibleTime -= Time.deltaTime;
            //FLASH SPRITE TO SHOW INVINCIBILITY
            if (invincibleTime <= 0)
            {
                ToggleInvincible();

                invincibleTime = defaultInvincibleTimer;
            }
        }
        /*      if(myEnemyState == EnemyState.Damaged || myEnemyState == EnemyState.Thrown)
                {
                    if(myRB.velocity.y == 0)
                    {
                        transform.Rotate(Vector3.up);
                        myRB.isKinematic = true;
                        myRB.velocity.Set(0,0);
                        myEnemyState = EnemyState.Normal;
                    }
                }
                if(m_IsTurningUpright && transform.rotation != Quaternion.Euler(transform.up) 
                    && myEnemyState == EnemyState.Thrown)
                {
                    transform.rotation = 
                        Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.up), .05f);
                    if(transform.rotation != Quaternion.Euler(transform.up))
                    { OnRecovered(); }
                }
        */
    }

    private void FixedUpdate()
    {
        if (myEnemyState == EnemyState.Damaged || myEnemyState == EnemyState.Dropped)
        {
            if (myRB.linearVelocity.y == 0 && !m_IsTurningUpright)
            {
                Debug.Log("Recovering");

                //transform.Rotate(Vector3.up);
                //myRB.velocity.Set(0, 0);
                //myRB.isKinematic = true; //deprecated in Unity 6
                myRB.bodyType = RigidbodyType2D.Kinematic;
                myRB.Sleep();
                StartRecover();
            }
        }
        if (m_IsTurningUpright && transform.rotation != Quaternion.Euler(transform.up)
            && (myEnemyState == EnemyState.Damaged || myEnemyState == EnemyState.Dropped || myEnemyState == EnemyState.Thrown))
        {
            transform.rotation = Quaternion.Euler(transform.up);
            myRB.WakeUp();
            //Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.up), .5f);
            if (transform.rotation == Quaternion.Euler(transform.up))
            { OnRecovered(); }
        }
    }
}
