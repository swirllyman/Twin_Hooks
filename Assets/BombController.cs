using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{

    public BombExplodeHandler explosion;
    Collider2D explosionCollider;
    public float fuseTime;
    float time;
    bool isCountingDown = true, exploding = false;
    // Start is called before the first frame update
    void Start()
    {
        time = fuseTime;
    }


    //Call StartCountdown when bomb is picked up or slammed by player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    void StartCountdown()
    {
        isCountingDown = true;
    }

    void Explode()
    {
        explosion.gameObject.SetActive(true);
        Destroy(gameObject, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if(isCountingDown && !exploding)
        {
            if (time > 0)
                time -= Time.deltaTime;
            else
            {
                exploding = true;
                Explode();
            }
        }
    }
}
