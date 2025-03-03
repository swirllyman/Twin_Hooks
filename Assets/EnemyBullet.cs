using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
public class EnemyBullet : MonoBehaviour
{
    public int damageToPlayer = 1;
    public bool doesDamage = true,
                hurtsPlayer = true,
                hurtsEnemies = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Damage and toggle invincible when hit by object with PhysicsDamageHandler
        if (hurtsPlayer)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                MyEventsManager.OnPlayerDamaged();
                doesDamage = false;
            }
            GetComponent<Rigidbody2D>().gravityScale = 10;
        }
        else if(hurtsEnemies && collision.gameObject.CompareTag("Enemy"))
        {

        }


        //gameObject.AddComponent<TimedObjectDestructor>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
}
