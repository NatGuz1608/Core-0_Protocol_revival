using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

/// <summary>
/// Cámara tercera persona en red — versión robusta:
///   • Auto-crea Camera.main si no existe.
///   • Auto-resuelve cameraPivot (busca hijo "CameraPivot" o lo crea sobre la cabeza).
///   • Conserva la colisión SphereCast contra el suelo para no atravesar terreno.
/// </summary>
public class ThirdPersonCameraMirror : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cameraPivot;

    [Header("Configuración de Seguimiento")]
    public Vector3 offset      = new Vector3(0.5f, 0.5f, -25.5f);
    public float   sensitivity = 3f;
    public float   smoothSpeed = 15f;

    [Header("Colisión de Cámara")]
    public LayerMask collisionLayers; // Marca "Default" o "Ground" en el Inspector
    public float    cameraRadius = 0.2f;

    [Header("Límites de Rotación")]
    public float minY = -20f;
    public float maxY = 60f;

    [Header("Auto-creación")]
    public bool autoCreateCameraIfMissing = true;

    private float    currentX = 0f;
    private float    currentY = 0f;
    private Camera   mainCam;
    private Transform mainCamTransform;

    public override void OnStartLocalPlayer()
    {
        ResolverPivot();
        ConfigurarCamara();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        Debug.Log($"[ThirdPersonCameraMirror] LocalPlayer listo. Pivot={cameraPivot?.name}, Cam={mainCam?.name}");
    }

    void ResolverPivot()
    {
        if (cameraPivot != null) return;

        // 1) Buscar hijo directo
        Transform t = transform.Find("CameraPivot");

        // 2) Buscar recursivo
        if (t == null)
        {
            foreach (var ch in GetComponentsInChildren<Transform>(true))
                if (ch.name == "CameraPivot") { t = ch; break; }
        }

        // 3) Crear automáticamente
        if (t == null)
        {
            var go = new GameObject("CameraPivot");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            t = go.transform;
            Debug.LogWarning("[ThirdPersonCameraMirror] CameraPivot no asignado: creado automáticamente.");
        }

        cameraPivot = t;
    }

    void ConfigurarCamara()
    {
        mainCam = Camera.main;

        if (mainCam == null && autoCreateCameraIfMissing)
        {
            var go = new GameObject("PlayerMainCamera");
            mainCam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            try { go.tag = "MainCamera"; } catch { /* tag puede no existir */ }
            Debug.LogWarning("[ThirdPersonCameraMirror] Camera.main no existía: creada en runtime.");
        }

        if (mainCam != null) mainCamTransform = mainCam.transform;
    }

    void LateUpdate()
    {
        if (!isLocalPlayer || cameraPivot == null) return;

        // Reintento si la cámara se perdió
        if (mainCamTransform == null)
        {
            ConfigurarCamara();
            if (mainCamTransform == null) return;
        }

        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            currentX += mouseDelta.x * sensitivity * 0.1f;
            currentY -= mouseDelta.y * sensitivity * 0.1f;
            currentY  = Mathf.Clamp(currentY, minY, maxY);
        }

        Quaternion rotation       = Quaternion.Euler(currentY, currentX, 0);
        Vector3    defaultTargetPos = cameraPivot.position + (rotation * offset);

        // SphereCast para evitar atravesar suelo / paredes
        Vector3 dirToCamera = defaultTargetPos - cameraPivot.position;
        float   maxDist     = dirToCamera.magnitude;

        if (maxDist > 0.001f &&
            Physics.SphereCast(cameraPivot.position, cameraRadius, dirToCamera.normalized,
                               out RaycastHit hit, maxDist, collisionLayers))
        {
            mainCamTransform.position = cameraPivot.position + (dirToCamera.normalized * (hit.distance - 0.1f));
        }
        else
        {
            mainCamTransform.position = Vector3.Lerp(mainCamTransform.position, defaultTargetPos,
                                                    Time.deltaTime * smoothSpeed);
        }

        mainCamTransform.rotation = Quaternion.Slerp(mainCamTransform.rotation, rotation,
                                                    Time.deltaTime * smoothSpeed);
    }
}
