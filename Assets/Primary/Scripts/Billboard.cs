using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        // Buscamos la cámara de forma segura
        Camera cam = Camera.main;

        if (cam != null)
        {
            // Solo rotamos si encontramos la cámara
            transform.LookAt(transform.position + cam.transform.forward);
        }
        else
        {
            // Esto te avisará en la consola si el problema es la cámara
            Debug.LogWarning("¡No encontré la Main Camera! Asegúrate de que tenga el Tag 'MainCamera'");
        }
    }
}