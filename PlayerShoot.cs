using UnityEngine;
using Mirror;

/// <summary>
/// Maneja el disparo del jugador en red.
/// - El Cliente con autoridad (LocalPlayer) detecta el input.
/// - Llama a [Command] para que el SERVIDOR instancie la bala.
/// - El Servidor llama a [ClientRpc] para sincronizar la animación en TODAS las instancias.
///
/// Requisitos en el GameObject del Player:
///   - NetworkIdentity (Local Player Authority activado)
///   - Animator
///   - NetworkAnimator (opcional si se usan parámetros automáticos; aquí usamos RPC manual por eficiencia)
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(Animator))]
public class PlayerShoot : NetworkBehaviour
{
    [Header("Referencias")]
    [Tooltip("Prefab de la bala. DEBE tener NetworkIdentity y estar registrado en NetworkManager > Spawnable Prefabs.")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("Punto de origen del disparo (ej. la punta del arma).")]
    [SerializeField] private Transform firePoint;

    [Header("Parámetros de disparo")]
    [SerializeField] private float bulletSpeed = 25f;
    [SerializeField] private float fireCooldown = 0.25f;

    [Header("Animación")]
    [Tooltip("Nombre del Trigger en el Animator que dispara la animación de disparo.")]
    [SerializeField] private string shootTriggerName = "Shoot";

    private Animator animator;
    private float nextFireTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Solo el jugador local controla su propio personaje.
        if (!isLocalPlayer) return;

        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            CmdShoot(firePoint.position, firePoint.rotation);
        }
    }

    // ---------- SERVIDOR ----------

    /// <summary>
    /// Se ejecuta en el Servidor a petición del cliente con autoridad.
    /// Aquí instanciamos la bala (autoridad de servidor) y avisamos a todos para animar.
    /// </summary>
    [Command]
    private void CmdShoot(Vector3 spawnPos, Quaternion spawnRot)
    {
        // 1) Instanciar la bala en el Servidor.
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, spawnRot);

        // 2) Asignar dueño/origen para evitar daño propio (lo veremos en Bullet.cs).
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.shooterNetId = netId;
        }

        // 3) Aplicar velocidad ANTES del Spawn para que se sincronice bien.
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = spawnRot * Vector3.forward * bulletSpeed;
            // En versiones antiguas de Unity usa: rb.velocity = ...
        }

        // 4) Spawn en red para que TODOS los clientes la vean.
        NetworkServer.Spawn(bullet);

        // 5) Disparar animación en todos los clientes.
        RpcPlayShootAnim();
    }

    // ---------- CLIENTES ----------

    /// <summary>
    /// Se ejecuta en TODOS los clientes (incluyendo Host) para reproducir la animación.
    /// Usar SetTrigger es más eficiente que sincronizar floats por NetworkAnimator.
    /// </summary>
    [ClientRpc]
    private void RpcPlayShootAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(shootTriggerName);
        }
    }
}
