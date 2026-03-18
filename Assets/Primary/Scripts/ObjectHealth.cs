using UnityEngine;
using UnityEngine.UI;

public class ObjectHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI Reference")]
    public Slider healthSlider;

    [Header("Efectos")]
    public AudioClip destructionSound; 
    public GameObject destructionEffect; 

    void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        healthSlider.value = currentHealth;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
     
        if (destructionSound != null)
        {
            AudioSource.PlayClipAtPoint(destructionSound, transform.position);
        }

      
        if (destructionEffect != null)
        {
            GameObject exp = Instantiate(destructionEffect, transform.position, transform.rotation);
            Destroy(exp, 4f); 
        }

        Destroy(gameObject);
    }
}