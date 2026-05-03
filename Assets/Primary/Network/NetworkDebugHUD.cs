using UnityEngine;
using Mirror;

/// <summary>
/// HUD de diagnóstico on-screen. Pégalo en cualquier GameObject de la escena
/// (por ejemplo el NetworkManager) para ver en tiempo real:
///   • Estado de la conexión (Server/Client/Host).
///   • Cantidad de jugadores conectados.
///   • Si el LocalPlayer existe y dónde está.
///   • Posición/rotación de la cámara y si la encuentra.
///
/// QUITA este script cuando termines de depurar.
/// </summary>
public class NetworkDebugHUD : MonoBehaviour
{
    [Header("Apariencia")]
    public int   fontSize = 14;
    public Color textColor = new Color(0f, 1f, 0.6f);

    GUIStyle style;

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label) { fontSize = fontSize, normal = { textColor = textColor } };
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("─── DEBUG HUD ───");

        // Estado de red
        sb.AppendLine($"NetworkServer.active: {NetworkServer.active}");
        sb.AppendLine($"NetworkClient.active: {NetworkClient.active}");
        sb.AppendLine($"NetworkClient.isConnected: {NetworkClient.isConnected}");
        sb.AppendLine($"Connections (server): {NetworkServer.connections.Count}");

        // LocalPlayer
        var localId = NetworkClient.localPlayer;
        if (localId != null)
        {
            sb.AppendLine($"LocalPlayer netId: {localId.netId}");
            sb.AppendLine($"LocalPlayer pos:   {localId.transform.position}");
            sb.AppendLine($"LocalPlayer rot:   {localId.transform.eulerAngles}");
            sb.AppendLine($"LocalPlayer active: {localId.gameObject.activeInHierarchy}");
        }
        else
        {
            sb.AppendLine("LocalPlayer: NULL (aún no spawneado)");
        }

        // Cámara
        var cam = Camera.main;
        if (cam != null)
        {
            sb.AppendLine($"Camera.main: {cam.name} @ {cam.transform.position}");
        }
        else
        {
            sb.AppendLine("Camera.main: NULL  ← problema");
        }

        // Spawned objects
        sb.AppendLine($"Spawned objs: {NetworkServer.spawned.Count}");

        GUI.Box(new Rect(10, 80, 380, 180), "");
        GUI.Label(new Rect(20, 90, 360, 170), sb.ToString(), style);
    }
}
