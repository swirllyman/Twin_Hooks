using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;
using UnityEngine.SceneManagement;
using TMPro;
//using Unity.Transforms;

[RequireComponent(typeof(PlatformerCharacter2D_Alt))]
public class PlayerHealth : MonoBehaviour
{
    //public int health = 5; //NOT USED. The real one is in GameManager
    [SerializeField] bool isInvincible = false;
    [SerializeField] bool m_InvincibleCheat = false;
    public float invincibleTimeLimit = 1.0f, invincibleTimer = 0f;
    public TextMeshProUGUI m_healthText;
    public MyGameManager m_gm;
    PlatformerCharacter2D_Alt playerPlatformChar;
    [SerializeField] SpriteRenderer spriteRend;
    SpriteRenderer [] childSprites;
    [SerializeField] ParticleSystem m_DamageParticles;
    List<Color> childColors = new List<Color>();
    //Color[] childColors;
    [Header("Audio")]
    public AudioSource myAudio;
    public AudioClip damage;
    public AudioClip death;
    public AudioClip revive;

    GameManager gm;

    Color col;
    float alph;
    // Start is called before the first frame update

    private void Awake()
    {
        isInvincible = false;
        if(!myAudio)
            myAudio = GetComponent<AudioSource>();
        gm = GameObject.FindFirstObjectByType<GameManager>();
        col = spriteRend.color;
        var a = spriteRend.color.a;
        childSprites =  GetComponentsInChildren<SpriteRenderer>();
        for(int c = 0; c < childSprites.Length; ++c)
        {
            childColors.Add(childSprites[c].color);
            //childColors[c] = childSprites[c].color;
        }
        playerPlatformChar = GetComponent<PlatformerCharacter2D_Alt>();
    }

    void Start()
    {
        //gm.health = health;
        if (!m_InvincibleCheat)
        {
            MyEventsManager.onPlayerDamaged += TakeDamage;
            MyEventsManager.onPlayerDamaged += HealthCheck;
        }
    }

    public bool GetInvincibleState()
    {
        return isInvincible;
    }

    void TakeDamage()
    {
        if (isInvincible)
            return;
        BumpPlayer();
        //Damage FX - NOTE: if any values are null, the rest of the method doesn't work!
        //Comment or add values before running!
        
        //m_DamageParticles.Play();
        //myAudio.Stop();
        //myAudio.PlayOneShot(damage);
        Debug.Log("I'm hit! Changing Health");

        m_gm.ChangeMyHealth(-1);
        /*if (spriteRend)
        //{

        //    col.a = 0.2f;
        //    spriteRend.color = col;            
        //}*/
        ChangeColorOnAllSprites(true);
    }

    void TakeDamage(int _d)
    {
        if (isInvincible)
            return;
        BumpPlayer();
        myAudio.Stop();
        myAudio.PlayOneShot(damage); 
        MyGameManager.ChangeHealth(-_d);
        /*if (spriteRend)
        //{           
        //    col.a = 0.2f;
        //    spriteRend.color = col;
        //    for(int i = 0; i < childSprites.Length; ++i)
        //    {
        //        childSprites[i].color = col;
        //    }
        //}*/
        ChangeColorOnAllSprites(true);
    }

    void HealthCheck()
    {
        if (m_gm.GetMyHealth() > 0)
        {
            StartCoroutine("BecomeTemporarilyInvincible");
            return;
        }
        else
            //StartCoroutine("DeathAndRespawnProcess");
            DeathAndRespawn_Alt();
            //Start restart process
    }

    void BumpPlayer()
    {
        StartCoroutine("DisableInputTemporarily");
        GetComponent<Rigidbody2D>().AddForce(new Vector2((transform.right.x * -100f), 100f), ForceMode2D.Impulse); 
    }

    void KillPlayer()
    {
        //myAudio.Stop();
        //myAudio.PlayOneShot(death);
        if (GetComponent<PlatformerCharacter2D_Alt>() != null)
        {
            GetComponent<PlatformerCharacter2D_Alt>().enabled = false;
        }
        GetComponent<Rigidbody2D>().AddForce(new Vector2((playerPlatformChar.GetHorizontalInput() * -100f), 50f), ForceMode2D.Impulse);

    }

    void ChangeColorOnAllSprites(bool _makeTransparent)
    {
        if (spriteRend)
        {
            if (_makeTransparent)
            {
                col.a = 0.2f;
                spriteRend.color = col;
                for (int i = 0; i < childSprites.Length; ++i)
                {
                    childSprites[i].color = col;
                }
            }
            else
            {
                col.a = 1f;
                spriteRend.color = col;
                for (int i = 0; i < childSprites.Length; ++i)
                {
                    childSprites[i].color = childColors[i];
                }                
            }
            
        }
    }
    
    private IEnumerator BecomeTemporarilyInvincible()
    {
        Debug.Log("Player turned invincible!");
        ToggleInvincibility(true);
        //if (isInvincible)
        //    gameObject.layer = LayerMask.NameToLayer("EnemyPass");
        //else
        //    gameObject.layer = LayerMask.NameToLayer("Player");

        yield return new WaitForSeconds(invincibleTimeLimit);

        ToggleInvincibility(false);

        Debug.Log("Player is no longer invincible!");
    }

    private IEnumerator BecomeTemporarilyInvincibleAndFlash()
    {
        Debug.Log("Player turned invincible!");
        isInvincible = true;

        //for (float i = 0; i < invincibleTimeLimit; i += invincibilityDeltaTime)
        //{
        //    // Alternate between 0 and 1 scale to simulate flashing
        //    if (model.transform.localScale == Vector3.one)
        //    {
        //        ScaleModelTo(Vector3.zero);
        //    }
        //    else
        //    {
        //        ScaleModelTo(Vector3.one);
        //    }
        //    yield return new WaitForSeconds(invincibilityDeltaTime);
        //}

        //Debug.Log("Player is no longer invincible!");
        //ScaleModelTo(Vector3.one);
        //isInvincible = false;
        yield return null;
    }

    private IEnumerator DisableInputTemporarily()
    {
        if (GetComponent<PlatformerCharacter2D_Alt>() != null)
        {
            GetComponent<PlatformerCharacter2D_Alt>().enabled = false;
        }
        yield return new WaitForSeconds(0.2f);
        GetComponent<PlatformerCharacter2D_Alt>().enabled = true;

        yield return null;
    }
    //private void ScaleModelTo(Vector3 scale)
    //{
    //    model.transform.localScale = scale;
    //}

    IEnumerator DeathAndRespawn()
    {
        KillPlayer();
        yield return new WaitForSeconds(2.0f);
        MyGameManager.RestartScene();
        yield return null;
    }
    void DeathAndRespawn_Alt()
    {
        KillPlayer();
        Invoke("CallRestart", 1f);
    }
    void ToggleInvincibility(bool _b)
    {
        isInvincible = _b;
        if (isInvincible)
            gameObject.layer = LayerMask.NameToLayer("EnemyPass");
        else
            gameObject.layer = LayerMask.NameToLayer("Player");

        ChangeColorOnAllSprites(_b);
    }

    void CallRestart()
    {
        MyGameManager.RestartScene();
    }
    // Update is called once per frame
    void Update()
    {
        //if (isInvincible)
        //{            
        //    invincibleTimer -= Time.deltaTime;
        //    if (invincibleTimer <= 0)
        //    {
        //        //col.a = 1f;
        //        //spriteRend.color = col;
        //        invincibleTimer = invincibleTimeLimit;
        //        ToggleInvincibility(false);
        //    }
        //}
    }
}
