using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PixelatedHandle : MonoBehaviour
{
    public GlitchPixelated mainGiltch;
    public GlitchPixelated otherGlitch;
    public Sprite sprite;
    public float delay;
    public bool showDebug;
    void Start()
    {
        SetOriginalTexturn();
    }
    [ContextMenu("SetOriginalTexturn")]
    public void SetOriginalTexturn()
    {
        mainGiltch.SetOriginalTexturn(sprite.texture);
    }
    [ContextMenu("Pixelate")]
    public void Pixelate()
    {
        mainGiltch.PixelatedMethod();
    }
    [ContextMenu("SendSetUp")]
    public void SendSetUp()
    {

        otherGlitch.ReciveSetUp(mainGiltch.GetSetUp());

    }



    [ContextMenu("Start Loop")]
    public void StartLoopTime()
    {
        StartCoroutine(LoopTime());
    }
    [ContextMenu("Stop Loop")]
    public void StopLoopTime()
    {
        StopAllCoroutines();
    }
    private IEnumerator LoopTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            if (showDebug) Debug.Log($"Update {gameObject.name}");
            mainGiltch.canFade = false;
            float[,] colorFadeValue;
            List<Vector2Int> changedPixelsData;
            mainGiltch.GetFadeData(out colorFadeValue, out var _changedPixelsData);
            changedPixelsData = new List<Vector2Int>(_changedPixelsData);


            //   Debug.Log($"{colorFadeValue.Length}");
            //   Debug.Log($"{changedPixelsData.Count}");
            otherGlitch.ReciveData(colorFadeValue, changedPixelsData);
            mainGiltch.ClearFadeData();
            mainGiltch.canFade = true;
        }

    }

    void OnMouseDrag()
    {
        mainGiltch.MouseDrag();
    }
}
