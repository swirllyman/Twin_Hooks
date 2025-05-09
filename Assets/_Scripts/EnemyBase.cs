using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//EnemyBase the base Grabbable class for all enemies. Needs this to be able to be picked up and thrown
public class EnemyBase : Grabbable
{
    bool pickedUp = false;
    bool damageOnContact = false;


    public delegate void EnemyPickedUp(bool _b);
    public event EnemyPickedUp onEnemyPickedUp;

    PlayerLevelStats p;
    //public event EventHandler EnemyPickedUp;
    
    BasicEnemyMovement basicEnemyMovement;
    
    
    public override void PickUp()
    {
        base.PickUp();
        pickedUp = true;

        LeanTween.cancel(myRend.gameObject);
        LeanTween.color(myRend.gameObject, startColor, 0.0f);
        audioSource.Play();
    }

    private void Update()
    {
        if (pickedUp)
        {
            
        }
        //StartCoroutine(RemoveAfterTime());
    }

    private void Awake()
    {
        
    }

    IEnumerator RemoveAfterTime()
    {
        yield return new WaitForSeconds(.2f);
        myRend.enabled = false;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(myBody.position, 1f);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            //if (hitColliders[i].CompareTag("FakeWall"))
            //{
            //    hitColliders[i].GetComponent<HiddenWalls>().ExplodeWall();
            //}
        }
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.GetComponent<PlayerHealth>())
        {
            var ph = collision.gameObject.GetComponent<PlayerHealth>();
            MyEventsManager.OnPlayerDamaged();
        }
    }
}
