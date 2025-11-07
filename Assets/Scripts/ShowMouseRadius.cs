using Unity.VisualScripting;
using UnityEngine;

public class ShowMouseRadius : MonoBehaviour
{
    public GlitchPixelated glitchPixelated;
    private bool gameStart;
    private void Start()
    {
        if (glitchPixelated == null) glitchPixelated = GetComponent<GlitchPixelated>();
        gameStart = true;
    }
    void OnDrawGizmos()
    {
        if (!gameStart) return;
        //    Debug.Log("GIZ");
        if (glitchPixelated.mouseOnSprite)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawSphere(Camera.main.ScreenToWorldPoint(Input.mousePosition), glitchPixelated.mouseDragRadius / glitchPixelated.spriteRenderer.sprite.pixelsPerUnit);
        }
    }
}
