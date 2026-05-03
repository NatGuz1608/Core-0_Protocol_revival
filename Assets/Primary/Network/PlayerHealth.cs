using System;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// Salud del jugador en red.
/// - El SERVIDOR es la única autoridad que modifica 'health'.
/// - 'health' es SyncVar con hook → todos los clientes la ven actualizada sin pedirlo.
/// - Expone OnHealthChangedEvent para que la UI (HealthUI) se suscriba.
/// - El daño se solicita desde Bullet (Servidor) y la curación desde HealthBooster (Servidor).
///
/// Requisitos en el GameObject del Player:
///   - NetworkIdentity
///   - Collider/CharacterController para que las balas puedan impactarlo.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class PlayerHealth : NetworkBehaviour
{
    [Header("Salud")]
    [SerializeField] private int maxHealth = 100;

    [SyncVar(hook = nameof(OnHealthChanged))]
    private int health;

    [Header("UI local (opcional)")]
    [Tooltip("Slider directo en el prefab (p.ej. una barra sobre la cabeza). " +
             "Para HUD global usa HealthUI que se suscribe a OnHealthChangedEvent.")]
    [SerializeField] private Slider healthBar;

    [Header("Respawn")]
    [SerializeField] private Vector3 respawnPosition = Vector3.zero;

    /// <summary>Evento global: (currentHealth, maxHealth). Lo usa HealthUI.</summary>
    public event Action<int, int> OnHealthChangedEvent;

    public int Health    => health;
    public int MaxHealth => maxHealth;

    public override void OnStartServer()
    {
        health = maxHealth;
    }

    public override void OnStartClient()
    {
        // Disparar el evento al iniciar para que la UI muestre el valor inicial correcto,
        // incluso si nos conectamos en mitad de una partida.
        OnHealthChangedEvent?.Invoke(health, maxHealth);
    }

    public override void OnStartLocalPlayer()
    {
        // Inicializar UI directa si se asignó en el prefab.
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value    = health;
        }
    }

    // ───────────────────── SERVIDOR ─────────────────────

    /// <summary>Llamado por Bullet (en el Servidor) al impactar.</summary>
    [Server]
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        health = Mathf.Max(0, health - amount);

        if (health <= 0)
            ServerHandleDeath();
    }

    /// <summary>Llamado por HealthBooster (en el Servidor) al recogerlo.</summary>
    [Server]
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        health = Mathf.Min(maxHealth, health + amount);
    }

    [Server]
    private void ServerHandleDeath()
    {
        // Reaparición simple: full HP y mover al punto de respawn.
        // CRÍTICO: el CharacterController DEBE desactivarse antes de tocar transform.position
        // y reactivarse INMEDIATAMENTE después para evitar 'Move called on inactive controller'.
        var cc = GetComponent<CharacterController>();
        bool wasEnabled = cc != null && cc.enabled;

        if (cc != null) cc.enabled = false;
        transform.position = respawnPosition;
        if (cc != null && wasEnabled) cc.enabled = true;

        health = maxHealth;
        RpcOnRespawn();
    }

    // ───────────────────── HOOK SYNCVAR ─────────────────────

    /// <summary>
    /// Se ejecuta en TODOS los clientes cuando 'health' cambia en el Servidor.
    /// Sin coste extra de red, ya que viaja por el SyncVar existente.
    /// </summary>
    private void OnHealthChanged(int oldValue, int newValue)
    {
        // UI directa (slider del prefab).
        if (healthBar != null)
            healthBar.value = newValue;

        // Aviso a HealthUI / otros listeners.
        OnHealthChangedEvent?.Invoke(newValue, maxHealth);
    }

    [ClientRpc]
    private void RpcOnRespawn()
    {
        // Punto de extensión para FX/sonidos de respawn en todos los clientes.
    }
}
