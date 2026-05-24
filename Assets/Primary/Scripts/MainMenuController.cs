using UnityEngine;
using Mirror; 

public class MainMenuController : MonoBehaviour
{
    [Header("Configuración de Red")]
    [SerializeField] private NetworkManager networkManager;

    [Header("Paneles del Menú")]
    [SerializeField] private GameObject panelMenuPrincipal; // El objeto que contiene los 3 botones principales
    [SerializeField] private GameObject panelCreditos;      // El objeto PanelCreditos que acabamos de crear

    private void Start()
    {
        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<NetworkManager>();
        }

        // Nos aseguramos de que el menú empiece en el estado correcto
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelCreditos != null) panelCreditos.SetActive(false);
    }

    public void UnirseAPartida()
    {
        if (networkManager != null)
        {
            networkManager.StartClient();
        }
    }

    public void CrearPartida()
    {
        if (networkManager != null)
        {
            networkManager.StartHost();
        }
    }

    /// <summary>
    /// BOTÓN: Créditos (Muestra el panel de créditos y oculta el menú principal)
    /// </summary>
    public void MostrarCreditos()
    {
        if (panelCreditos != null && panelMenuPrincipal != null)
        {
            panelMenuPrincipal.SetActive(false); // Oculta Iniciar, Crear y Créditos
            panelCreditos.SetActive(true);       // Muestra los créditos
        }
    }

    /// <summary>
    /// BOTÓN: Volver (Regresa al menú principal)
    /// </summary>
    public void VolverAlMenuPrincipal()
    {
        if (panelCreditos != null && panelMenuPrincipal != null)
        {
            panelMenuPrincipal.SetActive(true);  // Muestra de nuevo los botones principales
            panelCreditos.SetActive(false);      // Oculta los créditos
        }
    }
}