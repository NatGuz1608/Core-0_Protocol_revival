using UnityEngine;
using Mirror;

/// <summary>
/// Bala en red.
/// - Existe SOLO con autoridad de servidor (la mueve el Rigidbody y se replica con NetworkTransform).
/// - El daño se aplica en el SERVIDOR (autoridad de juego) y se sincroniza vía SyncVar en PlayerHealth.
///
/// Requisitos en el Prefab Bullet:
///   - NetworkIdentity
///   - NetworkTransform (Sync Direction = Server To Client)
///   - Rigidbody (Use Gravity = false; Is Kinematic = false)
///   - Collider (Is Trigger = true)  <-- importante para OnTriggerEnter
///   - Tag o Layer adecuados para no chocar con el propio shooter ni con triggers innecesarios
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : NetworkBehaviour
{
    [Header("Configuración de daño")]
    [SerializeField] private int damage = 20;

    [Header("Tiempo de vida")]
    [Tooltip("Tiempo (s) tras el cual la bala se destruye si no impactó nada.")]
    [SerializeField] private float lifeTime = 4f;

    /// <summary>
    /// netId del jugador que disparó. Se usa para evitar autodaño.
    /// Se asigna en el Servidor desde PlayerShoot.CmdShoot().
    /// </summary>
    [HideInInspector] public uint shooterNetId;

    public override void OnStartServer()
    {
        // Programar destrucción en el SERVIDOR para asegurar la limpieza en red.
        Invoke(nameof(ServerDestroy), lifeTime);
    }

    /// <summary>
    /// La detección de colisión se hace en TODAS las instancias,
    /// pero el daño y la destrucción autoritativa SOLO se aplican en el Servidor.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        // Ignorar otras balas (evitar choques entre proyectiles).
        if (other.GetComponent<Bullet>() != null) return;

        // Si choca con un jugador, comprobar que no sea el shooter.
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            NetworkIdentity targetId = other.GetComponent<NetworkIdentity>();
            if (targetId != null && targetId.netId == shooterNetId)
            {
                // Es el propio shooter: ignorar.
                return;
            }

            health.TakeDamage(damage);
        }

        // Destruir la bala en el Servidor (esto la elimina en todos los clientes).
        ServerDestroy();
    }

    [Server]
    private void ServerDestroy()
    {
        if (this == null || gameObject == null) return;
        NetworkServer.Destroy(gameObject);
    }
}
