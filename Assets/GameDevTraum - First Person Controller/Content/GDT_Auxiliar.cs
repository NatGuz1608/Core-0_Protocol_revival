using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameDevTraum
{

    public class GDT_Auxiliar : MonoBehaviour
    {
        public GameObject text;
        void Start()
        {
            text.SetActive(true);
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Application.OpenURL("https://www.youtube.com/@GameDevTraumEN/");
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                Application.OpenURL("https://gamedevtraum.itch.io/");
            }
        }

    }
}

