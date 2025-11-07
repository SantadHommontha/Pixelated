using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;


public class GlitchPixelated : MonoBehaviour
{
    #region Public Variable
    public SpriteRenderer spriteRenderer;
    //  public GlitchPixelated otherPicture;
    public int dividePixels = 10;
    public float mouseDragRadius = 10f;
    public float fadeSpeed = 0.15f;
    public Color[] colortest;
    public bool canFade = true;
    #endregion

    #region Private Variable
    private Texture2D originalTexture;
    private Texture2D copyTexture;
    private Camera cameraMain;
    // Track which pixels were changed or restored
    private HashSet<Vector2Int> changedPixels = new HashSet<Vector2Int>();
    //  private HashSet<Vector2Int> restoredPixels = new HashSet<Vector2Int>();
    // Shared random pattern for consistent pixelation between images
    private Color[,] pixelPattern; //สีในแต่ะ px
    private float[,] coloraFade; // ค่าที่ถูกของตัว px ที่ถูกปัดไปแล้ว 0 เป็นภาพ px 1 เป็นภาพต้นฉบับ
    private float[,] colorFadeValue; // เก็บค่าที่เราปัดในแต่ละ px ก่อนที่เราจะส่งออกไป แล้วจะรีเซ็ดค่าเป็น 0 หลังจากที่ส่งไป 

    private int texturnWidth => originalTexture.width;
    private int texturnHeight => originalTexture.height;
    private Collider2D collider;
    private int sizeX, sizeY;
    #endregion
    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        cameraMain = Camera.main;
        originalTexture = spriteRenderer.sprite.texture;

        SetUp();
    }


    // เซ็คค่าพื้นฐานต่างๆ
    public void SetUp()
    {
        copyTexture = new Texture2D(originalTexture.width, originalTexture.height);
        copyTexture.SetPixels(originalTexture.GetPixels());
        copyTexture.Apply();

        canFade = true;

        sizeX = Mathf.CeilToInt(texturnWidth / (float)dividePixels);
        sizeY = Mathf.CeilToInt(texturnHeight / (float)dividePixels);

        pixelPattern = new Color[sizeX, sizeY];
        coloraFade = new float[sizeX, sizeY];
        colorFadeValue = new float[sizeX, sizeY];
    }

    void Update()
    {
        //  rayCastHit = Physics2D.Raycast(cameraMain.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        // if (Input.GetKeyDown(KeyCode.Q) && isMainSprite)
        //     PixelatedMethod();

        // if (Input.GetKeyDown(KeyCode.E))
        //     UpdateSprite(originalTexture);
        // if (Input.GetKeyDown(KeyCode.W))
        // {
        //     if (otherPicture != null)
        //         SendSetUP();
        // }

        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     if (otherPicture != null)
        //         SendData();
        // }

        // FindPixelSeclcet();
        // if (Input.GetKeyDown(KeyCode.R) && otherPicture != null)
        //     CombineFromOther(otherPicture);

        // if (Input.GetKeyDown(KeyCode.U))
        //     Debug.Log($"Seclet {FindPixelSeclcet()}");


        Debug.Log(FindGridSeclcet());

    }
    // PIXELATION
    #region Pixelated
    public void PixelatedMethod()
    {

        if (dividePixels <= 0)
            dividePixels = 1;
        /**
                //   Debug.Log("--------------");

                // if (sharedPixelPattern == null ||
                //     sharedPixelPattern.GetLength(0) != sizeX ||
                //     sharedPixelPattern.GetLength(1) != sizeY)
                // {
                //     sharedPixelPattern = new Color[sizeX, sizeY];
        **/
        for (int by = 0; by < sizeY; by++)
        {
            for (int bx = 0; bx < sizeX; bx++)
            {
                float r = Random.Range(0.2f, 0.6f);
                float g = Random.Range(0f, 0.3f);
                float b = Random.Range(0.4f, 1f);
                float a = 1f;

                pixelPattern[bx, by] = new Color(r, g, b, a);
                coloraFade[bx, by] = 0;
                colorFadeValue[bx, by] = 0;

                FadePixel(bx, by, 0);
            }
        }
        /**
                // for (int y = 0; y < height; y += dividePixels)
                // {
                //     for (int x = 0; x < width; x += dividePixels)
                //     {
                //         int bx = x / dividePixels;
                //         int by = y / dividePixels;
                //         Color pixelColor = sharedPixelPattern[bx, by];

                //         for (int dy = 0; dy < dividePixels; dy++)
                //         {
                //             for (int dx = 0; dx < dividePixels; dx++)
                //             {
                //                 int px = x + dx;
                //                 int py = y + dy;

                //                 if (px < width && py < height)
                //                 {
                //                     //      Debug.Log("__");
                //                     copyTexture.SetPixel(px, py, pixelColor);
                //                     changedPixels.Add(new Vector2Int(px, py));

                //                 }
                //             }
                //         }
                //     }
                // }
        **/
        //  copyTexture.Apply();
        //   UpdateSprite();

    }
    #endregion
    void UpdateSprite(Texture2D texture = null)
    {
        if (texture == null)
            texture = copyTexture;

        spriteRenderer.sprite = Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));
    }
    #region FadePixel
    //ทำการ lerp px ตามตำแหน่งที่กำหนด
    private void FadePixel(int _px, int _py, float _fadeValue)
    {
        Vector2Int pxIndex = new Vector2Int(_px, _py);

        var startX = pxIndex.x * dividePixels;
        var startY = pxIndex.y * dividePixels;
        //  Debug.Log($"{gameObject.name} = {_fadeValue}");
        int blockwidth = startX + dividePixels >= originalTexture.width ? originalTexture.width - startX : dividePixels;
        int blockHigth = startY + dividePixels >= originalTexture.height ? originalTexture.height - startY : dividePixels;

        //   Debug.Log($"{startX}=>{blockwidth} .. {startX}=>{blockHigth}");
        var originalColors = originalTexture.GetPixels(startX, startY, blockwidth, blockHigth);
        colortest = new Color[originalColors.Length];
        colortest = originalColors;
        var currentColor = pixelPattern[pxIndex.x, pxIndex.y];
        var newColors = new Color[originalColors.Length];

        for (int i = 0; i < originalColors.Length; i++)
        {
            newColors[i] = Color.Lerp(
                currentColor,
                originalColors[i],
                _fadeValue
            );
        }
        SetColor(startX, startY, blockwidth, blockHigth, newColors);
    }

    private void FadePixel()
    {
        // if (rayCastHit.collider != collider) return;
        //  if (!canFade) return;
        var pixelSeclcet = FindPixelSeclcet();
        var fps = FindGridSeclcet(pixelSeclcet.x, pixelSeclcet.y);
        var currentFadeValue = (coloraFade[fps.x, fps.y] += fadeSpeed);
        colorFadeValue[fps.x, fps.y] += fadeSpeed;
        FadePixel(fps.x, fps.y, currentFadeValue);

        // var changeIndex = new Vector2Int(fps.x, fps.y);
        //    if (!changedPixelsData.Contains(changeIndex))
        //      changedPixelsData.Add(changeIndex);
        changedPixels.Add(new Vector2Int(fps.x, fps.y));
        /**
                // var ogIndex = FindPixelSeclcet();
                // var pxIndex = ogIndex;
                // pxIndex.x = pxIndex.x - 1;
                // pxIndex.y = pxIndex.y - 1;
                // var startX = pxIndex.x * dividePixels;
                // var startY = pxIndex.y * dividePixels;


                // int blockwidth = startX + dividePixels >= originalTexture.width ? blockwidth = originalTexture.width - startX : blockwidth = dividePixels;
                // int blockHigth = startY + dividePixels >= originalTexture.height ? blockHigth = originalTexture.height - startY : blockHigth = dividePixels;

                // var originalColors = originalTexture.GetPixels(startX, startY, blockwidth, blockHigth);
                // colortest = new Color[originalColors.Length];
                // colortest = originalColors;
                // var currentColor = pixelateColor[pxIndex.x, pxIndex.y];
                // var newColors = new Color[originalColors.Length];
                // var currentFadeValue = (coloraFade[pxIndex.x, pxIndex.y] += fadeSpeed);
                // for (int i = 0; i < originalColors.Length; i++)
                // {
                //     newColors[i] = Color.Lerp(
                //         currentColor,
                //         originalColors[i],
                //         currentFadeValue
                //     );
                // }
                // SetColor(startX, startY, blockwidth, blockHigth, newColors);
                **/
    }
    //เซ็คสีที่ต่างๆลงใน texture
    private void SetColor(int _startPx, int _StartPy, int _blockWidth, int blockHight, Color[] _color)
    {
        copyTexture.SetPixels(_startPx, _StartPy, _blockWidth, blockHight, _color);
        copyTexture.Apply();
        UpdateSprite();
    }

    //หาว่าเมาร์ไปชี่อยู่ที่ pixel grid ที่เท่าไร
    private Vector2Int FindPixelSeclcet()
    {
        Vector3 localPos = transform.InverseTransformPoint(RaycastHit().point);
        Sprite sprite = spriteRenderer.sprite;
        float ppu = sprite.pixelsPerUnit;

        Vector2 localPixelPos = new Vector2(localPos.x * ppu, localPos.y * ppu);
        Vector2 pivotPixel = sprite.pivot;
        Rect rect = sprite.rect;

        Vector2 pixelCoords = new Vector2(localPixelPos.x + pivotPixel.x, localPixelPos.y + pivotPixel.y);

        pixelCoords.x = Mathf.Clamp(pixelCoords.x, rect.x, rect.xMax - 1);
        pixelCoords.y = Mathf.Clamp(pixelCoords.y, rect.y, rect.yMax - 1);

        if (sprite.packed)
        {
            pixelCoords.x += rect.x;
            pixelCoords.y += rect.y;
        }
        return new Vector2Int(Mathf.CeilToInt(pixelCoords.x), Mathf.CeilToInt(pixelCoords.y)); ;
    }

    private Vector2Int FindGridSeclcet(int _px, int _py)
    {
        return new Vector2Int(Mathf.CeilToInt(_px / dividePixels), Mathf.CeilToInt(_py / dividePixels));
    }
    private Vector2Int FindGridSeclcet()
    {
        var pixelSeclcet = FindPixelSeclcet();
        return new Vector2Int(Mathf.CeilToInt(pixelSeclcet.x / dividePixels), Mathf.CeilToInt(pixelSeclcet.y / dividePixels));
    }
    private void FindPixelSeclcetInMouseRadius()
    {

        
    }
    #endregion
    // MOUSE INTERACTION
    void OnMouseDrag()
    {
        if (RaycastHit().collider == collider)
        {
            if (!canFade) return;
            FadePixel();
        }
    }

    /**ตอนปัด
    // void ReturnOriginalPixels(int posX, int posY)
    // {
    //     int width = originalTexture.width;
    //     int height = originalTexture.height;

    //     for (int y = -Mathf.RoundToInt(mouseDragRadius); y <= mouseDragRadius; y++)
    //     {
    //         for (int x = -Mathf.RoundToInt(mouseDragRadius); x <= mouseDragRadius; x++)
    //         {
    //             int px = posX + x;
    //             int py = posY + y;
    //             if (px < 0 || py < 0 || px >= width || py >= height)
    //                 continue;

    //             Vector2Int pixelPos = new Vector2Int(px, py);

    //             // Skip already restored pixels
    //             if (restoredPixels.Contains(pixelPos))
    //                 continue;

    //             Color currentColor = copyTexture.GetPixel(px, py);
    //             Color originalColor = originalTexture.GetPixel(px, py);
    //             //  coloraplha[px, py] -= fadespeedA;
    //             Color fadedColor = Color.Lerp(currentColor, originalColor, fadeSpeed);




    //             copyTexture.SetPixel(px, py, fadedColor);
    //             changedPixels.Add(pixelPos);

    //             // When nearly identical to original, mark restored and remove from changed list
    //             if (Vector4.Distance(fadedColor, originalColor) < 0.02f)
    //             {
    //                 restoredPixels.Add(pixelPos);
    //                 changedPixels.Remove(pixelPos);
    //             }
    //         }
    //     }

    //     copyTexture.Apply();
    // }

    // COMBINE FUNCTION

    // public void CombineFromOther(GlitchPixelated other)
    // {
    //     if (other == null || other.copyTexture == null) return;

    //     int w = copyTexture.width;
    //     int h = copyTexture.height;

    //     HashSet<Vector2Int> allChanged = new HashSet<Vector2Int>(changedPixels);
    //     foreach (var pos in other.changedPixels)
    //         allChanged.Add(pos);

    //     foreach (Vector2Int pos in allChanged)
    //     {
    //         if (pos.x < 0 || pos.y < 0 || pos.x >= w || pos.y >= h)
    //             continue;

    //         bool mineChanged = changedPixels.Contains(pos);
    //         bool otherChanged = other.changedPixels.Contains(pos);

    //         //Skip combining if both are already clean (restored)
    //         if (!mineChanged && !otherChanged)
    //         {
    //             Color originalColor = originalTexture.GetPixel(pos.x, pos.y);
    //             copyTexture.SetPixel(pos.x, pos.y, originalColor);
    //             continue;
    //         }

    //         // If one is restored but not the other, keep original
    //         if (restoredPixels.Contains(pos) || other.restoredPixels.Contains(pos))
    //         {
    //             Color originalColor = originalTexture.GetPixel(pos.x, pos.y);
    //             copyTexture.SetPixel(pos.x, pos.y, originalColor);
    //             restoredPixels.Add(pos);
    //             changedPixels.Remove(pos);
    //             continue;
    //         }

    //         Color myColor = copyTexture.GetPixel(pos.x, pos.y);
    //         Color otherColor = other.copyTexture.GetPixel(pos.x, pos.y);
    //         Color finalColor;

    //         if (mineChanged && otherChanged)
    //             finalColor = Color.Lerp(myColor, otherColor, 0.5f);
    //         else if (otherChanged)
    //             finalColor = otherColor;
    //         else
    //             finalColor = myColor;

    //         copyTexture.SetPixel(pos.x, pos.y, finalColor);
    //     }

    //     copyTexture.Apply();
    //     UpdateSprite();

    //     changedPixels = allChanged;

    //     foreach (var pos in other.restoredPixels)
    //         restoredPixels.Add(pos);

    //     other.changedPixels.Clear();
    // }
**/


    public void ClearFadeData()

    {
        Set2DArrayToValue(colorFadeValue, 0);
        // changedPixelsData.Clear();
        changedPixels.Clear();
    }
    #region Send ,Recive

    //ส่งข้อมูลการปัดตำแหน่งต่างๆ ไป
    public void SendData()
    {
        //    otherPicture.ReciveData(colorFadeValue, changedPixelsData);

        // ClearFadeData();
    }
    //รับค่าที่คนอื่นปัดมาบวกเข้ากับตัวเอง
    public void ReciveData(float[,] _colorFadeValue, List<Vector2Int> _changedPixels)
    {
        canFade = false;
        Add2DArrayToValue(coloraFade, _colorFadeValue);
        foreach (var T in _changedPixels)
        {
            FadePixel(T.x, T.y, coloraFade[T.x, T.y]);
        }
        canFade = true;
    }
    public void GetFadeData(out float[,] _colorFadeValue, out HashSet<Vector2Int> _changedPixels)
    {
        _colorFadeValue = colorFadeValue;
        //   _changedPixelsData = changedPixelsData;
        _changedPixels = changedPixels;
    }

    // ส่งค่าจากตัว setup ไปให้อีกรูปนึง
    public void GetSetUp(out Color[,] _pixelateColorPatturn, out Color[] _allColors)
    {
        // otherPicture.ReciveSetUp(dividePixels, mouseDragRadius, fadeSpeed, pixelateColor, copyTexture.GetPixels());
        _pixelateColorPatturn = pixelPattern;
        _allColors = copyTexture.GetPixels();
    }
    public void ReciveSetUp(out int _dividePixels, out float _mouseDragRadius, out float _fadeSpeed, out Color[,] _pixelatePatturn, out Color[] _allColors)
    {
        _dividePixels = dividePixels;
        _mouseDragRadius = mouseDragRadius;
        _fadeSpeed = fadeSpeed;
        _pixelatePatturn = pixelPattern;
        _allColors = copyTexture.GetPixels();
    }


    public GlitchPixelated GetSetUp() => this;

    // รับค่า setup จากอันอื่น
    public void ReciveSetUp(GlitchPixelated _glitchPixelated)
    {
        ReciveSetUp(_glitchPixelated.dividePixels, _glitchPixelated.mouseDragRadius, _glitchPixelated.fadeSpeed, _glitchPixelated.pixelPattern, _glitchPixelated.copyTexture.GetPixels());
    }

    public void ReciveSetUp(Color[,] _pixelatePatturn, Color[] _allColors)
    {

        ReciveSetUp(dividePixels, mouseDragRadius, fadeSpeed, _pixelatePatturn, _allColors);
    }
    public void ReciveSetUp(int _dividePixels, float _mouseDragRadius, float _fadeSpeed, Color[,] _pixelatePatturn, Color[] _allColors)
    {
        SetUp();
        dividePixels = _dividePixels;
        mouseDragRadius = _mouseDragRadius;
        fadeSpeed = _fadeSpeed;
        pixelPattern = _pixelatePatturn;

        for (int by = 0; by < sizeY; by++)
        {
            for (int bx = 0; bx < sizeX; bx++)
            {
                FadePixel(bx, by, 0);
            }
        }
        copyTexture.Apply();
        UpdateSprite();
    }
    #endregion

    #region Utility

    private RaycastHit2D RaycastHit()
    {
        return Physics2D.Raycast(cameraMain.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
    }
    // เซ็ตค่า array 2 2มิติให้มีแค่เท่ากับ value
    private void Set2DArrayToValue<T>(T[,] _array, T _value)
    {
        if (_array == null) return;

        int rows = _array.GetLength(0);
        int cols = _array.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                _array[i, j] = _value;
            }
        }
    }
    // บวกค่าใน array 2 มิติอันที่ 1 ให้มีแค่เพิ่มขึ้นตาม array ที่ 2
    private void Add2DArrayToValue(float[,] _array1, float[,] _array2)
    {
        if (_array1 == null || _array2 == null) return;

        int rows1 = _array1.GetLength(0);
        int cols1 = _array1.GetLength(1);

        int rows2 = _array2.GetLength(0);
        int cols2 = _array2.GetLength(1);

        if (rows1 != rows2) return;
        if (cols1 != cols2) return;

        for (int i = 0; i < rows1; i++)
        {
            for (int j = 0; j < cols1; j++)
            {
                _array1[i, j] += _array2[i, j];
            }
        }
    }
    #endregion


#if UNITY_EDITOR
    #region Debug
    // debug ค่าของ coloraFade
    private void DebugColorFade(float[,] _colorFade)
    {
        int num = 0;
        Debug.Log("-------------------------TOP-------------------------------");
        foreach (var T in _colorFade)
        {
            string g = T > 0 ? "<----------------------------" : "";
            Debug.Log($"{num++}-{gameObject.name} : {T} {g}");
        }
        Debug.Log("------------------------END-----------------------------");
    }

    private void DebugColorFade() => DebugColorFade(coloraFade);
    #endregion
#endif
}