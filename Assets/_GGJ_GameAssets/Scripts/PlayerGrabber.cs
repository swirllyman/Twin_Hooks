using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Aimer2D))]
public class PlayerGrabber : MonoBehaviour
{
    [SerializeField] Transform grabberTip;
    [SerializeField] Transform grabberHandsTransform;
    [SerializeField] float attachDistance = .7f;
    [SerializeField] float throwForce = 1.5f;
    [SerializeField] float minDistance = .2f;
    [SerializeField] float shotSpeed = .7f;
    [SerializeField] float shotDistanceCheck = .15f;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] LineRenderer lineRend;
    [SerializeField] LineRenderer trajectoryRend;
    [SerializeField] Animator myAnim;

    internal Grabbable attachedGrabbable;
    internal bool holding = false;
    Rigidbody2D mybody;
    Aimer2D aimer;
    Vector2 currentHitPos;
    RaycastHit2D hit;
    bool attached = false;
    bool justShot = false;

    private void Awake()
    {
        mybody = GetComponent<Rigidbody2D>();
        aimer = GetComponent<Aimer2D>();
        grabberTip.parent = null;
        lineRend.enabled = false;
        trajectoryRend.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
    }

    private void LateUpdate()
    {
        UpdateAim();
        if (justShot)
        {
            Vector3 moveDir = (hit.point - (Vector2)grabberTip.position).normalized * Time.deltaTime * shotSpeed;
            grabberTip.position += moveDir;
            lineRend.SetPosition(0, grabberHandsTransform.position);
            lineRend.SetPosition(1, grabberTip.position);
            if (Vector3.Distance(grabberTip.position, hit.point) < shotDistanceCheck)
            {
                AttachGrabber();
            }
        }

        if (attached)
        {
            if (Vector3.Distance(grabberTip.position, transform.position) > minDistance)
            {
                Vector3 moveDir = ((Vector2)transform.position - (Vector2)grabberTip.position).normalized * Time.deltaTime * shotSpeed;
                grabberTip.position += moveDir;
                attachedGrabbable.transform.position = grabberTip.position;
                lineRend.SetPosition(0, grabberHandsTransform.position);
                lineRend.SetPosition(1, grabberTip.position);
            }
            else
            {
                HoldItem();
            }
        }
        if (holding)
        {
            lineRend.SetPosition(0, grabberHandsTransform.position);
            lineRend.SetPosition(1, grabberTip.position);
            Vector3[] trajectory = Plot(mybody, attachedGrabbable.transform.position, aimer.aimDirection * throwForce, 100);
            trajectoryRend.positionCount = trajectory.Length;
            for (int i = 0; i < trajectoryRend.positionCount; i++)
            {
                trajectoryRend.SetPositions(trajectory);
            }
        }
    }

    void UpdateAim()
    {
        if (!justShot)
            hit = Physics2D.Raycast(transform.position, aimer.aimDirection, attachDistance, collisionMask);
        if (hit.collider != null && hit.collider.CompareTag("Grabbable"))
        {
            if (!attached)
            {
                aimer.aimDistance = hit.distance;
            }
        }
        else
        {
            aimer.aimDistance = attachDistance;
        }
    }

    void CheckInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (attached & !justShot)
            {
                DropAttachedGrabbable();
            }
            else if (justShot)
            {
                StopShot();
            }
        }

        if (Input.GetMouseButton(1))
        {
            if (hit.collider != null && !attached & !justShot)
            {
                ShootGrabber();
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            DropAttachedGrabbable();
        }
    }

    void StopShot()
    {
        justShot = false;
        lineRend.enabled = false;
    }

    void HoldItem()
    {
        trajectoryRend.enabled = true;
        holding = true;
    }

    internal void DropAttachedGrabbable()
    {
        if (attached)
        {
            if (holding)
            {
                attachedGrabbable.Throw(aimer.aimDirection, throwForce);
            }
            else
            {
                attachedGrabbable.Drop();
            }
            holding = false;
            attached = false;
            trajectoryRend.enabled = false;
            lineRend.enabled = false;
            justShot = false;
            attachedGrabbable.transform.SetParent(null);
            attachedGrabbable = null;
        }
    }

    void ShootGrabber()
    {
        lineRend.enabled = true;
        grabberTip.position = transform.position;
        justShot = true;
    }

    void AttachGrabber()
    {
        attached = true;
        justShot = false;

        grabberTip.position = hit.point;
        lineRend.SetPosition(0, grabberHandsTransform.position);
        lineRend.SetPosition(1, grabberTip.position);

        attachedGrabbable = hit.collider.GetComponent<Grabbable>();
        attachedGrabbable.transform.SetParent(grabberTip);
        attachedGrabbable.transform.localPosition = Vector3.zero;
        attachedGrabbable.PickUp();
    }

    public Vector3[] Plot(Rigidbody2D body, Vector2 pos, Vector2 vel, int steps)
    {
        List<Vector3> results = new List<Vector3>();
        float timestep = Time.fixedDeltaTime / Physics2D.velocityIterations;
        Vector2 gravityAccel = Physics2D.gravity * body.gravityScale * timestep * timestep;
        float drag = 1f - timestep * body.drag;
        Vector2 moveStep = vel * timestep;

        for (int i = 0; i < steps; i++)
        {
            moveStep += gravityAccel;
            moveStep *= drag;
            pos += moveStep;
            if(i % 3 == 0)
                results.Add(pos);
        }
        return results.ToArray();
    }
}