using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GrapGunSystem))]

public class PhysObjectHolder : MonoBehaviour
{
    GameObject player;
    Rigidbody2D heldPhysicsRB = null;
    [SerializeField]float physShootSpeed = 10.0f;
    
    SpringJoint2D grappleGunSpring;
    public GrapGunSystem grappleGun;
    public GameObject grapShootPoint;
    [SerializeField] GameObject heldPhysObject = null;

    void Awake()
    {
        //GrapGunEvents.hookThrowObjectEvent.AddListener(Throw)
        player = GameObject.FindWithTag("Player");
        grappleGun = GetComponent<GrapGunSystem>();
        grappleGunSpring = GetComponent<SpringJoint2D>();
    }

    public void HoldTargetObj(GameObject _targetObj)
    {
        grappleGun.PlayHoldSound();
        heldPhysObject = _targetObj;
        heldPhysicsRB = heldPhysObject.GetComponent<Rigidbody2D>();

        heldPhysicsRB.Sleep();
        heldPhysicsRB.isKinematic = true;

        heldPhysObject.transform.position = grappleGun.shootPoint.position;
        heldPhysObject.transform.rotation = grappleGun.shootPoint.rotation;

        heldPhysObject.GetComponent<Collider2D>().isTrigger = true;
    }

    public void ThrowHeldObj()
    {
        if(heldPhysObject)
        {
            grappleGun.PlayThrowObjectSound();
            heldPhysicsRB.isKinematic = false;
            heldPhysObject.GetComponent<Collider2D>().isTrigger = false;
            
            heldPhysicsRB.AddForceAtPosition(physShootSpeed * transform.right/*heldPhysicsRB.velocity * heldPhysicsRB.mass * 10*/, heldPhysicsRB.transform.localPosition, ForceMode2D.Impulse);
            //player.GetComponent<Rigidbody2D>().drag = 100;
            if (heldPhysObject.GetComponent<PhysicsDamageHandler>())
            {
                heldPhysObject.GetComponent<PhysicsDamageHandler>().doesDamage = true;
                heldPhysObject.GetComponent<PhysicsDamageHandler>().isThrown = true;
            }
            //else
            //    throw new System.Exception("Error! " + target.gameObject.name + "doesn't have PhysicsDamageHandler!");
            heldPhysObject = null;
            grappleGun.CallRelease();

            //INVOKE THROW EVENT
            GrabberGunEvents.grabberThrowObjectEvent.Invoke();
        }
    }

    void FixedUpdate()
    {
        if(grappleGun.HookModeState == GrapGunMode.HoldingObject && heldPhysObject)
        {
            heldPhysObject.transform.position = grappleGun.shootPoint.position;
            heldPhysObject.transform.rotation = grappleGun.shootPoint.rotation;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (heldPhysObject && !grappleGun.m_IsHookActive && grappleGun.HookModeState == GrapGunMode.HoldingObject)
                ThrowHeldObj();
            //else if (heldPhysObject && heldPhysObject.CompareTag("PhysObj"))
            //{
            //    Debug.Log("Pulling to player!");
            //    PullTargetToPlayer();
            //}
        }
    }
}
