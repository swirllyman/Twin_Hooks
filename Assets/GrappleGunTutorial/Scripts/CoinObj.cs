using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class CoinObj : MonoBehaviour
{
    //ScoreIntEvent scoreIntEvent;
    MyGameManager gameManager;
    public int scoreValue = 10;
    public AudioClip a_coinSound;
    public GameObject soundObjectPrefab;
    SpriteRenderer m_MySprite;
    public ParticleSystem m_MyParticles;
    // Start is called before the first frame update
    void Start()
    {
        //scoreIntEvent = GameObject.FindWithTag("GameController").GetComponent<ScoreIntEvent>();
        if(!gameManager)
            gameManager = GameObject.FindWithTag("GameController").GetComponent<MyGameManager>();
        m_MySprite = GetComponent<SpriteRenderer>();
        gameManager.coinsInLevel += 1;
        //MyEventsManager.onCoin += PlaySoundAndDisable;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CollectAndDestroy();
        }
    }

    void PlaySoundAndDisable()
    {
        var aSource = gameObject.AddComponent<AudioSource>();
        aSource.volume = 0.2f;
        aSource.PlayOneShot(a_coinSound);
        m_MySprite.enabled = false;
        Invoke("DisableObject", a_coinSound.length);
    }

    void DisableObject()
    {
        gameObject.SetActive(false);
    }

    public void CollectAndDestroy()
    {
        MyEventsManager.CoinCollect();
        //Play collect particles
        if (m_MyParticles)
            m_MyParticles.Play();
        PlaySoundAndDisable();
        //var soundObj = GameObject.Instantiate(soundObjectPrefab, transform.position, transform.rotation);
        //soundObj.GetComponent<AudioPlayAndDestroy>().InitializeAudioSource(a_coinSound, 0.8f, 1);
        //gameManager.AddScore(scoreValue);
        //gameManager.coins += 1;
        //Destroy(gameObject, a_coinSound.length);
        //scoreIntEvent.Invoke(scoreValue);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
