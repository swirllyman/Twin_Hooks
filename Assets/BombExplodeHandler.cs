using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombExplodeHandler : MonoBehaviour
{
    public float explosionForce = 4, explodeMultiplier = 5, maxDistance;
    Collider2D cols;
    // Start is called before the first frame update
    void OnEnable()
    {
        //StartCoroutine("BlastRBs");
        //maxDistance = GetComponent<CircleCollider2D>().radius;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, maxDistance);
    }

    void ExplosionForce(List<Collider2D> players)
    {
        foreach (Collider2D hit in players)
        {
            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            float x = maxDistance - (rb.position.x - transform.position.x);
            float y = maxDistance - (rb.position.y - transform.position.y);
            if (x > maxDistance)
            {
                x = -maxDistance - (rb.position.x - transform.position.x);
            }

            if (y > maxDistance)
            {
                y = -maxDistance - (rb.position.y - transform.position.y);
            }

            rb.AddForce(new Vector2(x, y) / 2, ForceMode2D.Impulse);
        }
    }



    IEnumerator BlastRBs()
    {
        // wait one frame because some explosions instantiate debris which should then
        // be pushed by physics force
        yield return null;

        float multiplier = explodeMultiplier;

        float r = 10 * multiplier;
        cols = Physics2D.OverlapCircle(transform.position, r);
        var rigidbodies = new List<Rigidbody>();
        //foreach (var col in cols)
        //{
        //    if (col.attachedRigidbody != null && !rigidbodies.Contains(col.attachedRigidbody) &&
        //       (col.gameObject.CompareTag("Blastable") || col.gameObject.CompareTag("Breakable") || col.gameObject.layer == LayerMask.NameToLayer("Enemy")
        //       && col.gameObject.transform != transform.parent))
        //    {
        //        Debug.Log("Adding RB in " + col.gameObject.name);
        //        rigidbodies.Add(col.attachedRigidbody);
        //    }
        //}
        foreach (var rb in rigidbodies)
        {
            if (rb.isKinematic)
                rb.isKinematic = false;
            rb.AddExplosionForce(explosionForce * multiplier, transform.position, r, 1 * multiplier, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Breakable") ||
            collision.gameObject.CompareTag("Blastable") || collision.gameObject.CompareTag("PhysObj") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            var co = collision.gameObject;
            //var rb = collision.attachedRigidbody;
            if (collision.attachedRigidbody.isKinematic)
                collision.attachedRigidbody.isKinematic = false;
            //float r = 10 * explodeMultiplier;

            //rb.AddForceAtPosition(, collision.transform.position);
            //ExplosionForce()
            Rigidbody2DExtension.AddExplosionForce(collision.attachedRigidbody, explosionForce, transform.position/*new Vector3(collision.ClosestPoint(transform.position).x, collision.ClosestPoint(transform.position).y, 0)*/, maxDistance/*, explosionForce * 1.5f*/);
            co.AddComponent<UnityStandardAssets.Utility.TimedObjectDestructor>();
        }
        else if (collision.gameObject.transform == transform.parent)
        {
            //transform.parent.parent.DetachChildren();
            Destroy(collision.gameObject);
        }
    }

}
