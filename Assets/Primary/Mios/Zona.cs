using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class zona : MonoBehaviour
{
    [Tooltip("El índice de la escena en Build Settings")]
    public int numeroEscena;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            if (numeroEscena < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(numeroEscena);
            }
            else
            {
                Debug.LogError("Error: El índice de escena " + numeroEscena + " no existe en Build Settings.");
            }
        }
    }
}