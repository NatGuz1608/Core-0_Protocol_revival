using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// UI de salud para el jugador local. Se suscribe al evento de PlayerHealth.
/// - Slider y Text son opcionales: si no están, simplemente no se actualizan.
/// - Búsqueda de HUD con tag protegida contra TagNotDefinedException.
/// </summary>
public class HealthUI : NetworkBehaviour
{
    [Header("UI (opcional)")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;
    [SerializeField] private Canvas worldCanvas;
    [Tooltip("Tag del Canvas global del HUD. Si el tag no existe simplemente se ignora.")]
    [SerializeField] private string hudTag = "HUD";

    private PlayerHealth health;

    void Awake() => health = GetComponent<PlayerHealth>();

    public override void OnStartLocalPlayer()
    {
        TryAutoFindHud();

        if (worldCanvas != null) worldCanvas.enabled = false;
        if (health != null) health.OnHealthChangedEvent += UpdateUI;
    }

    public override void OnStartClient()
    {
        if (!isLocalPlayer && worldCanvas != null) worldCanvas.enabled = true;
        if (health != null) health.OnHealthChangedEvent += UpdateUI;
    }

    /// <summary>
    /// FindGameObjectWithTag tira excepción si el tag no está definido en
    /// Tags & Layers. Lo envolvemos para que sea seguro.
    /// </summary>
    void TryAutoFindHud()
    {
        if (string.IsNullOrEmpty(hudTag)) return;

        GameObject hud = null;
        try { hud = GameObject.FindGameObjectWithTag(hudTag); }
        catch (UnityException) { /* Tag no definido: lo ignoramos */ }

        if (hud == null) return;

        if (healthSlider == null) healthSlider = hud.GetComponentInChildren<Slider>(true);
        if (healthText   == null) healthText   = hud.GetComponentInChildren<Text>(true);
    }

    void OnDestroy()
    {
        if (health != null) health.OnHealthChangedEvent -= UpdateUI;
    }

    void UpdateUI(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value    = current;
        }
        if (healthText != null) healthText.text = $"{current} / {max}";
    }
}
