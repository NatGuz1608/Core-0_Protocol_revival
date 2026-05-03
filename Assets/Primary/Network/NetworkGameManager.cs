using UnityEngine;
using Mirror;

/// <summary>
/// NetworkManager personalizado.
/// - Garantiza que NetworkManagerHUD esté presente para pruebas rápidas.
/// - Spawnea al jugador en un NetworkStartPosition aleatorio si existe; si no,
///   en una posición segura por defecto.
/// </summary>
[RequireComponent(typeof(NetworkManagerHUD))]
public class NetworkGameManager : NetworkManager
{
    [Header("Spawn")]
    public Vector3 fallbackSpawn = new Vector3(376f, 30f, 374f);

    public override void Awake()
    {
        base.Awake();

        // Asegurar que el HUD existe (para Host / Client / Server quick test)
        if (GetComponent<NetworkManagerHUD>() == null)
            gameObject.AddComponent<NetworkManagerHUD>();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Transform startPos = GetStartPosition();
        Vector3 pos    = startPos != null ? startPos.position : fallbackSpawn;
        Quaternion rot = startPos != null ? startPos.rotation : Quaternion.identity;

        GameObject player = Instantiate(playerPrefab, pos, rot);
        player.name = $"{playerPrefab.name} [Conn{conn.connectionId}]";

        NetworkServer.AddPlayerForConnection(conn, player);
        Debug.Log($"[NetworkGameManager] Player spawned for connection {conn.connectionId} at {pos}");
    }
}