using UnityEngine;
using UnityEngine.InputSystem; 

public class WeaponController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform shootSpawn;   
    public GameObject bulletPrefab;
    public AudioSource shootAudio; 
    public Animator weaponAnimator; 

    [Header("Configuración")]
    public float fireRate = 0.25f;
    private float nextFireTime;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextFireTime)
        {
            InstantiateBullet();
            nextFireTime = Time.time + fireRate;
        }
    }

    public void InstantiateBullet()
    {
        if (bulletPrefab == null) return;
 
        if (shootAudio != null) shootAudio.Play();

        if (weaponAnimator != null) weaponAnimator.SetTrigger("Shoot");

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint = Physics.Raycast(ray, out hit) ? hit.point : ray.GetPoint(100);
        Vector3 direction = (targetPoint - shootSpawn.position).normalized;

        Instantiate(bulletPrefab, shootSpawn.position, Quaternion.LookRotation(direction));
    }
}