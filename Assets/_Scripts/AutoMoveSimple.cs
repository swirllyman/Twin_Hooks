using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(EnemyController))]
public class AutoMoveSimple : MonoBehaviour
{
    public float moveSpeed, forwardRaycastDistance = 2f, groundRaycastDistance = 2f;
    public bool isMoving = true,
                movingRight = true,
                isPatrolling = false;
                
    public Transform groundDetection;
    public LayerMask layerMasks;
    [SerializeField] private SpriteRenderer m_EnemySprite;
    Rigidbody2D myRB;
    public Vector2 forwardDirection = Vector2.right;
    float boxExtents;
    bool defaultPatrolling;

    EnemyController enemyController;
    float defaultMoveSpeed, halfMoveSpeed;
    internal bool pauseMovement = false;

    Vector3 startPosition;
    Vector2 slopeNormalPerp;
    Vector2 movePos;
    Vector2 moveDir;
    Vector2 slopeHitAngle;

    public delegate void AutoMoveDirectionChanged();
    public event AutoMoveDirectionChanged onAutoMoveDirChanged;

    internal void PauseMovement(bool pause)
    {
        pauseMovement = pause;
        if (pause)
        {
            movePos = Vector2.zero;
        }
    }

    public void StopMoving()
    {
        if (enemyController.GetState() != EnemyState.Normal)
            isMoving = false;
    }

    public void StartMoving()
    {
        if (enemyController.GetState() == EnemyState.Normal)
            isMoving = true;
        isPatrolling = defaultPatrolling;
    }
    public void GetEnemyStateAndToggleMove()
    {
        if (enemyController.GetState() != EnemyState.Normal)
            MoveToggle(true);//isMoving = false;
        else
        {
            isPatrolling = false;
            MoveToggle(false);
        }
        //isMoving = true;
    }

    public void MoveToggle(bool _m)
    {
        isMoving = _m;
        if(isMoving == true && defaultPatrolling)
            isPatrolling = _m;
    }

    public void ChangeMove(bool _c)
    {
        if (_c == true)
            moveSpeed = halfMoveSpeed;
        else
            moveSpeed = defaultMoveSpeed;
    }

    public Vector2 GetMoveDirection()
    {
        return forwardDirection;
    }

    private void Awake()
    {
        defaultPatrolling = isPatrolling;
        enemyController = GetComponent<EnemyController>();
        defaultPatrolling = isPatrolling;
        if (enemyController == null)
        {
            Debug.LogError("EnemyController not found on " + gameObject.name);
        }
        //else
        //{
        //    enemyController.onEnemyStateChanged += GetEnemyStateAndToggleMove;
        //}
        EnemyBase e = GetComponent<EnemyBase>();
        if (e != null)
        {
            e.onPull += StopMoving;
            e.onDrop += StartMoving;
        }

        boxExtents = m_EnemySprite.bounds.extents.x;
        if(!movingRight)
        {
            boxExtents = -boxExtents;
            m_EnemySprite.flipX = !movingRight;
            forwardDirection = Vector2.left;
            InvertGroundXPosition();
        }

        //TurnAround();
        halfMoveSpeed = moveSpeed / 2;
        defaultMoveSpeed = moveSpeed;
        myRB = GetComponent<Rigidbody2D>();
    }

    void TurnAround() 
    { //If this changes, change the code in Awake()
        Debug.Log("Turning: Moving right is: " + movingRight);

        //Options:
        // mult scale & movespeed by -1
        if (movingRight)
        {
            //transform.eulerAngles = new Vector3(0, -180, 0); 
            //possibly change below to a 
            boxExtents = -boxExtents;
            //m_EnemySprite.flipX = movingRight;
            forwardDirection = Vector2.left;
            movingRight = false;
        }
        else
        {
            //transform.eulerAngles = new Vector3(0, 0, 0);
            boxExtents = -boxExtents;
            //m_EnemySprite.flipX = !movingRight;
            forwardDirection = Vector2.right;
            movingRight = true;
            //groundDetection.transform.Translate(new Vector2(groundDetection.transform.position.x, groundDetection.transform.position.y));
        }
        m_EnemySprite.flipX = movingRight;
        onAutoMoveDirChanged?.Invoke();
        InvertGroundXPosition();
    }

    // Update is called once per frame
    void InvertGroundXPosition()
    {
        groundDetection.transform.position.Set(-groundDetection.transform.position.x, groundDetection.transform.position.y, groundDetection.transform.position.z);
        //FIXED To turn around GroundDetection object. The easiest way to change x in the local position is to 
        //create a transformHolder Vector3, give it the localPosition value and change its x, then re-assign localPosition to the transformHolder.
        Vector3 transformHolder = groundDetection.transform.localPosition;
        transformHolder.x *= -1;
        groundDetection.transform.localPosition = transformHolder;
        Debug.DrawLine(groundDetection.position, (Vector3.down * groundRaycastDistance) + groundDetection.position);
    }

    void Update()
    {
        if(isMoving)
        {
            //Move
            //transform.Translate(Vector2.right * moveSpeed * Time.deltaTime); // Only move by time.deltatime if you're using Translate
            myRB.linearVelocity = moveSpeed * forwardDirection;
            //Debug.Log("Moving right is: " + movingRight);
            if(isPatrolling)
            {
                //Cast a ray down to detect ground ahead
                RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, groundRaycastDistance, layerMasks);
                Debug.DrawLine(groundDetection.position, (Vector3.down * groundRaycastDistance) + groundDetection.position);

                if (!groundInfo.collider)
                {
                    TurnAround();
                }

                var halfWidth = new Vector2(transform.position.x + boxExtents, transform.position.y);
                RaycastHit2D collidingWithSide = Physics2D.Raycast(halfWidth, forwardDirection  /** (-myRB.velocity/myRB.velocity)*/, forwardRaycastDistance/2, layerMasks);
                
                var leftPerpPos = halfWidth + forwardDirection * forwardRaycastDistance/2;
                //Debug.Log("Enemy Forward Direction " + forwardDirection);
                Debug.DrawRay(halfWidth, forwardDirection/*transform.right * (-myRB.velocity / myRB.velocity)*/,/*transform.right * (raycastDistance/2)*/ Color.red);
                //Debug.DrawLine(transform.position, leftPerpPos, Color.red);
                if (collidingWithSide)
                {
                    //Debug.Log("Enemy Forward Direction Before" + forwardDirection);

                    //Debug.Log(gameObject.name + " found side: " + collidingWithSide.collider.name + "\nTurning around"  );
                    TurnAround();
                    //Debug.Log("Enemy Forward Direction After" + forwardDirection);

                }

                Debug.DrawLine(transform.position, leftPerpPos, Color.red);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if (collision.gameObject.CompareTag("Environment"))
        //{
        //    RaycastHit2D collidingWithSide = Physics2D.Raycast(transform.position, transform.right * moveSpeed, raycastDistance, layerMasks);
        //    var leftPerpPos = (Vector2)transform.position + (Vector2)transform.right * raycastDistance;

        //    Debug.DrawLine(transform.position, leftPerpPos, Color.red);
        //    if (collidingWithSide)
        //    {
        //        Debug.Log("Found side. Turning around ");
        //        TurnAround();
        //    }
        //}

        //if(isMoving && collision.gameObject.CompareTag("Enemy") 
        //    && myRB.velocity.y < 0 && myRB.velocity.y > 0)
        //{
        //    TurnAround();
        //}
    }
}
