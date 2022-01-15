using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleShooter : MonoBehaviour, Hitable
{
    public float detectionRad = 10;
    public GameObject bulletSpawn;
    public float bulletSpeed = 10;
    public GameObject projectile;
    public bool tracking;
    public bool minigun;
    public bool shotgun;

    private int playerLayer;
    private int groundLayer;
    private bool alerted = false;
    private SpriteRenderer spriteRenderer;
    private int projectileLayer;
    Coroutine shooting;

    void Start()
    {
        playerLayer = LayerMask.NameToLayer("player");
        groundLayer = LayerMask.NameToLayer("ground");
        projectileLayer = LayerMask.NameToLayer("projectile");
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (alerted)
        {
            flipSprite();
        } else
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, 
                LevelGlobals.player.transform.position - transform.position, 
                detectionRad, 
                1 << playerLayer | 1 << groundLayer);
            if (hit && hit.collider.gameObject == LevelGlobals.player)
            {
                alerted = true;
                shooting = StartCoroutine(shoot());
            }
        }
    }

    IEnumerator shoot()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.25f);
            int repeat = minigun ? 5 : 1;
            for (int i = 0; i < repeat; i++)
            {
                if (shotgun)
                {
                    for (int j = -2; j < 3; j++)
                    {
                        Vector3 direction = LevelGlobals.player.transform.position - bulletSpawn.transform.position;
                        direction = Quaternion.AngleAxis(3 * j, Vector3.forward) * direction;
                        GameObject bullet = Instantiate(projectile);
                        bullet.transform.position = bulletSpawn.transform.position;
                        bullet.GetComponent<Rigidbody2D>().velocity = direction.normalized * bulletSpeed;
                        bullet.transform.right = direction.normalized;
                        bullet.GetComponent<bullet>().tracking = tracking;
                        Destroy(bullet.gameObject, detectionRad / bulletSpeed);
                    }
                } else
                {
                    Vector3 direction = LevelGlobals.player.transform.position - bulletSpawn.transform.position;
                    GameObject bullet = Instantiate(projectile);
                    bullet.transform.position = bulletSpawn.transform.position;
                    bullet.GetComponent<Rigidbody2D>().velocity = direction.normalized * bulletSpeed;
                    bullet.transform.right = direction.normalized;
                    bullet.GetComponent<bullet>().tracking = tracking;
                    yield return new WaitForSeconds(0.1f);
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    private void flipSprite()
    {
        Vector3 direction = LevelGlobals.player.transform.position - transform.position;
        if (direction.x < 0)
        {
            spriteRenderer.flipX = true;
            bulletSpawn.transform.localPosition = new Vector3(-Mathf.Abs(bulletSpawn.transform.localPosition.x), 
                bulletSpawn.transform.localPosition.y, 
                bulletSpawn.transform.localPosition.z);
        } else
        {
            spriteRenderer.flipX = false;
            bulletSpawn.transform.localPosition = new Vector3(Mathf.Abs(bulletSpawn.transform.localPosition.x),
                bulletSpawn.transform.localPosition.y,
                bulletSpawn.transform.localPosition.z);
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.back, detectionRad);
#endif
    }

    public void onHit(bool reflect)
    {
        StopCoroutine(shooting);
        Destroy(gameObject);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == projectileLayer)
        {
            bullet bul = collision.gameObject.GetComponent<bullet>();
            if (bul != null && Time.time - bul.timeMade > 0.1f)
            {
                bul.onHit(false);
                Destroy(gameObject);
            }
        }
    }
}
