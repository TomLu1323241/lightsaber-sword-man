using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerList : MonoBehaviour
{
    //The list of colliders currently inside the trigger
    [HideInInspector]
    public HashSet<Collider2D> hitList = new HashSet<Collider2D>();
    private int projectileLayer;
    private int enemyLayer;

    private void Start()
    {
        projectileLayer = LayerMask.NameToLayer("projectile");
        enemyLayer = LayerMask.NameToLayer("enemy");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == projectileLayer || collision.gameObject.layer == enemyLayer) {
            hitList.Add(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        hitList.Remove(collision);
    }
}
