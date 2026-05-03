using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// Salud de jugador en red.
/// - El Servidor es la única autoridad que modifica 'health'.
/// - 'health' es SyncVar con hook → todos los clientes ven el cambio sin pedirlo.
/// - El Booster se procesa en el Servidor también (anti-cheat básico).
///
/// Requisitos en el GameObject del Player:
///   - NetworkIdentity
///   - Collider (no trigger) y Rigidbody si quieres físicas; o Collider trigger según tu juego.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class PlayerHealth : NetworkBehaviour
{
    [Header("Salud")]
    [SerializeField] private int maxHealth = 100;

    [SyncVar(hook = nameof(OnHealthChanged))]
    private int health;

    [Header("UI (opcional, sólo para LocalPlayer)")]
    [SerializeField] private Slider healthBar;

    public int Health => health;
    public int MaxHealth => maxHealth;

    public override void OnStartServer()
    {
        health = maxHealth;
    }

    public override void OnStartLocalPlayer()
    {
        // Inicializar UI local si existe.
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }
    }

    // ---------- SERVIDOR ----------

    /// <summary>
    /// Llamado por la bala (en el Servidor) al impactar.
    /// </summary>
    [Server]
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        health = Mathf.Max(0, health - amount);

        if (health <= 0)
        {
            ServerHandleDeath();
        }
    }

    /// <summary>
    /// Llamado por el Booster (en el Servidor) al recogerlo.
    /// </summary>
    [Server]
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        health = Mathf.Min(maxHealth, health + amount);
    }

    [Server]
    private void ServerHandleDeath()
    {
        // Aquí podrías reaparecer al jugador, mostrar animación de muerte, etc.
        // Por simplicidad, lo reseteamos al máximo y lo movemos al origen.
        health = maxHealth;
        RpcOnRespawn();
        transform.position = Vector3.zero;
    }

    // ---------- HOOK SYNCVAR ----------

    /// <summary>
    /// Se ejecuta en TODOS los clientes cuando 'health' cambia en el Servidor.
    /// Aquí actualizamos UI y efectos visuales sin coste extra de red.
    /// </summary>
    private void OnHealthChanged(int oldValue, int newValue)
    {
        if (isLocalPlayer && healthBar != null)
        {
            healthBar.value = newValue;
        }

        // Aquí puedes lanzar partículas de daño, sonidos, etc.
        // Ej.: if (newValue < oldValue) PlayHitEffect();
    }

    [ClientRpc]
    private void RpcOnRespawn()
    {
        // Efectos de respawn en todos los clientes (opcional).
    }
}
