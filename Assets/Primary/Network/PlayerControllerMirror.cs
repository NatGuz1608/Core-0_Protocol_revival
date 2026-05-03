using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador de jugador en red — Versión Corregida:
/// • Incluye guardas de seguridad para evitar errores de "inactive controller".
/// • Solo procesa el movimiento si es el LocalPlayer.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkIdentity))]
public class PlayerControllerMirror : NetworkBehaviour
{
    [Header("Configuración de Movimiento")]
    public float speed     = 5.0f;
    public float gravity   = 20.0f;
    public float jumpForce = 8.0f;

    [Header("Anti-caída al vacío")]
    public float   safeFloorY  = -20f;
    public Vector3 safeRespawn = new Vector3(376f, 25f, 374f);

    [Header("Referencias")]
    [SerializeField] private Animator anim;
    [SerializeField] private NetworkAnimator netAnim;
    [SerializeField] private PlayerInput playerInput;

    private CharacterController controller;
    private Transform cameraTransform;
    private Vector2   inputMovement;
    private Vector3   moveDirection = Vector3.zero;
    private bool      isFiring;

    static readonly int HashIdle    = Animator.StringToHash("Idle");
    static readonly int HashFiring  = Animator.StringToHash("Firing");
    static readonly int HashSpeed   = Animator.StringToHash("Speed");
    static readonly int HashBlend   = Animator.StringToHash("Blend");
    static readonly int HashJump    = Animator.StringToHash("Jump");
    static readonly int HashHit     = Animator.StringToHash("Hit");

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        // Búsqueda automática de componentes si no se asignaron en el Inspector
        if (anim        == null) anim        = GetComponentInChildren<Animator>();
        if (netAnim     == null) netAnim     = GetComponent<NetworkAnimator>();
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
    }

    public override void OnStartLocalPlayer()
    {
        if (Camera.main != null) cameraTransform = Camera.main.transform;
        
        // Habilitar el input solo para quien controla este personaje
        if (playerInput != null) playerInput.enabled = true;
        
        // Asegurar que el controlador esté activo al iniciar
        if (controller != null) controller.enabled = true;

        Debug.Log("[PlayerControllerMirror] LocalPlayer iniciado correctamente.");
    }

    public override void OnStartClient()
    {
        // Desactivar el input en los clones de otros jugadores para evitar conflictos
        if (!isLocalPlayer && playerInput != null) playerInput.enabled = false;
    }

    // Callbacks del Input System
    public void OnMove(InputValue value)   => inputMovement = value.Get<Vector2>();
    public void OnJump(InputValue value)   { if (value.isPressed) ProcesarSalto(); }
    public void OnAttack(InputValue value) => isFiring = value.isPressed;

    /// <summary>
    /// Centraliza la comprobación: ¿podemos tocar el CharacterController de forma segura?
    /// True solo si soy el dueño, el GO está activo, y el controller existe y está habilitado.
    /// </summary>
    private bool IsControllerUsable()
    {
        return isLocalPlayer
            && this != null
            && gameObject != null
            && gameObject.activeInHierarchy
            && controller != null
            && controller.enabled;
    }

    void Update()
    {
        // 1. Solo el dueño del objeto procesa input/movimiento
        if (!isLocalPlayer) return;

        // 2. GUARDA CRÍTICA inicial — bloquea TODO si el controller no es usable
        if (!IsControllerUsable()) return;

        // Lógica de anti-caída
        if (transform.position.y < safeFloorY)
        {
            TeletransportarASeguridad();
            return;
        }

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null) ManejarMovimiento();

        ActualizarParametrosAnimator();
    }

    private void TeletransportarASeguridad()
    {
        // Si en este frame ya no es usable, salimos en silencio
        if (controller == null) return;

        bool wasEnabled = controller.enabled;
        controller.enabled = false;          // desactivar antes del cambio de posición
        transform.position = safeRespawn;
        if (wasEnabled) controller.enabled = true; // restaurar si lo estaba
        moveDirection = Vector3.zero;
        Debug.LogWarning("Caída detectada: Teletransportando a posición segura.");
    }

    private void ManejarMovimiento()
    {
        // 3. SEGUNDA GUARDA: NetworkTransform/respawn pudo haber deshabilitado el controller
        //    entre el inicio del Update y este punto. Comprobamos justo antes de Move().
        if (!IsControllerUsable()) return;

        if (controller.isGrounded)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right   = cameraTransform.right;
            forward.y = 0; right.y = 0;
            forward.Normalize(); right.Normalize();

            moveDirection = (forward * inputMovement.y + right * inputMovement.x).normalized * speed;
        }

        moveDirection.y -= gravity * Time.deltaTime;

        // 4. GUARDA FINAL inmediata antes de Move — la más crítica.
        if (controller != null && controller.enabled)
        {
            controller.Move(moveDirection * Time.deltaTime);
        }

        if (inputMovement.sqrMagnitude > 0.01f && cameraTransform != null)
        {
            Quaternion targetRot = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
            transform.rotation   = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    private void ProcesarSalto()
    {
        if (!IsControllerUsable()) return;
        if (!controller.isGrounded) return;
        moveDirection.y = jumpForce;

        if (netAnim != null) netAnim.SetTrigger(HashJump);
        else if (anim != null) anim.SetTrigger(HashJump);
    }

    private void ActualizarParametrosAnimator()
    {
        if (anim == null) return;
        float speedMag = inputMovement.magnitude;
        anim.SetFloat(HashSpeed, speedMag);
        anim.SetFloat(HashBlend, inputMovement.y);
        anim.SetBool (HashIdle, speedMag < 0.01f);
        anim.SetBool (HashFiring, isFiring);
    }
}