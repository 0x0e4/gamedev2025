using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private AudioClip shotSound;

    [SerializeField]
    private AudioClip reloadSound;

    [SerializeField]
    public float fireRate = 0.2f, damage = 20f, reloadTime = 1f;
    public int maxClips = 7, currentClips = 0, ammoCount = 60;
    [SerializeField]
    private Transform muzzle;

    [SerializeField]
    private bool isReloading, isPlayers, isActive;
    private float fireTimer, reloadTimer;

    [SerializeField]
    private AudioSource audioSourceShot, audioSourceReload;


    // Start is called before the first frame update
    void Start()
    {
        if(isPlayers && isActive)
            FindObjectOfType<GameManager>().ui.SetAmmo(currentClips, ammoCount);

        if(ammoCount >= maxClips)
        {
            currentClips = maxClips;
            ammoCount -= maxClips;
        }
    }

    public void TryShoot(Vector3 point)
    {
        if(currentClips > 0)
            Shoot(point);
        else if(ammoCount > 0)
            TryReload();
    }

    void Shoot(Vector3 point)
    {
        if (currentClips == 0 || fireTimer > 0f) return;

        audioSourceShot.PlayOneShot(shotSound);

        fireTimer = fireRate;
        currentClips--;
        if(isPlayers && isActive)
            FindObjectOfType<GameManager>().ui.SetAmmo(currentClips, ammoCount);

        if (Physics.Raycast(muzzle.position, point - muzzle.position, out RaycastHit hit, 200f, ~LayerMask.GetMask("Room")))
        {
            if(hit.transform.root.tag == transform.root.tag) return;

            Transform root = hit.transform.root;
            Debug.DrawLine(muzzle.position, hit.point, Color.red, 10f);
            if (root.gameObject.tag == "Enemy")
            {
                transform.root.GetComponent<Player>().AddSlowTime(hit.collider.tag == "Head");
                root.GetComponent<Enemy>().OnAlert(transform.position);
                root.GetComponent<Enemy>().TakeDamage(hit.collider.tag == "Head" ? damage * 6f : damage);
            }
            else if (root.gameObject.tag == "Player")
                root.GetComponent<Player>().TakeDamage(damage);
        }
    }

    void TryReload()
    {
        if (!isReloading && ammoCount > 0 && currentClips < maxClips)
        {
            audioSourceReload.Play();
            isReloading = true;
        }
    }

    void FixedUpdate()
    {
        audioSourceReload.pitch = Time.timeScale == 1f ? 1f : 0.6f;
        audioSourceShot.pitch = Time.timeScale == 1f ? 1f : 0.8f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isReloading)
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer >= reloadTime)
            {
                int ammo = Math.Min(ammoCount, maxClips);
                currentClips = ammo;
                ammoCount -= ammo;
                isReloading = false;
                reloadTimer = 0f;
                fireTimer = fireRate;
                if(isPlayers && isActive)
                    FindObjectOfType<GameManager>().ui.SetAmmo(currentClips, ammoCount);
            }
            return;
        }

        if (isPlayers && isActive)
        {
            if (Input.GetKeyDown(KeyCode.R) && ammoCount > 0)
                TryReload();

            if (currentClips > 0 && fireTimer <= 0f)
            {
                if (Input.GetMouseButtonDown(0) || (fireRate <= 0.2f && Input.GetMouseButton(0)))
                {
                    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward,
                        out RaycastHit hit, 200f, ~LayerMask.GetMask("Room") & ~LayerMask.GetMask("Body")))
                        Shoot(hit.point);
                    else
                        Shoot(Camera.main.transform.forward);

                    Enemy.AlertNearbyEnemies(transform.position);
                }
            }
        }

        if (fireTimer > 0f)
            fireTimer -= Time.deltaTime;
    }
}
