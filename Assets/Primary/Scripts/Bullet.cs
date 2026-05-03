using UnityEngine;
using Mirror;

/// <summary>
/// Bala de red autoritativa.
/// - El SERVIDOR es la única autoridad que aplica daño y destruye la bala.
/// - Se sincroniza visualmente vía NetworkTransform.
///
/// Requisitos en el Prefab Bullet:
///   - NetworkIdentity
///   - NetworkTransform (Sync Direction = Server To Client)
///   - Rigidbody (Use Gravity = false)
///   - Collider (sólido o trigger; este script soporta ambos)
///   - Registrado en NetworkManager > Spawnable Prefabs
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : NetworkBehaviour
{
    [Header("Configuración de daño")]
    [SerializeField] private int damage = 20;

    [Header("Efectos")]
    [SerializeField] private GameObject explosionPrefab;

    [Header("Límites de Vuelo")]
    [SerializeField] private float maxDistance = 20f;
    [Tooltip("Tiempo máximo de vida de la bala (s).")]
    [SerializeField] private float maxLifeTime = 4f;

    [HideInInspector] public uint shooterNetId;

    private Vector3 startPosition;
    private Rigidbody rb;
    private bool destroyed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    public override void OnStartServer()
    {
        startPosition = transform.position;
        // Garantizar destrucción aunque la bala no impacte nada
        Invoke(nameof(ServerDestroy), maxLifeTime);
    }

    [ServerCallback]
    void Update()
    {
        // Distancia euclidiana — si supera maxDistance, se destruye en el server
        if (!destroyed && Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            ServerDestroy();
        }
    }

    /// <summary>
    /// Procesa la colisión EXCLUSIVAMENTE en el servidor.
    /// </summary>
    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject,
                  collision.contactCount > 0 ? collision.contacts[0].point  : transform.position,
                  collision.contactCount > 0 ? collision.contacts[0].normal : -transform.forward);
    }

    /// <summary>
    /// Soporte para colliders configurados como Trigger.
    /// </summary>
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject, transform.position, -transform.forward);
    }

    [Server]
    private void HandleHit(GameObject hit, Vector3 contactPoint, Vector3 contactNormal)
    {
        if (destroyed || hit == null) return;

        // Ignorar otras balas (evita choques bala-bala)
        if (hit.GetComponent<Bullet>() != null) return;

        // Buscar PlayerHealth en el objeto o en jerarquía superior
        PlayerHealth health = hit.GetComponent<PlayerHealth>()
                              ?? hit.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            NetworkIdentity targetId = health.GetComponent<NetworkIdentity>();
            // Evitar fuego amigo: si el netId impactado es igual al del shooter, ignorar
            if (targetId != null && targetId.netId == shooterNetId) return;

            health.TakeDamage(damage);
        }

        RpcSpawnExplosion(contactPoint, Quaternion.LookRotation(contactNormal));
        ServerDestroy();
    }

    [ClientRpc]
    private void RpcSpawnExplosion(Vector3 pos, Quaternion rot)
    {
        if (explosionPrefab != null)
        {
            GameObject exp = Instantiate(explosionPrefab, pos, rot);
            Destroy(exp, 2f); // Limpieza local del efecto visual
        }
    }

    [Server]
    private void ServerDestroy()
    {
        if (destroyed) return;
        destroyed = true;
        CancelInvoke();

        if (gameObject != null)
            NetworkServer.Destroy(gameObject);
    }
}