using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

/// <summary>
/// Arma sincronizada en red. Es el ÚNICO componente de disparo en el Player
/// (PlayerShoot.cs ha sido retirado para evitar duplicidad).
///
/// Flujo:
///   1. El cliente local detecta el input "Attack" vía PlayerInput (SendMessages).
///   2. Calcula posición + rotación de salida y envía [Command] al Servidor.
///   3. El Servidor instancia la bala, le aplica velocidad, le asigna shooterNetId
///      y la spawnea en red (NetworkServer.Spawn).
///   4. El Servidor llama [ClientRpc] para que TODOS los clientes ejecuten
///      muzzle flash + sfx + animación de disparo (sincronización autoritativa).
///
/// Requiere en el Player:
///   - NetworkIdentity
///   - PlayerInput (Behavior = Send Messages) con acción "Attack".
///   - Animator (opcional, para el trigger de animación).
/// </summary>
public class NetworkWeapon : NetworkBehaviour
{
    [Header("Disparo de Proyectiles")]
    [Tooltip("Prefab de la bala. DEBE tener NetworkIdentity y estar en NetworkManager > Spawnable Prefabs.")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("Velocidad inicial de la bala (m/s).")]
    [SerializeField] private float bulletSpeed = 35f;

    [Tooltip("Cadencia mínima entre disparos (segundos).")]
    public float fireRate = 0.2f;

    [Header("Referencias")]
    [Tooltip("Punto 'Muzzle' donde nacerá el proyectil.")]
    [SerializeField] private Transform firePoint;

    [Tooltip("Prefab de partícula de fogonazo (opcional). NO requiere NetworkIdentity: es puramente visual.")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    [Tooltip("AudioSource para el sonido de disparo (opcional).")]
    [SerializeField] private AudioSource shotSfx;

    [Header("Apuntado")]
    [Tooltip("Si es true, la bala viaja hacia donde apunta la cámara del jugador local.")]
    public bool aimFromCamera = true;

    [Header("Animación")]
    [Tooltip("Nombre del Trigger en el Animator para la animación de disparo. Déjalo vacío para no usarlo.")]
    [SerializeField] private string shootTriggerName = "Shoot";

    private float  nextShotTime;
    private Camera cam;
    private Animator anim;

    // ────────────────────── Lifecycle ──────────────────────
    void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    public override void OnStartLocalPlayer()
    {
        cam = Camera.main;
    }

    // PlayerInput → "Send Messages" llama OnAttack en todos los componentes
    public void OnAttack(InputValue value)
    {
        if (!isLocalPlayer) return;          // Sólo el dueño dispara.
        if (!value.isPressed) return;
        if (Time.time < nextShotTime) return;
        if (firePoint == null) return;
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[NetworkWeapon] bulletPrefab no asignado.");
            return;
        }

        nextShotTime = Time.time + fireRate;

        Quaternion fireRotation = GetFireRotation();
        CmdFireProjectile(firePoint.position, fireRotation);
    }

    private Quaternion GetFireRotation()
    {
        if (aimFromCamera)
        {
            if (cam == null) cam = Camera.main;
            if (cam != null) return cam.transform.rotation;
        }
        return firePoint.rotation;
    }

    // ────────────────────── Servidor ──────────────────────
    [Command]
    void CmdFireProjectile(Vector3 position, Quaternion rotation)
    {
        if (bulletPrefab == null) return;

        // 1) Instanciar el proyectil en el Servidor.
        GameObject bullet = Instantiate(bulletPrefab, position, rotation);

        // 2) Asignar el shooter para evitar autodaño.
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.shooterNetId = netId;

        // 3) Aplicar velocidad ANTES del Spawn para que se replique correctamente.
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = rotation * Vector3.forward * bulletSpeed;
#else
            rb.velocity = rotation * Vector3.forward * bulletSpeed;
#endif
        }

        // 4) Spawn en red — los clientes verán la bala viajar.
        NetworkServer.Spawn(bullet);

        // 5) Notificar a todos los clientes para FX y animación sincronizados.
        RpcOnFireEffects(position, rotation);
    }

    // ────────────────────── Clientes ──────────────────────
    [ClientRpc]
    void RpcOnFireEffects(Vector3 muzzlePos, Quaternion muzzleRot)
    {
        // Muzzle flash (puramente visual, sin networking).
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePos, muzzleRot);
            Destroy(flash, 1f);
        }

        // Sonido de disparo.
        if (shotSfx != null) shotSfx.Play();

        // Animación de disparo: forzamos un Trigger en el Animator local de cada cliente.
        // Esto evita depender de NetworkAnimator y garantiza sincronización inmediata.
        if (anim != null && !string.IsNullOrEmpty(shootTriggerName))
        {
            // Sólo activar si el parámetro existe en el Animator (evita warnings).
            foreach (var p in anim.parameters)
            {
                if (p.name == shootTriggerName && p.type == AnimatorControllerParameterType.Trigger)
                {
                    anim.SetTrigger(shootTriggerName);
                    break;
                }
            }
        }
    }
}
