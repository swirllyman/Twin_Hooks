using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : Grabbable
{
    bool pickedUp = false;
    bool damageOnContact = false;

    public delegate void PickUpCallback();
    public event PickUpCallback onPickup;

    BasicEnemyMovement basicEnemyMovement;
    public override void PickUp()
    {
        base.PickUp();
        pickedUp = true;
        onPickup?.Invoke();
        LeanTween.cancel(myRend.gameObject);
        LeanTween.color(myRend.gameObject, startColor, 0.0f);
        audioSource.Play();
    }

    private void Update()
    {
        if (pickedUp)
        {
            
        }
        StartCoroutine(RemoveAfterTime());

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

}
