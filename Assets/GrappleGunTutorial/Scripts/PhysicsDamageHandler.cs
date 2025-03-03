using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]

public class PhysicsDamageHandler : MonoBehaviour
{
    public bool doesDamage = false,
                breakable = false,
                isThrown = false;
    //Enum to describe phys obj size TBA
    public int damageValue = 1;
    public Vector2 minClampedVelocity = new Vector2(-100, -100),
                   maxClampedVelocity = new Vector2(100, 100);
    public float maxClampedLength = 70f;
    Rigidbody2D myRB;
    [SerializeField] ParticleSystem m_DamageParticles;
    void Start()
    {
        myRB = GetComponent<Rigidbody2D>();
        doesDamage = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (myRB.linearVelocity.magnitude >= 1f)
            doesDamage = true;
        else
            doesDamage = false;
        myRB.linearVelocity = Vector2.ClampMagnitude(myRB.linearVelocity, maxClampedLength);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (isThrown)
        {
            if (m_DamageParticles && !m_DamageParticles.isPlaying)
                m_DamageParticles.Play(); 
            
            if (breakable)
                GameObject.Destroy(gameObject, 0.5f);
        }
        isThrown = false;
    }
    

    //void ClampVelocity(Vector2 _myVel, Vector2 _minVel, Vector2 _maxVel)
    //{
    //    rigidbody2D.velocity = Vector2.ClampMagnitude(rigidbody2D.velocity.magnitude, maxVelocity);
    //    _myVel = Vector2.c
    //    if(_myVel.magnitude < _minVel.magnitude)
    //    {
    //        _myVel = _minVel;

    //    }
    //    if (_myVel.magnitude < _maxVel.magnitude)
    //    {
    //        _myVel = _maxVel;
    //    }
    //}

}
