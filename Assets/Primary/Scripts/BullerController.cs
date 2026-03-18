using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 3f;
    public GameObject explosionPrefab; 
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.linearVelocity = transform.forward * speed;
        
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
       
            if (!other.CompareTag("Player") && !other.CompareTag("Weapon"))
            {

                ObjectHealth health = other.GetComponent<ObjectHealth>();

                if (health != null)
                {
                    health.TakeDamage(10f); 
                }

                if (explosionPrefab != null)
                {
                    GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                    Destroy(exp, 2f);
                }

                Destroy(gameObject); 
            }
    }
}