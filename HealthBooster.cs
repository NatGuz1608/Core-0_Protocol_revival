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

    private void OnTriggerEnter(Collider other)
    {
        // El servidor es quien decide.
        if (!isServer) return;

        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player == null) return;

        // Solo curar si no está al máximo.
        if (player.Health >= player.MaxHealth) return;

        player.Heal(healAmount);

        // Destruir el booster en red.
        NetworkServer.Destroy(gameObject);
    }
}
