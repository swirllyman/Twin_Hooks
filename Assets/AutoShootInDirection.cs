using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoShootInDirection : MonoBehaviour
{
    public float shotSpeed = 10f, fireRateEvery = 1.0f, bulletDmg = 10;
    float fireTime;
    public GameObject bulletPrefab;
    public Transform aimPoint;

    [SerializeField] AudioClip shootSound;
    AudioSource myAudio;
    // Start is called before the first frame update
    void Start()
    {
        myAudio = GetComponentInParent<AudioSource>();
    }

    void ShootBullet()
    {
        if (bulletPrefab != null)
        {
            myAudio.Stop();
            myAudio.PlayOneShot(shootSound);
            GameObject newBullet = GameObject.Instantiate(bulletPrefab, aimPoint.position, aimPoint.rotation);
            newBullet.GetComponent<Rigidbody2D>().AddForce(transform.right * shotSpeed);
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
