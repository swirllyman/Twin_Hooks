using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeedGround = 1;
    [SerializeField] float moveSpeedAir = .35f;
    [SerializeField] float maxVelocity = 1;
    [SerializeField] float wallDistanceCheck = .08f;
    [SerializeField] Vector2 verticalVelocityMax = new Vector2(3, 3);
    [SerializeField] AudioClip runClip;

    [Header("Jumping")]
    [SerializeField] float initialJumpForce = 2.5f;
    [SerializeField] float sustainedJumpForce = .5f;
    [SerializeField] float groundedDistance = .1f;
    [SerializeField] float groundRadiusCheck = .08f;
    [SerializeField] AudioClip landingClip;
    [SerializeField] AudioClip jumpClip;

    [Header("Slope")]
    [SerializeField] float slopeCheckDistance;
    [SerializeField] float maxSlopeAngle;
    [SerializeField] PhysicsMaterial2D noFriction;
    [SerializeField] PhysicsMaterial2D fullFriction;

    [Header("Extras")]
    [SerializeField] Animator myAnim;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] float resetDistance = 50;
    [SerializeField] PlayerGrabber grabber;

    Rigidbody2D myBody;
    CapsuleCollider2D myCollider;
    AudioSource audioSource;

    Vector3 startPosition;
    Vector2 slopeNormalPerp;
    Vector2 movePos;
    Vector2 moveDir;
    Vector2 slopeHitAngle;

    Coroutine jumpRoutine;

    private bool isOnSlope;
    private bool canWalkOnSlope;
    private float slopeDownAngle;
    private float slopeAngle;
    private float lastSlopeAngle;

    bool justJumped = false;
    bool grounded = false;
    bool wasGrounded = false;
    bool wasMoving = false;
    bool facingRight = true;

    internal bool pauseMovement = false;

    internal void PauseMovement(bool pause)
    {
        pauseMovement = pause;
        if (pause)
        {
            movePos = Vector2.zero;
        }
    }

    #region Mono
    private void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();

        startPosition = transform.position;

        grabber = GetComponent<PlayerGrabber>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckGrounded();

        if (!pauseMovement)
        {
            CheckMovementInput();
            CheckJump();
        }
        CheckReset();
        UpdateAnimation();
    }

    void CheckMovementInput()
    {
        movePos = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void FixedUpdate()
    {
        SlopeCheck();
        UpdateMovement();
        UpdateJump();
    }
    #endregion

    void MovingChanged()
    {
        wasMoving = !wasMoving;

        if (grounded)
        {
            if (wasMoving)
            {
                audioSource.clip = runClip;
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
    }

    void UpdateMovement()
    {

        moveDir = new Vector2(movePos.x * (grounded ? moveSpeedGround : moveSpeedAir), 0);

        if(Mathf.Abs(moveDir.x) > 0)
        {
            if (!wasMoving)
            {
                MovingChanged();
            }
        }
        else
        {
            if (wasMoving)
            {
                MovingChanged();
            }
        }

        if (moveDir.x > 0)
        {
            transform.rotation = Quaternion.Euler(Vector3.zero);
            facingRight = true;
        }
        else if (moveDir.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            facingRight = false;
        }

        if (CanMove(moveDir))
        {
            if (isOnSlope)
            {
                //Vector2 moveSlope = new Vector2(Mathf.Abs(slopeHitAngle.x), Mathf.Abs(slopeHitAngle.y));
                if (canWalkOnSlope)
                {
                    
                    myBody.AddForce(slopeHitAngle * (facingRight ? moveDir.x : -moveDir.x), ForceMode2D.Impulse);
                }
            }
            else
            {
                myBody.AddForce(moveDir, ForceMode2D.Impulse);
            }
        }
        //if (moveDir.x > 0)
        //{
        //    transform.rotation = Quaternion.Euler(Vector3.zero);
        //}
        //else if (moveDir.x < 0)
        //{
        //    transform.rotation = Quaternion.Euler(0, 180, 0);
        //}
        //if (CanMove(moveDir))
        //{
        //    myBody.AddForce(moveDir, ForceMode2D.Impulse);
        //}

        //Limiting Velocity
        if (Mathf.Abs(myBody.linearVelocity.x) > maxVelocity)
        {
            myBody.linearVelocity = new Vector3(myBody.linearVelocity.x > 0 ? maxVelocity : -maxVelocity, myBody.linearVelocity.y);
        }

        if (myBody.linearVelocity.y < 0 && Mathf.Abs(myBody.linearVelocity.y) > verticalVelocityMax.x)
        {
            myBody.linearVelocity = new Vector3(myBody.linearVelocity.x, -verticalVelocityMax.x);
        }

        if (myBody.linearVelocity.y > 0 && Mathf.Abs(myBody.linearVelocity.y) > verticalVelocityMax.y)
        {
            myBody.linearVelocity = new Vector3(myBody.linearVelocity.x, verticalVelocityMax.y);
        }
    }
    
    void UpdateAnimation()
    {
        myAnim.SetFloat("GroundSpeed", Mathf.Abs(moveDir.x) > 0 ? 1.0f : 0.0f);
        myAnim.SetBool("Falling", !grounded);
        if (isOnSlope)
        {
            myAnim.SetBool("Falling", !canWalkOnSlope);
        }
        //if (!justJumped)
        //{
        //    myAnim.SetFloat("GroundSpeed", Mathf.Abs(moveDir.x) > 0 ? 1.0f : 0.0f);
        //    myAnim.SetBool("Falling", !grounded);
        //}
    }

    void CheckReset()
    {
        if (Mathf.Abs(transform.position.x) > resetDistance || Mathf.Abs(transform.position.y) > resetDistance)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void Reset()
    {
        myBody.linearVelocity = Vector3.zero;
        transform.position = startPosition;
    }

    #region Jumping
    void CheckJump()
    {
        if (grounded & !justJumped)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
        }
    }

    void Jump()
    {
        justJumped = true;
        isOnSlope = false;
        //Debug.Log("Jumping");
        myAnim.SetBool("Jumping", true);
        myBody.AddForce(Vector3.up * initialJumpForce, ForceMode2D.Impulse);
        if (jumpRoutine != null) StopCoroutine(jumpRoutine);
        jumpRoutine = StartCoroutine(ResetJump());
        audioSource.Stop();
        audioSource.PlayOneShot(jumpClip);
    }
    
    void KickJump()
    {
        if (grabber == null)
        {
            Debug.Log("Error: grabber not found. Exiting KickJump");
            return; 
        }        
        
        justJumped = true;
        isOnSlope = false;
        //Debug.Log("Jumping");
        myAnim.SetBool("Jumping", true);
        //Add force in aim direction
        Vector3 kickJumpDirection = grabber.get
        myBody.AddForce(Vector3.up * initialJumpForce, ForceMode2D.Impulse);

        if (jumpRoutine != null) StopCoroutine(jumpRoutine);
        jumpRoutine = StartCoroutine(ResetJump());
        audioSource.Stop();
        audioSource.PlayOneShot(jumpClip);
    }


    void UpdateJump()
    {
        if (justJumped)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                myBody.AddForce(Vector2.up * sustainedJumpForce);
            }
        }
    }

    IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(1.0f);
        StopJump();
    }

    void StopJump()
    {
        if (jumpRoutine != null) StopCoroutine(jumpRoutine);
        myAnim.SetBool("Jumping", false);
        justJumped = false;
    }
    #endregion

    #region Slope
    private void SlopeCheck()
    {
        Vector2 checkPos = transform.position - (Vector3)(new Vector2(0.0f, myCollider.size.y / 2));

        Vector2 dir = moveDir.x > 0 ? Vector3.right : Vector3.left;
        RaycastHit2D hitSlope = Physics2D.Raycast(checkPos, dir, slopeCheckDistance, collisionMask);
        Debug.DrawRay(checkPos, dir * slopeCheckDistance, hitSlope.collider == null ? Color.red : Color.blue);
        if (hitSlope.collider != null && Mathf.Abs(hitSlope.normal.x) < .9f)
        {
            //print("Hit Normal: " + hitSlope.normal);
            isOnSlope = true;
            slopeAngle = Vector2.Angle(hitSlope.normal, Vector3.up);
            if (!facingRight)
            {
                slopeAngle += 90;
            }
            slopeHitAngle = GetDirectionVector2D(slopeAngle);

            if (Mathf.Max(Mathf.Abs(slopeHitAngle.x), Mathf.Abs(slopeHitAngle.y)) > maxSlopeAngle)
            {
                canWalkOnSlope = false;
                Debug.DrawRay(checkPos, slopeHitAngle * slopeCheckDistance, Color.red);
            }
            else
            {
                canWalkOnSlope = true;
                Debug.DrawRay(checkPos, slopeHitAngle * slopeCheckDistance, Color.yellow);
            }

        }
        else
        {
            slopeAngle = 0.0f;
            isOnSlope = false;
        }

        if (moveDir.x == 0.0f)
        {
            myBody.sharedMaterial = fullFriction;

            if (isOnSlope & !canWalkOnSlope)
            {
                myBody.sharedMaterial = noFriction;
            }
        }
        else
        {
            myBody.sharedMaterial = noFriction;
        }
    }

    Vector3 GetDirectionVector2D(float angle)
    {
        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
    }
    #endregion

    //return true if we are NOT moving into a wall
    bool CanMove(Vector2 moveDir)
    {
        SlopeCheck();

        bool canMove = Physics2D.Raycast(transform.position, moveDir.x > 0 ? Vector3.right : Vector3.left, wallDistanceCheck, collisionMask).collider == null;
        Debug.DrawRay(transform.position, (moveDir.x > 0 ? Vector3.right : Vector3.left) * wallDistanceCheck, canMove ? Color.green : Color.red);
        return canMove;
    }

    //return true if an object is under us
    void CheckGrounded()
    {
        grounded = Physics2D.CircleCast(transform.position, groundRadiusCheck, Vector3.down, groundedDistance, collisionMask).collider != null;
        Debug.DrawRay(transform.position, Vector3.down * groundedDistance, grounded ? Color.green : Color.red);

        if (isOnSlope & !canWalkOnSlope)
            grounded = false;

        if (wasGrounded != grounded)
        {
            GroundedChanged();
        }
    }

    void GroundedChanged()
    {
        wasGrounded = grounded;
        if (grounded)
        {
            StopJump();
        }

        if(!justJumped)
        {
            if (!grounded)
            {
                audioSource.Stop();
            }
            else
            {
                if (wasMoving)
                {
                    audioSource.clip = runClip;
                    audioSource.Play();
                }
                else
                {
                    audioSource.Stop();
                }
                audioSource.PlayOneShot(landingClip);
            }
        }
    }
}