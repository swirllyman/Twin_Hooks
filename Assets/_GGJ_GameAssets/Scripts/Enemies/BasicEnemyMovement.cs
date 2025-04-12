using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class BasicEnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeedGround = 1;
    [SerializeField] float moveSpeedAir = .35f;
    [SerializeField] float maxVelocity = 1;
    [SerializeField] float wallDistanceCheck = .08f;
    [SerializeField] float forwardRaycastDistance = 2f;
    [SerializeField] float groundRaycastDistance = 2f;
    [SerializeField] Vector2 verticalVelocityMax = new Vector2(3, 3);
    [SerializeField] AudioClip runClip;
    [SerializeField] bool canTurnOnEdge;
    [SerializeField] Transform edgeDetector;

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
    [SerializeField] Animator? myAnim;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] float resetDistance = 50;

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
    bool onEdge = false;
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
        EnemyBase e = GetComponent<EnemyBase>();
        if(e != null)
        {
            e.onEnemyPickedUp += PauseMovement;
        }

        if (canTurnOnEdge && edgeDetector != null)
            Debug.Log("Edge Detector found.");
            
        myBody = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();

        startPosition = transform.position;

        myBody.linearVelocity = new Vector3(0.01f, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        CheckGrounded();
        if (canTurnOnEdge)
            CheckEdge();

        if (!pauseMovement)
        {
            CheckMovementDirection();
            //CheckJump();
        }
        CheckReset();
        //UpdateAnimation();
    }

    void CheckMovementDirection()
    {
        if (facingRight) {
            movePos = new Vector2(1, 0);
        }
        else {
            movePos = new Vector2(-1, 0);
        }
    }

    private void FixedUpdate()
    {
        //Every fixed update, check the slope, update movement and update jump
        CheckSlope();
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
        //moveDir is now Get vector2 with x = movePos.x times moveSpeedGround if grounded or times moveSpeedAir if not grounded; y = 0
        moveDir = new Vector2(movePos.x * (grounded ? moveSpeedGround : moveSpeedAir), 0);

        //If Abs value of moveDir x is over 0(have any sort of x movement), and was NOT moving, call MovingChanged
        if(Mathf.Abs(moveDir.x) > 0)
        {
            if (!wasMoving)
            {
                MovingChanged();
            }
        }
        else //if not and was moving, call MovingChanged
        {
            if (wasMoving)
            {
                MovingChanged();
            }
        }

        //If moving right, set rotation to Quat.Euler's Vec3's 0; set facing right to true
        if (moveDir.x > 0)
        {
            transform.rotation = Quaternion.Euler(Vector3.zero);
            facingRight = true;
        }
        //If moving left, set rotation to Quat.Euler's 180y; set facing right to false
        else if (moveDir.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            facingRight = false;
        }

        //If I can move in moveDirection, do various movement. Else, turn around by inverting facingRight. 
        if (CanMove(moveDir))
        {
            //Turn around
            if (canTurnOnEdge && grounded && onEdge)
            {
                /*if (CanMove(moveDir) && grounded && onEdge)
                {*/
                    //Move in opposite direction as long as onEdge is true.
                    //To try:
                    /*
                     * x Change forcemode2d to force
                     * x move facing right def below addforce
                     * x add multiplier to movedir
                     * x Add '-' back to moveDir 
                     * x Move this code into CanMove encapsulation inside UpdateMovement
                     * Add more similar code from CheckGrounded after debug.drawray
                     * x Set moveDir x to -moveDir x
                     * x Multiply moveDir by facingRight in AddForce after update
                     * Call EdgeCheck
                     * Set onEdge to false after
                     */

                //myBody.AddForce(moveDir, ForceMode2D.Impulse);
                facingRight = !facingRight;
                moveDir = -moveDir;
                Debug.Log("Edge found. Moving in opposite direction.");
                CheckEdge();
                //}
            }
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

            //Turn around
            //if (canTurnOnEdge)
            //{
            //    if (CanMove(moveDir) && grounded && onEdge)
            //    {
                    //Move in opposite direction as long as onEdge is true.


            //        myBody.AddForce(moveDir, ForceMode2D.Impulse);
            //        facingRight = !facingRight;
            //        Debug.Log("Edge found. Moving in opposite direction.");
            
            //    }
            //} 
        }
        //else{
        //    facingRight = !facingRight;
        //}

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

        //If enemy hits a wall or the edge, 
        //Check for ground
        //CheckGrounded for wall

        //Check if turn on edge is true.

        
    }
    
    void UpdateAnimation()
    {
        myAnim?.SetFloat("GroundSpeed", Mathf.Abs(moveDir.x) > 0 ? 1.0f : 0.0f);
        myAnim?.SetBool("Falling", !grounded);
        if (isOnSlope)
        {
            myAnim?.SetBool("Falling", !canWalkOnSlope);
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
            // right now it is never time to jump
            bool isTimeToJump = false;
            if (isTimeToJump)
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
        myAnim?.SetBool("Jumping", true);
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
    private void CheckSlope()
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
        CheckSlope();

        bool canMove = Physics2D.Raycast(transform.position, moveDir.x > 0 ? Vector3.right : Vector3.left, wallDistanceCheck, collisionMask).collider == null;
        Debug.DrawRay(transform.position, (moveDir.x > 0 ? Vector3.right : Vector3.left) * wallDistanceCheck, canMove ? Color.green : Color.red);
        return canMove;
    }

    void CheckEdge()
    {        
        //onEdge = Physics2D.CircleCast(edgeDetector.position, groundRadiusCheck, Vector3.down, groundedDistance, collisionMask).collider == null;
        onEdge = Physics2D.Raycast(edgeDetector.position, Vector3.down, groundRaycastDistance, collisionMask).collider == null;
        Debug.DrawRay(edgeDetector.position, Vector3.down * groundRaycastDistance, onEdge ? Color.green : Color.red);
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
                audioSource.PlayOneShot(landingClip);
            }
        }
    }

    void InvertGroundXPosition()
    {
        edgeDetector.transform.position.Set(-edgeDetector.transform.position.x, edgeDetector.transform.position.y, edgeDetector.transform.position.z);
        //FIXED To turn around edgeDetector object. The easiest way to change x in the local position is to 
        //create a transformHolder Vector3, give it the localPosition value and change its x, then re-assign localPosition to the transformHolder.
        Vector3 transformHolder = edgeDetector.transform.localPosition;
        transformHolder.x *= -1;
        edgeDetector.transform.localPosition = transformHolder;
        Debug.DrawLine(edgeDetector.position, (Vector3.down * groundRaycastDistance) + edgeDetector.position);
    }
}