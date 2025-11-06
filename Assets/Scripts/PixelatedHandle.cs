using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PixelatedHandle : MonoBehaviour
{
    public GlitchPixelated mainGiltch;
    public GlitchPixelated otherGlitch;
    public float delay;
    void Start()
    {

    }

    [ContextMenu("GetSetUp")]
    public void GetSetUp()
    {
        mainGiltch.GetSetUp(out var _pixelateColorPatturn, out var _allColors);
        otherGlitch.ReciveSetUp(_pixelateColorPatturn, _allColors);

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
            Debug.Log($"Update {gameObject.name}");
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
}
