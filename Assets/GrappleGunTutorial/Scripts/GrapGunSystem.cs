using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum GrapGunMode  {
    Firing,
    Attached,
    PlayerToTarget,
    TargetToPlayer,
    HoldingObject,
    StuckOnTarget,
    Idle
}

public static class GrabberGunEvents
{
    public static UnityEvent shootHookEvent = new UnityEvent(),
                             hookAttachEvent = new UnityEvent(),
                             hookDetachEvent = new UnityEvent(),
                             hookPullTargetToPlayerEvent = new UnityEvent(),
                             hookPullPlayerToTargetEvent = new UnityEvent(),
                             grabberHoldObjectEvent = new UnityEvent(),
                             grabberThrowObjectEvent = new UnityEvent();
}

//public class GrapAttachEvent : UnityEvent<Vector2, GameObject, > { }
//Questions & Concerns:
/*
 Wouldn't an individual event class only work with certain functions called? 
 Would/Could I make duplicates or inherited versions for different amounts of parameters?
 

 */
[Serializable] public class GrapGunSystem : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject m_HookObjPrefab;

    [Header("Hook Functionality")]
    [SerializeField] GrapGunMode m_HookMode;
    public float m_HookSpeed;
    public float m_MaxHookLength;
    public bool m_IsHookActive = false;
    public bool m_CanShootHook = true;
    GameObject m_HookInst;
    GrapBulletScript m_GrapBulletScript;
    PlayerGrappleHandler m_PlayerGrappleHandler;

    [Header("Spring Functionality")]    
    public SpringJoint2D currentSpringJoint; //SpringJoint will be referencing the player's springjoint. 
    [SerializeField] SpringJoint2D playerSpring;
    [SerializeField] SpringJoint2D grapGunSpring;
    public LineRenderer debugLine;
    public float m_SpringFreqDefault = 0.125f;
    public float m_SpringFreqPull = 8f;
    public float m_SpringPullRate = 0.5f;

    LineRenderer m_LineRend;

    [Header("References")]
    public Transform shootPoint;
    public FeedbackHandler feedbackTextObj;


    Rigidbody2D targetRB;
    Vector2 aimDirection, colliderContactPoint;

    [Header("Audio")]
    [SerializeField] AudioClip shootHookBullet;
    [SerializeField] AudioClip holdPulledObject;
    [SerializeField] AudioClip shootHeldObject;
    [SerializeField] AudioClip releaseHook;

    AudioSource m_MyAudio;
    Animation m_MyAnimation;
    [SerializeField] ParticleSystem m_MyParticles;

    [Header("Debugging")]
    public GameObject targetObj;
    public TMP_Text targetText;
    public TMP_Text targetRBText;
    public TMP_Text springJointText;
    Text m_DebugText;

    private void Awake()
    {
        GrabberGunEvents.shootHookEvent.AddListener(Shoot);
        GrabberGunEvents.shootHookEvent.AddListener(AnimateGrapGunFire);
        GrabberGunEvents.grabberThrowObjectEvent.AddListener(NullTargetObj);
        GrabberGunEvents.grabberHoldObjectEvent.AddListener(/*CallPlayerStickToTarget*/CallHold);
        m_MyAnimation = GetComponent<Animation>();
        //GrapGunEvents.hookAttachEvent.AddListener(ToggleGrapGunSpring, bool);
    }

    void Start()
    {
        m_PlayerGrappleHandler = GetComponentInParent<PlayerGrappleHandler>();
        m_MyAudio = GetComponent<AudioSource>();
        if(feedbackTextObj)
            feedbackTextObj.ToggleStateText(false);
        m_LineRend = GetComponent<LineRenderer>();
        if(m_LineRend)
            m_LineRend.enabled = false;
        m_HookMode = GrapGunMode.Idle;
        //if(springJoint == null)
        //   springJoint = GetComponent<SpringJoint2D>();
        currentSpringJoint.frequency = m_SpringFreqDefault;
        SetCurrentJoint(grapGunSpring);
        playerSpring.enabled = false;
        grapGunSpring.enabled = false;
        //currentSpringJoint.enabled = false;
    }
    public GrapGunMode HookModeState
    {
        get { return m_HookMode; }
        set { m_HookMode = value; }
    }
    public GameObject GetHookInst()
    {
        if (m_HookInst)
            return m_HookInst;
        else
        {
            throw new Exception("Ooops! Hook Instance is currently null!");
        }
    }

    void ToggleGrapGunSpring(bool _b)
    {
        grapGunSpring.enabled = _b;
    }

    public void SetCurrentJoint(SpringJoint2D _s) //TODO: Change to getter & setter
    {
        if (currentSpringJoint != null)
            currentSpringJoint = null;

        if (_s == playerSpring)
        {
            playerSpring.enabled = true;
            grapGunSpring.enabled = false;
        }
        else if(_s == grapGunSpring)
        {
            grapGunSpring.enabled = true;
            playerSpring.enabled = false;
        }
        currentSpringJoint = _s;

        Debug.Log("Current Joint is now " + currentSpringJoint.name);
        currentSpringJoint.enabled = true;
        /* //TODO: Change func to getter & setter
           set: 

           get:
              return currentSpringJoint;
         */

    }


    void PullPlayerToTarget()
    {
        //Set the current joint to the player spring.
        //if (currentSpringJoint != playerSpring)
        //    currentSpringJoint = playerSpring;
        m_GrapBulletScript.PlaySound(m_GrapBulletScript.movingForward, true);
        SetCurrentJoint(playerSpring);

        //Enalbe the current spring and set the hook mode to PlayerToTarget
        m_HookMode = GrapGunMode.PlayerToTarget;
                
        //If the target object has a RB, then assign it to the spring joint's connected body
        if (targetRB)
            currentSpringJoint.connectedBody = targetRB;
        else //assign the hook to the targetObjRB AND the spring joint's connected body
        {
            targetRB = m_HookInst.GetComponent<Rigidbody2D>();
            currentSpringJoint.connectedBody = targetRB;
        }

        //set target to isKinematic 
        if (!targetRB.isKinematic)
            targetRB.isKinematic = true;

        //Set the spring joint's frequency to the pulling value, making it tighter and faster to pull
        //currentSpringJoint.frequency = springFreqPull;
        GrabberGunEvents.hookPullPlayerToTargetEvent.Invoke();
    }

    void PullTargetToPlayer()
    {         
        //Only perform this function if the targetObj has a PhysObj tag
        if (targetObj.CompareTag("PhysObj") || targetObj.layer == LayerMask.NameToLayer("Enemy"))
        {
            m_GrapBulletScript.PlayPullingBackSound();
            if (targetObj.layer == LayerMask.NameToLayer("Enemy"))
            {
                var enemyTarg = targetObj.GetComponent<EnemyController>();
                enemyTarg.OnGrabberPulled();
            }
            currentSpringJoint.enabled = true;
            //Set the current joint to the Grapple gun spring so the player isn't affected
            //if (currentSpringJoint != grapGunSpring)
            //    currentSpringJoint = grapGunSpring;
            SetCurrentJoint(grapGunSpring);

            //Enable the spring joint and set the hook mode to TargetToPlayer
            m_HookMode = GrapGunMode.TargetToPlayer;

            //If the target object has a RB, then assign target to the spring joint's connected body
            if (targetRB)
                currentSpringJoint.connectedBody = targetRB;

            //set target to isKinematic 
            if (targetRB.isKinematic)
                targetRB.isKinematic = false;
            //Set the spring joint's frequency to the pulling value, making it tighter and faster to pull
            currentSpringJoint.frequency = m_SpringFreqPull;
        }
    }

    public void PlayHoldSound()
    {
        m_MyAudio.Stop();
        m_MyAudio.loop = false;
        //myAudio.clip = holdPulledObject;
        m_MyAudio.PlayOneShot(holdPulledObject);
    }

    public void PlayReleaseHookSound()
    {
        //myAudio.Stop();
        //myAudio.clip = releaseHook;
        //myAudio.loop = false;
        m_MyAudio.PlayOneShot(releaseHook);
    }

    void AnimateGrapGunFire()
    {
        if(m_MyAnimation != null) 
            m_MyAnimation.Play();
        if (m_MyParticles != null)
        {
            if (m_MyParticles.isPlaying)
                m_MyParticles.Stop();

            m_MyParticles.Play();
            //Invoke("DisableSparkParticles", .3f);
        }
    }

    private void DisableSparkParticles()
    {
        if (m_MyParticles != null)
            m_MyParticles.gameObject.SetActive(false);
    }
    public void PlayThrowObjectSound()
    {
        m_MyAudio.Stop();
        m_MyAudio.clip = shootHeldObject;
        m_MyAudio.loop = false;
        m_MyAudio.Play();
        m_MyAudio.PlayOneShot(shootHeldObject);
    }

    void Update()
    {
        Vector2 MousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        aimDirection = MousePos - (Vector2)transform.position;

        FaceMouse();

        //MOUSE0 (LEFT)
        //is pressed, Fire the Hook or pull object to the player
        if (Input.GetMouseButtonDown(0))
        {
            //Shoot hook normally
            if (!targetObj && !m_IsHookActive
                && HookModeState == GrapGunMode.Idle)
            {
                    GrabberGunEvents.shootHookEvent.Invoke();
            }
            /*Shoot hook while attached to other object
            //else if (!targetObj && !m_IsHookActive
            //    && HookModeState == GrapGunMode.StuckOnTarget)
            //    {
            //        //m_PlayerGrappleHandler.StickToTargetToggle(false);
            //        Shoot();
            //    }*/
            //}
            //Pull target hook is attached to
            else if (targetObj &&
                    (targetObj.CompareTag("PhysObj") || targetObj.layer == LayerMask.NameToLayer("Enemy"))
                    && (HookModeState == GrapGunMode.Attached))
            {

                if (HookModeState == GrapGunMode.StuckOnTarget)
                {
                    m_PlayerGrappleHandler.StickToTargetToggle(false);
                }
                //if (targetObj.layer == LayerMask.NameToLayer("Enemy"))
                //    targetObj.GetComponent<EnemyController>().OnGrapplePulled();

                Debug.Log("Pulling to player!");
                PullTargetToPlayer();
            }
        }

        //MOUSE1 (RIGHT) 
        //is clicked with a target object, pull the player to it
        else if (Input.GetMouseButtonDown(1) && targetObj)
        {
            if (m_PlayerGrappleHandler.isSticking/*HookModeState == HookMode.StuckOnTarget*/)
            {
                m_PlayerGrappleHandler.StickToTargetToggle(false);
            }
            //if(targetObj.CompareTag("PhysObj"))
            PullPlayerToTarget();
        }

        //MOUSE3 (MIDDLE) or Space
        //Release target and detach hook
        else if (Input.GetMouseButtonDown(2) || Input.GetAxis("Jump") > 0f)
        {
            if (targetObj)
            {
                GrabberGunEvents.hookDetachEvent.Invoke();
                PlayReleaseHookSound();
                ReleaseTarget(GrapGunMode.Idle);
            }
            else if (m_HookInst && !targetObj)
            {
                PlayReleaseHookSound();
                Destroy(m_HookInst);
                m_HookInst = null;
                m_IsHookActive = false;
                m_LineRend.enabled = false;
                HookModeState = GrapGunMode.Idle;
            }
        }
        
        // Set SpringJoint's Connected Anchor to TargetObj 
        //based on tag or layer
        if (targetObj != null)
        {
            if (targetObj.CompareTag("PhysObj")&& m_HookInst != null)
                currentSpringJoint.connectedAnchor = m_HookInst.transform.localPosition; //localPosition works when target object is physics. 
            else if (targetObj.CompareTag("Environment"))
                currentSpringJoint.connectedAnchor = targetObj.transform.InverseTransformPoint(colliderContactPoint);
                        /*m_HookInst.transform.position*/
                        /*colliderContactPoint*/
                        /*m_HookInst.transform.localPosition;*/ 
                        //localPosition works when target object is physics. 

            //lineRend.SetPosition(0, shootPoint.position);
            //lineRend.SetPosition(1, m_HookInst.transform.position);
        }
        //else if(m_HookInst != null && m_HookInst.)

        if (m_HookInst != null)
        {
            if (Vector2.Distance(transform.position, m_HookInst.transform.position) >= m_MaxHookLength)
                m_HookInst = null;
            m_LineRend.SetPosition(0, shootPoint.position);
            m_LineRend.SetPosition(1, m_HookInst.transform.position/*currentSpringJoint.connectedAnchor*/);
        }


        //if(/*m_MyParticles.time >= */m_MyParticles.isStopped)

        //if (currentSpringJoint != null && currentSpringJoint.isActiveAndEnabled)
        //{
        //    Debug.DrawLine(currentSpringJoint.anchor, currentSpringJoint.connectedAnchor, Color.yellow);
        //    debugLine.SetPosition(0, transform.position);
        //    debugLine.SetPosition(1, currentSpringJoint.connectedAnchor);
        //}

        //DEBUGGING TEXT
        if (targetRBText)
        {
            if (targetObj)
                targetText.text = "Target Object: " + targetObj;

            if (targetRB)
                targetRBText.text = "Target RB: " + targetRB;
            else
                targetRBText.text = "Target RB: N/A";

            if (currentSpringJoint && m_HookInst)
                springJointText.text = "Current Joint: " + currentSpringJoint +
                "\nHook Position: " + m_HookInst.transform.position + "\nConnected Anchor: " + currentSpringJoint.connectedAnchor;
        }
    }   

    private void Shoot()
    {
        // -- FIRE THE HOOK BULLET AND ASSIGN INSTANCES TO THE HOOK AND GrapBulletScript
        GameObject hookInst = GameObject.Instantiate(m_HookObjPrefab, shootPoint.position, transform.rotation/*Quaternion.identity*/);
        hookInst.GetComponent<Rigidbody2D>().AddForce(transform.right * m_HookSpeed, ForceMode2D.Force);
        //hookInst.GetComponent<GrapBulletScript>().
        m_HookInst = hookInst;
        m_GrapBulletScript = hookInst.GetComponent<GrapBulletScript>();


        // -- ASSIGN GUN SOURCE TO THIS AND GRAP BULLET TO PlayerGrappleHandler
        m_GrapBulletScript.AssignGunSource(this);
        AssignGrapBulletToPlayer(true);

        // -- ENABLE HOOK, LINERENDERER AND SET HookMode to Firing
        m_IsHookActive = true;
        m_LineRend.enabled = true;
        m_HookMode = GrapGunMode.Firing;

        // -- PLAY HOOK BULLET ANIMATIONS AND SOUND
        m_MyAudio.PlayOneShot(shootHookBullet);
        if(m_MyAnimation)
            m_MyAnimation.Play();

        // -- UPDATE FEEDBACK TEXT
        if (feedbackTextObj != null)
        {
            feedbackTextObj.ToggleStateText(true);
            feedbackTextObj.StateTextSignal();
            feedbackTextObj.Invoke("StateTextUpdate", 0.5f);
        }
    }
    
    // NullTargetObj - Sets Target Object and its rigidbody attached to the hook to null.
    void NullTargetObj()
    {
        if (targetObj)
            targetObj = null;
        if (targetRB)
            targetRB = null;
    }
    public void AssignGrapBulletToPlayer(bool _assign)
    {
        if (_assign)
            m_PlayerGrappleHandler.m_GrapBulletInstance = m_GrapBulletScript;
        else
            m_PlayerGrappleHandler.m_GrapBulletInstance = null;
    }
    public void SetTargetObj(GameObject hit)
    {
        targetObj = hit;
        currentSpringJoint.enabled = true;
        if (targetObj.CompareTag("Environment"))
        {
            currentSpringJoint.connectedBody = m_HookInst.GetComponent<Rigidbody2D>();
        }
        else if ( targetObj.GetComponent<Rigidbody2D>())
        {
            targetRB = targetObj.GetComponent<Rigidbody2D>();
            currentSpringJoint.connectedBody = targetRB;
        }
        SetCurrentJoint(grapGunSpring);

        //else
        //{
        //    targetObjRB = 
        //}
        //else
        m_LineRend.enabled = true;
        currentSpringJoint.frequency = m_SpringFreqDefault;
        m_HookMode = GrapGunMode.Attached;
        //currentSpringJoint.
        //m_HookInst = null;
    }

    public void SetColliderConnectPoint(Vector2 point)
    {
        colliderContactPoint = point;
    }

    public void CallRelease()
    {
        ReleaseTarget(GrapGunMode.Idle);
    }
    public void CallHold()
    {
        ReleaseTarget(GrapGunMode.HoldingObject);
    }

    public void CallPlayerStickToTarget()
    {
        ReleaseTarget(GrapGunMode.Idle/*HookMode.StuckOnTarget*/);
    }

    //Release Target that accepts a HookMode checking for Idle or holding object
    void ReleaseTarget(GrapGunMode _mode)
    {
        if (_mode == GrapGunMode.HoldingObject || 
            _mode == GrapGunMode.Idle)
        {
            HookModeState = _mode;
            ResetSpringJoints(); //experiment later with keeping spring joints while holding object
            if(_mode == GrapGunMode.Idle) 
                m_PlayerGrappleHandler.TurnUpright();
            m_IsHookActive = false;
            m_LineRend.enabled = false;
            targetRB = null;
            targetObj = null;
            Destroy(m_HookInst);
            m_GrapBulletScript = null;
            m_HookInst = null;

            m_CanShootHook = (_mode == GrapGunMode.HoldingObject);
            //if (_mode == HookMode.HoldingObject)
            //    canFire = false;
            //else
            //    canFire = true;
        }

        else
            return;        
    }

    void ReleaseTarget()
    {
        targetRB = null;
        targetObj = null;
        Destroy(m_HookInst, 0.15f);
        AssignGrapBulletToPlayer(false);
        m_GrapBulletScript = null;
        m_HookInst = null;
        m_IsHookActive = false;
        ResetSpringJoints();

        HookModeState = GrapGunMode.Idle;
        m_LineRend.enabled = false;
        m_IsHookActive = false;
    }

    void ResetSpringJoints()
    {
        //Reset spring frequencies
        currentSpringJoint.frequency = m_SpringFreqDefault;
        playerSpring.frequency = m_SpringFreqDefault;
        grapGunSpring.frequency = m_SpringFreqDefault;
        
        //Set connected bodies to null
        playerSpring.connectedBody = null;
        grapGunSpring.connectedBody = null;
        currentSpringJoint.connectedBody = null;

        //Reset anchor positions
        colliderContactPoint.Set(0, 0);/* = new Vector2(0, 0)*/;
        currentSpringJoint.connectedAnchor.Set(0, 0);
        currentSpringJoint.connectedAnchor = new Vector2(0, 0);
        playerSpring.connectedAnchor.Set(0, 0);
        grapGunSpring.connectedAnchor.Set(0, 0);
        grapGunSpring.connectedAnchor = new Vector2(0, 0);

        //Toggle spring joints
        currentSpringJoint.enabled = false;
        playerSpring.enabled = false;
        grapGunSpring.enabled = false;
    }
    private void FaceMouse()
    {
        transform.right = aimDirection;
    }
}
