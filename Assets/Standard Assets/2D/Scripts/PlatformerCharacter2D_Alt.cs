using System;
using System.Collections;
using UnityEngine;
//using Unity.Entities;
using Unity.Collections;
using Unity;
#pragma warning disable 649
namespace UnityStandardAssets._2D
{
    public class PlatformerCharacter2D_Alt : MonoBehaviour
    {
        [SerializeField] private float m_MaxSpeed = 10f;                    // The fastest the player can travel in the x axis.
        //[SerializeField] private float m_JumpForce = 400f;                  // Amount of force added when the player jumps.
        //[Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
        [SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
        [SerializeField] private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character
        [SerializeField] private SpriteRenderer m_PlayerSprite;
        private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
        const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
        private bool m_Grounded;            // Whether or not the player is grounded.
        private Transform m_CeilingCheck;   // A position marking where to check for ceilings
        const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
        private Animator m_Anim;            // Reference to the player's animator component.
        private Rigidbody2D m_Rigidbody2D;
        private bool m_FacingRight = true;  // For determining which way the player is currently facing.

        //PlayerMovement variables copied
        public float speed = 1f;
        public float jumpSpeed = 3f;
        public bool groundCheck;
        public bool canSwing, isSwinging;
        private SpriteRenderer playerSprite;
        //private Rigidbody2D m_Rigidbody2D;
        private bool isJumping;
        private Animator animator;
        private float jumpInput;
        private float horizontalInput;
        private float verticalInput;

        [SerializeField] ParticleSystem m_LandingParticles;


        public Vector2 ropeHook;
        public float swingForce = 4f;

        private void Awake()
        {
            // Setting up references.
            m_GroundCheck = transform.Find("GroundCheck");
            m_CeilingCheck = transform.Find("CeilingCheck");
            m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public float GetHorizontalInput()
        {
            return horizontalInput;
        }
        public float GetVerticalInput()
        {
            return verticalInput;
        }
        //void TurnAround(float _x)
        //{
        //    if (_x < 0)
        //    {
        //        transform.localScale.y *= -1f;
        //    }
        //    else if (_x > 0)
        //    {
        //        transform.eulerAngles = Vector3.zero;
        //    }
        //    else if(_x == 0)
        //    {
        //        return;
        //    }
        //}
        void FixedUpdate()
        {
            if (horizontalInput < 0f || horizontalInput > 0f) //Just made the movement code independent from horizontal input 
            {
                //if()
                //Temporarily disable animations
                //animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
                m_PlayerSprite.flipX = horizontalInput < 0f; //Should M_player sprite be same as child? Or is there another sprite different from sprite component
                
                //Instead of negative scaling, flip the sprite and shift in position offset 
            }


            //else
            //{
            //    if(groundCheck)
            //        m_Rigidbody2D.gravityScale = 0f;
            //    else
            //        m_Rigidbody2D.gravityScale = 15f;

            //}
            //animator.SetBool(" IsSwinging", false);
            if (groundCheck || m_AirControl)
            {
                //SEMI-WORKING ORIGINAL CODE ***
                //m_Rigidbody2D.velocity = new Vector2(horizontalInput * speed, m_Rigidbody2D.velocity.y); // this assumes I'm moving only on a flat surface. Behaves differently with slopes

                //******
                //var groundForce = speed * 2f;
                //m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x , m_Rigidbody2D.velocity.y);
                //m_Rigidbody2D.velocity += new Vector2(horizontalInput * speed, 0); // this assumes I'm moving only on a flat surface. Behaves differently with slopes

                //RaycastHit2D rayDown = Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
                    
                int layer_mask = LayerMask.GetMask("Environment");
                RaycastHit2D rayDown = Physics2D.Raycast(transform.position, Vector2.down, 2f, layer_mask);
                if (rayDown.collider != null)
                {
                    Debug.DrawLine(transform.position, rayDown.point);
                    Vector2 normalFound = rayDown.normal;
                    //Debug.Log("RayDown Point: " + rayDown.point + " RayDown Collider: " + rayDown.collider.gameObject);
                    //Rotated normalFound coordinates by 90 degrees, 
                    var rotatedNormal = new Vector2(-normalFound.y, normalFound.x); 

                    //transform.Rotate(normalFound, 90f);

                    Debug.Log("Normal rotated: " + rotatedNormal + " Magnitude: " + rotatedNormal.magnitude);
                    m_Rigidbody2D.linearVelocity += -rotatedNormal * horizontalInput * speed; /* new Vector2(normalFound.x * horizontalInput * speed, 0); */// this assumes I'm moving only on a flat surface. Behaves differently with slopes
                                                                                                                                                      // If the input is moving the player right and the player is facing left...
                    //if (m_Rigidbody2D.velocity.x > 0 && !m_FacingRight)
                    //{
                    //    // ... flip the player.
                    //    Flip();
                    //}
                    //// Otherwise if the input is moving the player left and the player is facing right...
                    //else if (m_Rigidbody2D.velocity.x < 0 && m_FacingRight)
                    //{
                    //    // ... flip the player.
                    //    Flip();
                    //}
                }
                m_Rigidbody2D.linearVelocity = new Vector2(horizontalInput * speed, m_Rigidbody2D.linearVelocity.y); 
                
                /* new Vector2(normalFound.x * horizontalInput * speed, 0); */// this assumes I'm moving only on a flat surface. Behaves differently with slopes
                //m_Rigidbody2D.velocity += new Vector2(normalFound.magnitude * horizontalInput * speed, 0); // this assumes I'm moving only on a flat surface. Behaves differently with slopes
                //m_Rigidbody2D.velocity += normalFound * horizontalInput * speed; /* new Vector2(normalFound.x * horizontalInput * speed, 0); */// this assumes I'm moving only on a flat surface. Behaves differently with slopes


                //normalFound.Rotate(new Vector3())
                //Air control = Need to control speed, drag and gravity

                //try increasing the linear drag or decreasing the velocity OR

                //try reworking Char Controller that stops assuming I work on the flat surface, know the slope angle you're on.
                //Raycast downward & read the collision's 'normal' vector var. Take the 90deg vector from that, 
                //rotate the normal vector detected by 90 degrees -- Find formula to rotate vector
                //newfound vector * horizontalInput * speed to get velocity 
                //
                //Make sure you add or multiply 
            }
            //}
            //else
            //{

            //    //animator.SetBool("IsSwinging", false);
            //    //animator.SetFloat("Speed", 0f);
            //}

            m_Rigidbody2D.linearVelocity = Vector2.ClampMagnitude(m_Rigidbody2D.linearVelocity, m_MaxSpeed);

        }
        void Update()
        {
            //jumpInput = Input.GetAxis("Jump");
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
            var halfHeight = m_PlayerSprite.bounds.extents.y;
            //Debug.Log("half height = " + halfHeight);
            //Debug.DrawLine(transform.position, new Vector3(transform.position.x, (transform.position.y - halfHeight/* - 0.04f*/) + 0.025f - transform.position.y), Color.green);
            groundCheck = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - halfHeight/* - 0.04f*/), Vector2.down, 0.0025f, 14); //14 is Environment Layer

        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Environment"))
            {
                m_LandingParticles.transform.position = collision.GetContact(0).point;
                m_LandingParticles.Play(); 
            }
        }

        // FUNCTION - Flip(): FLIPS THE OBJECT BY MULTIPLYING M_FACINGRIGHT & LOCAL SCALE'S X VALUE BY -1 
        private void Flip()
        {
            // -- Switch the way the player is labelled as facing.
            m_FacingRight = !m_FacingRight;

            // -- Multiply the player's x local scale by -1. Put local
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        } //Just avoid negative scale 
    }

    //private void FixedUpdate()
    //{
        //        m_Grounded = false;

        //        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        //        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        //        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        //        for (int i = 0; i < colliders.Length; i++)
        //        {
        //            if (colliders[i].gameObject != gameObject)
        //                m_Grounded = true;
        //        }
        //        m_Anim.SetBool("Ground", m_Grounded);

        //        // Set the vertical animation
        //        m_Anim.SetFloat("vSpeed", m_Rigidbody2D.velocity.y);
        //    }


        //    public void Move(float move, bool crouch, bool jump)
        //    {
        //        // If crouching, check to see if the character can stand up
        //        if (!crouch && m_Anim.GetBool("Crouch"))
        //        {
        //            // If the character has a ceiling preventing them from standing up, keep them crouching
        //            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
        //            {
        //                crouch = true;
        //            }
        //        }

        //        // Set whether or not the character is crouching in the animator
        //        m_Anim.SetBool("Crouch", crouch);

        //        //only control the player if grounded or airControl is turned on
        //        if (m_Grounded || m_AirControl)
        //        {
        //            // Reduce the speed if crouching by the crouchSpeed multiplier
        //            move = (crouch ? move*m_CrouchSpeed : move);

        //            // The Speed animator parameter is set to the absolute value of the horizontal input.
        //            m_Anim.SetFloat("Speed", Mathf.Abs(move));

        //            // Move the character
        //            m_Rigidbody2D.velocity = new Vector2(move*m_MaxSpeed, m_Rigidbody2D.velocity.y);

        //            // If the input is moving the player right and the player is facing left...
        //            if (move > 0 && !m_FacingRight)
        //            {
        //                // ... flip the player.
        //                Flip();
        //            }
        //                // Otherwise if the input is moving the player left and the player is facing right...
        //            else if (move < 0 && m_FacingRight)
        //            {
        //                // ... flip the player.
        //                Flip();
        //            }
        //        }
        //        // If the player should jump...
        //        if (m_Grounded && jump && m_Anim.GetBool("Ground"))
        //        {
        //            // Add a vertical force to the player.
        //            m_Grounded = false;
        //            m_Anim.SetBool("Ground", false);
        //            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        //        }
    //}
}
