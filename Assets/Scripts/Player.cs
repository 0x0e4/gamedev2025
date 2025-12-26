using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float hp = 100f;
    public float maxHp = 100f;
    public Weapon currentWeapon;

    private UI ui;

    private float healTimer;

    [SerializeField]
    private AudioClip[] hittedSounds;

    [SerializeField]
    private AudioClip deadeyeSound, deadeyeStopSound;

    [SerializeField]
    private AudioSource audioSource, audioSourceDeadeye;

    private bool slowTimeMode;
    private float slowTimeCount = 4.0f;
    [SerializeField]
    private readonly float slowTimeMultiplierHead = 2.5f;
    [SerializeField]
    private readonly float slowTimeMultiplierCount = 1.5f;
    [SerializeField]
    private readonly int slowTimeMultiplierCountLimit = 8;
    [SerializeField]
    private readonly float slowTimeMultiplierCountInterval = 0.7f;
    [SerializeField]
    private readonly float slowTimeMultiplierCountSave = 1.2f;

    private float lastHit = 0.0f;
    private int hitCount;

    void Start()
    {
        ui = FindObjectOfType<UI>();
    }

    void SlowTimeModeEnable()
    {
        slowTimeMode = true;
        Time.timeScale = 0.2f;
        audioSourceDeadeye.clip = deadeyeSound;
        audioSourceDeadeye.Play();
    }

    void SlowTimeModeDisable()
    {
        slowTimeMode = false;
        Time.timeScale = 1f;
        audioSourceDeadeye.Stop();
        audioSourceDeadeye.clip = deadeyeStopSound;
        audioSourceDeadeye.Play();
    }

    public void AddSlowTime(bool head)
    {
        if((hitCount > 0 && lastHit < slowTimeMultiplierCountInterval && hitCount < slowTimeMultiplierCountLimit) || hitCount == 0f) hitCount++;
        slowTimeCount += 0.15f * (head ? slowTimeMultiplierHead : 1f) * (hitCount * slowTimeMultiplierCount);
    }
    
    void Update()
    {
        if(healTimer > 0f) healTimer -= Time.deltaTime;
        if(slowTimeMode)  { slowTimeCount -= Time.unscaledDeltaTime; if(slowTimeCount <= 0f) SlowTimeModeDisable(); }
        if(slowTimeCount < 0f) slowTimeCount = 0.0f;
        if(lastHit < slowTimeMultiplierCountSave) { lastHit += Time.unscaledDeltaTime; hitCount = 0; }
        ui.SetHP((int)hp);
        ui.SetSlowTimeCount(slowTimeCount);

        if(hp < maxHp && healTimer <= 0f) hp += Time.deltaTime * 6f;

        if(Input.GetMouseButtonDown(2))
        {
            if(slowTimeMode)
            {
                SlowTimeModeDisable();
            }
            else if (slowTimeCount > 2.0f)
            {
                SlowTimeModeEnable();
            }
        }
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        healTimer = 2.0f;

        audioSource.PlayOneShot(hittedSounds[UnityEngine.Random.Range(0, hittedSounds.Length)]);

        if(hp <= 0f)
            GameManager.Restart();
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Exit" && !FindObjectOfType<GameManager>().changingLevel && FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length == 0)
        {
            FindObjectOfType<GameManager>().NextLevel();
        }
        else if(FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length > 0)
            FindObjectOfType<UI>().escapeFailed.enabled = true;
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Exit")
            FindObjectOfType<UI>().escapeFailed.enabled = false;
    }
}
