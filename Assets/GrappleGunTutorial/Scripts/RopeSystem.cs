using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;
using System.Linq;

public class RopeSystem : MonoBehaviour
{
    public GameObject ropeHingeAnch;
    public DistanceJoint2D ropeJoint;
    public Transform aimReticle;
    public SpriteRenderer aimReticleSprite;
    public PlatformerCharacter2D_Alt platformerChar;
    private bool isRopeAttached, playerToHook, hookToPlayer;
    private Vector2 playerPos;
    private Rigidbody2D ropeHingeAnchRb;
    private SpriteRenderer ropeHingeAnchSprite;

    public LineRenderer ropeRenderer;
    public LayerMask ropeLayerMask;
    private float ropeMaxCastDistance = 20f;
    private List<Vector2> ropePositions = new List<Vector2>();
    private Dictionary<Vector2, int> wrapPointsLookup = new Dictionary<Vector2, int>(); //track the positions that the rope should be wrapping around.

    private bool isDistanceSet;

    public float climbSpeed = 3f,
                 minDistanceToDetach = 0.005f;
    private bool isColliding;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        // 2
        ropeJoint.enabled = false;
        playerPos = transform.position;
        ropeHingeAnchRb = ropeHingeAnch.GetComponent<Rigidbody2D>();
        ropeHingeAnchSprite = ropeHingeAnch.GetComponent<SpriteRenderer>();
    }

    private void HandleRopeLength()
    {
        // 1
        //if (Input.GetAxis("Vertical") >= 1f && isRopeAttached && !isColliding)
        if (Input.GetKeyDown(KeyCode.W) && isRopeAttached && /*!isColliding && */!playerToHook)
        {
            //ropeJoint.distance -= Time.deltaTime * climbSpeed;
            playerToHook = true;
        }
        else if (Input.GetKeyDown(KeyCode.S)/*GetAxis("Vertical") < 0f*/ && isRopeAttached /*&& !isColliding */&& !playerToHook)
        {
            hookToPlayer = true;
            isRopeAttached = false;
            //ropeJoint.distance += Time.deltaTime * climbSpeed;
        }
    }

    private void PullPlayerToHook()
    {
        if (isRopeAttached && !isColliding)
        {           
            ropeJoint.distance -= Time.deltaTime * climbSpeed;
            Debug.Log("Distance: " + ropeJoint.distance);

            if (ropeJoint.distance <= minDistanceToDetach)
                ResetRope();
        }
    }

    private void PullHookToPlayer()
    {
        //Unparent anchor
        if (ropeJoint.transform.parent != null)
            ropeJoint.transform.parent = null;
        //Make object dynamic 
        if (ropeJoint.gameObject.GetComponent<Rigidbody>().isKinematic)
            ropeJoint.gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    void moveRopeToPlayer()
    {
        //if (/*isRopeAttached && */!isColliding)
        //{
        ropeJoint.distance -= Time.deltaTime * climbSpeed;
        Debug.Log("Distance: " + ropeJoint.distance);

        if (ropeJoint.distance <= minDistanceToDetach)
            ResetRope();
        //}
    }
    void Update()
    {
        // 3
        var worldMousePosition =
            Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
        var directionFacing = worldMousePosition - transform.position;
        var aimAngle = Mathf.Atan2(directionFacing.y, directionFacing.x);
        if (aimAngle < 0f)
        {
            aimAngle = Mathf.PI * 2 + aimAngle;
        }

        // 4
        var aimDir = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.right;
        // 5
        playerPos = transform.position;

        // 6
        if (!isRopeAttached)
        {
            platformerChar.isSwinging = false;          
        }
        else
        {
            platformerChar.isSwinging = true;
            platformerChar.ropeHook = ropePositions.Last();
            SetAimReticlePosition(aimAngle);
            aimReticleSprite.enabled = false;
            // 1: If the ropePositions list has any positions stored, then...
            if (ropePositions.Count > 0)
            {
                // 2: Fire a raycast out from the player's position, in the direction of the player looking at the last rope position in the list — 
                //the pivot point where the grappling hook is hooked into the rock — with a raycast distance set to the distance between the 
                //player and rope pivot position.
                var lastRopePoint = ropePositions.Last();
                var playerToCurrentNextHit = Physics2D.Raycast(playerPos, (lastRopePoint - playerPos).normalized, Vector2.Distance(playerPos, lastRopePoint) - 0.1f, ropeLayerMask);

                // 3: If the raycast hits something, then that hit object's collider is safe cast to a PolygonCollider2D. 
                // As long as it's a real PolygonCollider2D, then the closest vertex position on that collider is returned as a Vector2, 
                // using that handy-dandy method you wrote earlier.
                if (playerToCurrentNextHit)
                {
                    var colliderWithVertices = playerToCurrentNextHit.collider as PolygonCollider2D;
                    if (colliderWithVertices != null)
                    {
                        var closestPointToHit = GetClosestColliderPointFromRaycastHit(playerToCurrentNextHit, colliderWithVertices);

                        // 4: The wrapPointsLookup is checked to 
                        // make sure the same position is not being wrapped again. 
                        // If it is, then it'll reset the rope and cut it, dropping the player.
                        if (wrapPointsLookup.ContainsKey(closestPointToHit))
                        {
                            ResetRope();
                            return;
                        }

                        // 5: The ropePositions list is now updated, adding the position the rope 
                        // should wrap around, and the wrapPointsLookup dictionary is also updated. 
                        // Lastly the distanceSet flag is disabled, so that UpdateRopePositions() method 
                        // can re-configure the rope's distances to take into account the new rope length and segments.

                        ropePositions.Add(closestPointToHit);
                        wrapPointsLookup.Add(closestPointToHit, 0);
                        isDistanceSet = false;
                    }
                }
            }
        }

        HandleInput(aimDir);
        UpdateRopePositions();
        HandleRopeLength();
        HandleRopeUnwrap();
    }

    // 1
    private Vector2 GetClosestColliderPointFromRaycastHit(RaycastHit2D hit, PolygonCollider2D polyCollider)
    {
        // 2
        var distanceDictionary = polyCollider.points.ToDictionary<Vector2, float, Vector2>(
            position => Vector2.Distance(hit.point, polyCollider.transform.TransformPoint(position)),
            position => polyCollider.transform.TransformPoint(position));

        // 3
        var orderedDictionary = distanceDictionary.OrderBy(e => e.Key);
        return orderedDictionary.Any() ? orderedDictionary.First().Value : Vector2.zero;
    }

    //private Vector2 GetClosestColliderPointFromRaycastHit(RaycastHit2D hit, Collider2D col2D)
    //{
    //    // 2
    //    var distanceDictionary = col2D.points.ToDictionary<Vector2, float, Vector2>(
    //        position => Vector2.Distance(hit.point, col2D.transform.TransformPoint(position)),
    //        position => col2D.transform.TransformPoint(position));

    //    // 3
    //    var orderedDictionary = distanceDictionary.OrderBy(e => e.Key);
    //    return orderedDictionary.Any() ? orderedDictionary.First().Value : Vector2.zero;
    //}
    private void UpdateRopePositions()
    {
        // 1
        if (!isRopeAttached)
        {
            return;
        }

        // 2
        ropeRenderer.positionCount = ropePositions.Count + 1;

        // 3
        for (var i = ropeRenderer.positionCount - 1; i >= 0; i--)
        {
            if (i != ropeRenderer.positionCount - 1) // if not the Last point of line renderer
            {
                ropeRenderer.SetPosition(i, ropePositions[i]);

                // 4: configures the ropeJoint distance to the distance between the player and 
                // the current rope position being looped over.
                if (i == ropePositions.Count - 1 || ropePositions.Count == 1)
                {
                    var ropePosition = ropePositions[ropePositions.Count - 1];
                    if (ropePositions.Count == 1)
                    {
                        ropeHingeAnchRb.transform.position = ropePosition;
                        if (!isDistanceSet)
                        {
                            ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            isDistanceSet = true;
                        }
                    }
                    else
                    {
                        ropeHingeAnchRb.transform.position = ropePosition;
                        if (!isDistanceSet)
                        {
                            ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            isDistanceSet = true;
                        }
                    }
                }
                // 5: if-statement handles the case where the rope position being looped over is the 
                //second -to-last one; that is, the point at which the rope connects to an object, a.k.a. 
                //the current hinge/anchor point. 
                    
                else if (i - 1 == ropePositions.IndexOf(ropePositions.Last()))
                {
                    var ropePosition = ropePositions.Last();
                    ropeHingeAnchRb.transform.position = ropePosition;
                    if (!isDistanceSet)
                    {
                        ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                        isDistanceSet = true;
                    }
                }
            }
            else
            {
                // 6: handles setting the rope's last vertex position to the player's current position
                ropeRenderer.SetPosition(i, transform.position);
            }
        }
    }

    private void SetAimReticlePosition(float aimAngle)
    {
        if (!aimReticleSprite.enabled)
        {
            aimReticleSprite.enabled = true;
        }

        var x = transform.position.x + 1f * Mathf.Cos(aimAngle);
        var y = transform.position.y + 1f * Mathf.Sin(aimAngle);

        var aimReticlePosition = new Vector3(x, y, 0);
        aimReticle.transform.position = aimReticlePosition;
    }

    
    // 1
    private void HandleInput(Vector2 aimDirection)
    {
        //if (playerToHook )
        //{
        //    PullPlayerToHook();
        //}
        //if (Input.GetMouseButtonUp(0))
        //{
        //    if (isRopeAttached)
        //    {
        //        playerToHook = true;
        //        return;
        //    }
        //}
        if (Input.GetMouseButtonDown(0))
        {
            // 2
            if (isRopeAttached)
            {               
                return;
            }
            ropeRenderer.enabled = true;

            var hit = Physics2D.Raycast(playerPos, aimDirection, ropeMaxCastDistance, ropeLayerMask);

            // 3
            if (hit.collider != null)
            {
                isRopeAttached = true;
                if (!ropePositions.Contains(hit.point))
                {
                    // 4
                    // Jump slightly to distance the player a little from the ground after grappling to something.
                    transform.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 2f), ForceMode2D.Impulse);
                    ropePositions.Add(hit.point);
                    ropeJoint.distance = Vector2.Distance(playerPos, hit.point);
                    ropeJoint.enabled = true;
                    ropeHingeAnchSprite.enabled = true;
                }
            }
            // 5
            else
            {
                ropeRenderer.enabled = false;
                isRopeAttached = false;
                ropeJoint.enabled = false;
            }
        }
        if (playerToHook)
        {
            moveRopeToPlayer();
        }
        if (Input.GetMouseButton(1))
        {
            ResetRope();
        }
    }

    // 6
    private void ResetRope()
    {
        playerToHook = false;
        hookToPlayer = false;
        ropeJoint.enabled = false;
        isRopeAttached = false;
        platformerChar.isSwinging = false;
        ropeRenderer.positionCount = 2;
        ropeRenderer.SetPosition(0, transform.position);
        ropeRenderer.SetPosition(1, transform.position);
        ropePositions.Clear();
        wrapPointsLookup.Clear();
        ropeHingeAnchSprite.enabled = false;
    }

    private void HandleRopeUnwrap()
    {
        if (ropePositions.Count <= 1)
        {
            return;
        }
        // Hinge = next point up from the player position
        // Anchor = next point up from the Hinge
        // Hinge Angle = Angle between anchor and hinge
        // Player Angle = Angle between anchor and player

        // 1
        var anchorIndex = ropePositions.Count - 2;
        // 2
        var hingeIndex = ropePositions.Count - 1;
        // 3
        var anchorPosition = ropePositions[anchorIndex];
        // 4
        var hingePosition = ropePositions[hingeIndex];
        // 5
        var hingeDir = hingePosition - anchorPosition;
        // 6
        var hingeAngle = Vector2.Angle(anchorPosition, hingeDir);
        // 7
        var playerDir = playerPos - anchorPosition;
        // 8
        var playerAngle = Vector2.Angle(anchorPosition, playerDir);

        if (!wrapPointsLookup.ContainsKey(hingePosition))
        {
            Debug.LogError("We were not tracking hingePosition (" + hingePosition + ") in the look up dictionary.");
            return;
        }

        if (playerAngle < hingeAngle)
        {
            // 1
            if (wrapPointsLookup[hingePosition] == 1)
            {
                UnwrapRopePosition(anchorIndex, hingeIndex);
                return;
            }

            // 2
            wrapPointsLookup[hingePosition] = -1;
        }
        else
        {
            // 3
            if (wrapPointsLookup[hingePosition] == -1)
            {
                UnwrapRopePosition(anchorIndex, hingeIndex);
                return;
            }

            // 4
            wrapPointsLookup[hingePosition] = 1;
        }

    }

    public void CallRopeHookAttached()
    {
        if (isRopeAttached)
            isRopeAttached = false;
        else
            isRopeAttached = true;
    }

    private void UnwrapRopePosition(int anchorIndex, int hingeIndex)
    {
        // 1: The current anchor index (the second rope position away 
        //from the slug) becomes the new hinge position and the old hinge position is 
        //removed (the one that was previously closest to the slug that we are now 'unwrapping'). 
        //The newAnchorPosition variable is set to the anchorIndex value in the rope positions list. 
        //This will be used to position the updated anchor position next. 
        var newAnchorPosition = ropePositions[anchorIndex];
        wrapPointsLookup.Remove(ropePositions[hingeIndex]);
        ropePositions.RemoveAt(hingeIndex);

        // 2: The rope hinge RigidBody2D (which is what the rope's 
        // DistanceJoint2D is attached to) has its position changed here to the new anchor position. 
        //This allows the seamless continued movement of the slug on his rope as he is connected to the 
        //DistanceJoint2D, and this joint should allow him to continue swinging based off the new position 
        //he is anchored to — in other words, the next point down the rope from his position.
        ropeHingeAnchRb.transform.position = newAnchorPosition;
        isDistanceSet = false;

        // Set new rope distance joint distance for anchor position if not yet set.
        if (isDistanceSet)
        {
            return;
        }
        ropeJoint.distance = Vector2.Distance(transform.position, newAnchorPosition);
        isDistanceSet = true;
    }

    void OnTriggerStay2D(Collider2D colliderStay)
    {
        isColliding = true;
    }

    private void OnTriggerExit2D(Collider2D colliderOnExit)
    {
        isColliding = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        isColliding = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isColliding = false;

    }

}
/*
 // 1
public GameObject ropeHingeAnchor;
public DistanceJoint2D ropeJoint;
public Transform crosshair;
public SpriteRenderer crosshairSprite;
public PlayerMovement playerMovement;
private bool isisRopeAttached;
private Vector2 playerPos;
private Rigidbody2D ropeHingeAnchRb;
private SpriteRenderer ropeHingeAnchSprite;

void Awake()
{
    // 2
    ropeJoint.enabled = false;
    playerPos = transform.position;
    ropeHingeAnchRb = ropeHingeAnchor.GetComponent<Rigidbody2D>();
    ropeHingeAnchSprite = ropeHingeAnchor.GetComponent<SpriteRenderer>();
}

void Update()
{
    // 3
    var worldMousePosition =
        Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
    var facingDirection = worldMousePosition - transform.position;
    var aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
    if (aimAngle < 0f)
    {
        aimAngle = Mathf.PI * 2 + aimAngle;
    }

    // 4
    var aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.right;
    // 5
    playerPos = transform.position;

    // 6
    if (!isisRopeAttached)
    {
    }
    else
    {
    }
}

     */
