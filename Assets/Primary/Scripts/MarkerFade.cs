using UnityEngine;

public class MarkerFade : MonoBehaviour
{
    public Transform player;
    public float fadeStartDistance = 10f; 
    public float fadeEndDistance = 3f;   

    private CanvasGroup canvasGroup; 
    private SpriteRenderer spriteRenderer; 

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
            if (player == null) return; 

                float dist = Vector3.Distance(transform.position, player.position);

                float alpha = Mathf.InverseLerp(fadeEndDistance, fadeStartDistance, dist);

                if (canvasGroup != null) canvasGroup.alpha = alpha;
                if (spriteRenderer != null) 
                {
                    Color c = spriteRenderer.color;
                    c.a = alpha;
                    spriteRenderer.color = c;
                
        }
    }
}
