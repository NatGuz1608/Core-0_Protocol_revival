using UnityEngine;
using Mirror;

/// <summary>
/// Pickup que cura al jugador.
/// - SOLO el Servidor decide si el pickup se consume (autoridad de juego).
/// - El objeto se destruye en red para que desaparezca en todos los clientes.
///
/// Requisitos en el Prefab Booster:
///   - NetworkIdentity
///   - Collider (Is Trigger = true)
///   - Registrado en NetworkManager > Spawnable Prefabs si lo vas a instanciar dinámicamente.
///     Si lo colocas a mano en la escena, basta con que tenga NetworkIdentity.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(Collider))]
public class HealthBooster : NetworkBehaviour
{
    [Header("Curación")]
    [SerializeField] private int healAmount = 25;

    /// <summary>
    /// [ServerCallback] garantiza que sólo se ejecute en el SERVIDOR
    /// (autoridad de juego). Si la colisión sucede en un cliente, el método
    /// no se ejecuta y se evita cualquier desincronización.
    /// </summary>
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        // Buscar PlayerHealth en el objeto o en su jerarquía
        PlayerHealth player = other.GetComponent<PlayerHealth>()
                              ?? other.GetComponentInParent<PlayerHealth>();
        if (player == null) return;

        // Solo curar si no está al máximo.
        if (player.Health >= player.MaxHealth) return;

        // Llamada autoritativa: incrementa el SyncVar 'health' en PlayerHealth
        player.Heal(healAmount);

        // Destruir el booster en red para que desaparezca en todos los clientes.
        NetworkServer.Destroy(gameObject);
    }
}