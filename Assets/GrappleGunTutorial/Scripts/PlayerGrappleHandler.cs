//using Boo.Lang.Environments;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityStandardAssets._2D;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpringJoint2D))]



public class PlayerGrappleHandler : MonoBehaviour
{
    [Header("Grapple Gun & Spring Settings")]
    public SpringJoint2D mySpring;
    public SpringJoint2D grapGunSpring;
    public GrapGunSystem grapGun;
    public GameObject grapShootPoint;
    public GrapBulletScript m_GrapBulletInstance;

    Transform m_HookTransform;
    Vector2 m_HookPos;
    Quaternion m_HookRot;

    [Header("Target Object Info")]
    [SerializeField] GameObject targetObj;    
    Rigidbody2D myRB, targetRB;
    GameObject heldPhysObject;
    Rigidbody2D heldPhysicsRB;
    PlatformerCharacter2D_Alt myPlayerController;
    

    float gravScaleDefault;
    float dragDefault;
    bool isHolding = false;
    public bool isSticking = false,
                pullingToTarget = false, 
                isTurningUpright = false;

    public float pullRate = 0.1f;
    public float tugRate = 0.01f;
    
    [Header("Swinging")]
    public bool m_SwingEnabled = false;
    public bool m_IsSwinging = false;
    public float m_SwingYOffset = 1f;

    [Header("Debug")]
    public FeedbackHandler feedbackTextObj;
    [SerializeField] GrapGunMode grappleGunHookMode;


    // Start is called before the first frame update
    void Awake()
    {
        // -- HOOK ATTACH/DETACH EVENT LISTENERS
        GrabberGunEvents.hookAttachEvent.AddListener(SetTargetObj);
        GrabberGunEvents.hookDetachEvent.AddListener(NullTargetObj);

        if (m_SwingEnabled)
        { 
            GrabberGunEvents.hookAttachEvent.AddListener(StartSwinging);
            GrabberGunEvents.hookAttachEvent.AddListener(SetHookTransform);
        }
        // -- PULL PLAYER EVENT LISTENERS
        GrabberGunEvents.hookPullPlayerToTargetEvent.AddListener(SetHookTransform);
        GrabberGunEvents.hookPullPlayerToTargetEvent.AddListener(SetHookModeToPlayerToTarget);
        // -- THROWING OBJECT EVENT LISTNERS
        GrabberGunEvents.grabberThrowObjectEvent.AddListener(NullTargetObj);

        // -- INITIALIZE VALUES FOR PLAYER CONTROLLER AND RIGIDBODY2D
        myPlayerController = GetComponent<PlatformerCharacter2D_Alt>();
        myRB = GetComponent<Rigidbody2D>();
        
        if (mySpring == null)
            mySpring = GetComponent<SpringJoint2D>();
        if (grapGunSpring == null)
            throw new System.Exception("Error from PlayerGrappleHandler! Grapple gun spring isn't assigned!");
        if (grapGun == null)
            throw new System.Exception("Error from PlayerGrappleHandler! Grapple gun not here!");
        
        gravScaleDefault = myRB.gravityScale;
        dragDefault = myRB.linearDamping;
    }
    
    void SetTargetObj()
    {
        if(grapGun.targetObj)
            targetObj = grapGun.targetObj;
    }

    void SetTargetToHook()
    {
        if (grapGun.targetObj)
            targetObj = m_GrapBulletInstance.gameObject;
    }

    void SetTargetRB()
    {                   
        if (targetObj && targetObj.GetComponent<Rigidbody2D>())
            targetRB = targetObj.GetComponent<Rigidbody2D>();
        if (m_SwingEnabled)
            mySpring.connectedBody = targetRB;
    }

    void NullTargetObj()
    {
        if (grapGun.targetObj)
            targetObj = null;
        if (targetRB)
            targetRB = null;
    }
    
    void StartSwinging()
    {
       if (m_HookPos.y > transform.position.y + m_SwingYOffset)
       {
            mySpring.enabled = true;
            SetTargetToHook();
            SetTargetRB();
            m_IsSwinging = true;
       }
    }

    void ResetSwinging()
    {
        mySpring.frequency = .125f;
        mySpring.enabled = false;
        
        ResetTargetsHere();
        //SetTargetToHook();
        //SetTargetRB();
        m_IsSwinging = false;

    }

    void PullMeToTarget()
    {
        if (targetRB)
        { 
            mySpring.connectedBody = targetRB;
            //targetRB.isKinematic = true;
        }
        SetHookTransform();
        pullingToTarget = true;
    }

    void SetHookModeToPlayerToTarget()
    {
        grappleGunHookMode = GrapGunMode.PlayerToTarget;
    }


    void ResetTargetsHere()
    {
        targetObj = null;
        if (targetRB)
        {
            mySpring.connectedBody = null;
            targetRB = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        /*
         *if player is pulled to an object, gravityscale is 0 and isKinematic
         *if they put any move input, gravityscale is normal and isKinematic = false
         *if they have their hook attached to another object and pull either themselves or the object, 
          gravityscale is normal and isKinematic = false
        */
        //var isCollidngWithBullet = (col.gameObject == grapBullet);
        //Debug.Log("GrapBullet has entered the trigger? " + isCollidngWithBullet + "\nObject: " + col.gameObject.name + "\nBullet: " + grapBullet);

        //if collision is attached object 

        
        if (m_GrapBulletInstance && targetObj
            && (col.gameObject == m_GrapBulletInstance.gameObject  || col.gameObject == grapGun.targetObj))
        {
            if (targetObj.GetComponent<Rigidbody2D>())
            {
                SetTargetRB();

                //If target is a physics object, it's not kinematic and hookmode is TargetToPlayer...
                if (grapGun.HookModeState == GrapGunMode.TargetToPlayer &&
                    targetObj.CompareTag("PhysObj") &&
                    !targetRB.isKinematic)
                {
                    var physHolder = grapGun.GetComponent<PhysObjectHolder>();
                    //GrapGunEvents.hookHoldObjectEvent.Invoke();
                    physHolder.HoldTargetObj(targetObj);
                    grapGun.CallHold();

                    /*if(targetObj.GetComponent<EnemyController>())
                    {
                    }*/
                    GrabberGunEvents.grabberHoldObjectEvent.Invoke();

                    //if attached obj is kinematic
                    //set to kinematic = false
                    //add reverse force equal to the higher speed that either you or the attached object is at
                    //set attachted obj's dealsDamage to true

                    //target.GetComponent<Rigidbody2D>().isKinematic = true;
                }

                //Else if hookmode is not PlayerToTarget and target is NOT an enemy, 
                else if (grapGun.HookModeState == GrapGunMode.PlayerToTarget
                        && !targetObj.CompareTag("Enemy"))
                {
                    //mySpring.frequency = (mySpring.frequency < grapGun.springFreqPull) ? grapGun.springFreqPull : mySpring.frequency;

                    if (targetRB.isKinematic/*targetObj.GetComponent<TilemapCollider2D>()*/)
                        StickToTargetToggle(true);
                }
            }
            else
            {
                if (grapGun.HookModeState == GrapGunMode.PlayerToTarget
                        && !targetObj.CompareTag("Enemy"))
                {
                    //mySpring.frequency = (mySpring.frequency < grapGun.springFreqPull) ? grapGun.springFreqPull : mySpring.frequency;

                    //if (targetRB.isKinematic/*targetObj.GetComponent<TilemapCollider2D>()*/)
                    pullingToTarget = false;
                    mySpring.frequency = grapGun.m_SpringFreqDefault;
                    StickToTargetToggle(true);
                }
            }
        }
    }

    public void StickToTargetToggle(bool _b)
    {
        isSticking = _b;

        if (isSticking)
        {
            m_GrapBulletInstance.StopUnloopSound();
            grapGun.PlayHoldSound();
            //grapGun.HookModeState = HookMode.StuckOnTarget;

            myRB.isKinematic = true;
            myRB.linearDamping = 10;
            myRB.linearVelocity = new Vector2(0, 0);
            myRB.gravityScale = 0;

            grapGun.CallPlayerStickToTarget();
        }
        else
        {
            //grapGun.HookModeState = HookMode.Attached;
            myRB.isKinematic = false;
            //grapGun.CallRelease();
            myRB.linearDamping = dragDefault;
            myRB.gravityScale = gravScaleDefault;
        }
    }

    void SetHookTransform()
    {
        m_HookTransform = m_GrapBulletInstance.transform;
        //Debug.Break();
        m_HookPos = m_HookTransform.position/*GetStickingPosition()*/;
        m_HookRot = m_HookTransform.rotation;
    }

    void LerpToTarget()
    {

        //Lerp player to target
        transform.position = Vector2.Lerp(transform.position, /*m_HookBulletTransform.position*/(Vector3)m_HookPos, pullRate);
    }

    void ShortLerpToTarget()
    {

        //Lerp player to target
        transform.position = Vector2.Lerp(transform.position, /*m_HookBulletTransform.position*/(Vector3)m_HookPos, pullRate);
    }

    private void FixedUpdate()
    {
        if (grappleGunHookMode == GrapGunMode.PlayerToTarget)
            LerpToTarget();

        //Lerp Spring Frequency from minimum to springFreqPull
        if (pullingToTarget)
        {
            if (mySpring.frequency < grapGun.m_SpringFreqPull)
            {
                mySpring.frequency = Mathf.Lerp(mySpring.frequency, grapGun.m_SpringFreqPull, pullRate);
                ////Get angle between player and hook
                //Vector2 dir = grapGun.GetHookInst().transform.position - transform.position;
                //myRB.AddForce(dir * 2f);
            }
            //else
            //{
            //    transform.position = Vector2.Lerp(transform.position, m_GrapHookBullet.GetStickingPosition(), .2f);
            //    transform.rotation = Quaternion.Lerp(transform.rotation, m_GrapHookBullet.transform.rotation, .2f);

            //}
        }
        else if (isSticking)
        {
            //if(targetRB & targetRB.isKinematic)
            //{

            //}

            if (transform.position != (Vector3)m_HookPos)
            {
                Debug.Log("Moving to hook bullet point");
                transform.position = Vector2.Lerp(transform.position, /*m_HookBulletTransform.position*/(Vector3)m_HookPos, .1f);

                transform.rotation = Quaternion.Lerp(transform.rotation, /*m_HookBulletTransform.rotation*/m_HookRot, .1f);
            }
        }
        else if (grappleGunHookMode == GrapGunMode.Idle && transform.rotation != Quaternion.Euler(transform.up))
            TurnUpright();
    }

    private void Update()
    {
        grappleGunHookMode = grapGun.HookModeState;
        
        if(m_IsSwinging)
        {
            if (myPlayerController.GetVerticalInput() > 0f || myPlayerController.GetVerticalInput() < 0f)
            {
                //mySpring.frequency += myPlayerController.GetVerticalInput()/2;
                mySpring.distance += myPlayerController.GetVerticalInput();
            }
        }
        
        else if ((myPlayerController.GetHorizontalInput() > 0f || myPlayerController.GetHorizontalInput() < 0f)
            && isSticking)
        {
            StickToTargetToggle(false);
        }

        //if(Input.GetAxis(""))
    }


    public void TurnUpright()
    {
        transform.rotation = 
            Quaternion.Lerp(transform.rotation, /*m_HookBulletTransform.rotation*/Quaternion.Euler(Vector3.up), .05f);
    }
    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    //if collision is atttached object 
 
    //    //{
    //    //    var target = grapGun.targetObj;
    //    //    var targetRB = target.GetComponent<Rigidbody2D>();

    //    //    if (target.CompareTag("PhysObj") && 
    //    //        !targetRB.isKinematic && 
    //    //        grapGun.HookModeState == HookMode.TargetToPlayer)
    //    //    {
    //    //        var physHolder = grapGun.GetComponent<PhysObjectHolder>();
    //    //        grapGun.CallHold();
    //    //        physHolder.HoldTargetObj(target);
    //    //        //if attached obj is kinematic
    //    //            //set to kinematic = false
    //    //            //add reverse force equal to the higher speed that either you or the attached object is at
    //    //            //set attachted obj's dealsDamage to true

    //    //        //targetRB.AddForceAtPosition(-targetRB.velocity * targetRB.mass * 10, collision.contacts[0].point, ForceMode2D.Impulse);
    //    //        //myRB.drag = 100;
    //    //        //if (target.GetComponent<PhysicsDamageHandler>())
    //    //        //    target.GetComponent<PhysicsDamageHandler>().doesDamage = true;
    //    //        //else
    //    //        //    throw new System.Exception("Error! " + target.gameObject.name + "doesn't have PhysicsDamageHandler!");

    //    //        //grappleGun.CallRelease();
    //    //        //GetComponent<Rigidbody2D>().drag = 0;

    //    //        //target.GetComponent<Rigidbody2D>().isKinematic = true;
    //    //    }

    //    //    else if (targetRB.isKinematic && grapGun.HookModeState == HookMode.TargetToPlayer)
    //    //    {
    //    //        //mySpring.frequency = grappleGun.springFreqDefault;
    //    //        //var physHolder = grappleGun.GetComponent<PhysObjectHolder>();
    //    //        //grappleGun.CallHold();
    //    //        //physHolder.HoldTargetObj(target);
    //    //        //if attached obj is kinematic
    //    //        //set to kinematic = false
    //    //        //add reverse force equal to the higher speed that either you or the attached object is at
    //    //        //set attachted obj's dealsDamage to true

    //    //        //targetRB.AddForceAtPosition(-targetRB.velocity * targetRB.mass * 10, collision.contacts[0].point, ForceMode2D.Impulse);
    //    //        //myRB.drag = 100;
    //    //        //if (target.GetComponent<PhysicsDamageHandler>())
    //    //        //    target.GetComponent<PhysicsDamageHandler>().doesDamage = true;
    //    //        //else
    //    //        //    throw new System.Exception("Error! " + target.gameObject.name + "doesn't have PhysicsDamageHandler!");

    //    //        //grappleGun.CallRelease();
    //    //        //GetComponent<Rigidbody2D>().drag = 0;

    //    //        //target.GetComponent<Rigidbody2D>().isKinematic = true;
    //    //    }
    //    //}          
    //}

    void HoldTargetObj(GameObject _targetObj)
    {
        heldPhysObject = _targetObj;
        heldPhysicsRB = heldPhysObject.GetComponent<Rigidbody2D>();
        heldPhysicsRB.isKinematic = true;
        heldPhysObject.GetComponent<Collider2D>().isTrigger = true;
        heldPhysObject.transform.position = grapGunSpring.anchor;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == grapGun.targetObj)
        {
            var target = grapGun.targetObj;
            var targetRB = target.GetComponent<Rigidbody2D>();
            if (target.CompareTag("PhysObj") && !targetRB.isKinematic)
            {
                myRB.linearDamping = 0;
                //grappleGun.CallRelease();
                //target.GetComponent<Rigidbody2D>().isKinematic = true;
            }
        }
    }

}
