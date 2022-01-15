using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour, Hitable
{
    public GameObject particleEffect;
    public Transform contactPoint;
    public bool tracking;
    public float timeMade;
    private Rigidbody2D myRigidBody2D;
    private int groundLayer;
    void Start()
    {
        groundLayer = LayerMask.NameToLayer("ground");
        myRigidBody2D = GetComponent<Rigidbody2D>();
        timeMade = Time.time;
    }

    void Update()
    {
        if (tracking)
        {
            Vector2 direction = LevelGlobals.player.transform.position - transform.position;
            myRigidBody2D.MovePosition(myRigidBody2D.position + direction.normalized * Time.fixedDeltaTime * 5f);
            transform.right = direction.normalized;
        }
    }

    public void onHit(bool reflect)
    {
        tracking = false;
        TriggerParticleEffect();
        if (!reflect)
        {
            Destroy(gameObject);
            return;
        }
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 worldPos2D = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
        Vector3 direction = (worldPos2D - transform.position).normalized;
        GetComponent<Rigidbody2D>().velocity = direction * 10;
        transform.right = worldPos2D - transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == groundLayer)
        {
            TriggerParticleEffect();
            Destroy(gameObject);
        }
    }

    private void TriggerParticleEffect()
    {
        GameObject effect = Instantiate(particleEffect);
        effect.transform.up = -GetComponent<Rigidbody2D>().velocity;
        effect.transform.position = contactPoint.position;
        Destroy(effect, 1);
    }
}
