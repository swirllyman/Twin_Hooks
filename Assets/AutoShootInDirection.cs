using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoShootInDirection : MonoBehaviour
{
    public float shotSpeed = 10f, fireRateEvery = 1.0f, bulletDmg = 10;
    float fireTime;
    public bool isShooting = true;
    public bool aimAtTarget = false;
    public GameObject bulletPrefab;
    public Transform aimPoint;
    public Transform target;


    [SerializeField] AudioClip shootSound;
    AudioSource myAudio;

    AutoMoveSimple enemyMovementScript; // Reference to the enemy movement script

    // Start is called before the first frame update
    void Start()
    {
        myAudio = GetComponentInParent<AudioSource>();
        if(enemyMovementScript == null)
        {
            enemyMovementScript = GetComponentInParent<AutoMoveSimple>();
        }
        //Check if the enemyMovementScript is not null before subscribing to the event
        if (enemyMovementScript != null && aimAtTarget == false)
        {
            enemyMovementScript.onAutoMoveDirChanged += ChangeGunXDirection;
        }
    }

    void ShootBullet()
    {
        if (bulletPrefab == null)
        {
            return;
        }
        else if (aimAtTarget && target != null)
        {
            Vector2 direction = target.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        GameObject newBullet = GameObject.Instantiate(bulletPrefab, aimPoint.position, transform.rotation);
        newBullet.GetComponent<Rigidbody2D>().AddForce(transform.right * shotSpeed);
        myAudio.Stop();
        myAudio.PlayOneShot(shootSound);

    }

    void ChangeGunXDirection()
    {
        if (enemyMovementScript != null)
        {
            Vector2 direction = enemyMovementScript.GetMoveDirection();
            if (direction.x > 0)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (direction.x < 0)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (fireTime < fireRateEvery)
            fireTime += Time.deltaTime;
        else
        {
            ShootBullet();
            fireTime = 0;
        }
    }
}
