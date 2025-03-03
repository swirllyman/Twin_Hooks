using System.Collections;
using System.Collections.Generic;
//using Unity.Transforms;
using UnityEngine;

public class GrappleDestroyer : MonoBehaviour
{
    AudioSource myAudio;
    public EnemyState parentEnemyState;
    EnemyController parentEnemy;
    private void Awake()
    {
        myAudio = GetComponent<AudioSource>();
        if (!parentEnemy)
        {
            parentEnemy = transform.parent.gameObject.GetComponent<EnemyController>();
            parentEnemyState = parentEnemy.GetMyState();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && parentEnemy)
        {
            parentEnemyState = parentEnemy.GetMyState();
            if ((parentEnemyState == EnemyState.Normal || 
                 parentEnemyState == EnemyState.Grappled) &&
                 parentEnemy.hurtsPlayer && 
                 collision.gameObject.GetComponent<PlayerHealth>().GetInvincibleState() == false)
            { MyEventsManager.OnPlayerDamaged(); }
        }
    }
}
