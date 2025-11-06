using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;


public class GlitchPixelated : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public GlitchPixelated otherPicture;

    public int dividePixels = 10;
    public float mouseDragRadius = 10f;
    public float fadeSpeed = 0.15f;

    private Texture2D originalTexture;
    private Texture2D copyTexture;
    private Camera cameraMain;
    private RaycastHit2D rayCastHit;

    // Track which pixels were changed or restored
    // public HashSet<Vector2Int> changedPixels = new HashSet<Vector2Int>();
    //  private HashSet<Vector2Int> restoredPixels = new HashSet<Vector2Int>();
    // Shared random pattern for consistent pixelation between images
    //private Color[,] sharedPixelPattern;

    private Color[,] pixelateColor; //สีในแต่ะ px
    public List<Vector2Int> changedPixelsData = new List<Vector2Int>(); //เก็บว่ามี grid ใหนเปลี่ยนบ้าง

    private float[,] coloraFade; // ค่าที่ถูกของตัว px ที่ถูกปัดไปแล้ว 0 เป็นภาพ px 1 เป็นภาพต้นฉบับ
    private float[,] colorFadeValue; // เก็บค่าที่เราปัดในแต่ละ px ก่อนที่เราจะส่งออกไป แล้วจะรีเซ็ดค่าเป็น 0 หลังจากที่ส่งไป 

    private int width => originalTexture.width;
    private int height => originalTexture.height;
    private Collider2D collider;
    private int sizeX, sizeY;
    public bool isMainSprite;
    public Color[] colortest;
    private bool canFade = true;
    //[Range(0f, 1f)] public float F = 0;
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
    private void SetUp()
    {
        copyTexture = new Texture2D(originalTexture.width, originalTexture.height);
        copyTexture.SetPixels(originalTexture.GetPixels());
        copyTexture.Apply();

        canFade = true;

        sizeX = Mathf.CeilToInt(width / (float)dividePixels);
        sizeY = Mathf.CeilToInt(height / (float)dividePixels);

        pixelateColor = new Color[sizeX, sizeY];
        coloraFade = new float[sizeX, sizeY];
        colorFadeValue = new float[sizeX, sizeY];
        pixelateColor = new Color[sizeX, sizeY];

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

    void Update()
    {
        rayCastHit = Physics2D.Raycast(cameraMain.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (Input.GetKeyDown(KeyCode.Q) && isMainSprite)
            PixelatedMethod();

        // if (Input.GetKeyDown(KeyCode.E))
        //     UpdateSprite(originalTexture);
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (otherPicture != null)
                SendSetUP();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (otherPicture != null)
                SendData();
        }

        // FindPixelSeclcet();
        // if (Input.GetKeyDown(KeyCode.R) && otherPicture != null)
        //     CombineFromOther(otherPicture);

        if (Input.GetKeyDown(KeyCode.U))
            Debug.Log($"Seclet {FindPixelSeclcet()}");

        if (Input.GetKeyDown(KeyCode.F))
            DebugColorFade();
        if (Input.GetKeyDown(KeyCode.G))
            DebugColorFade(colorFadeValue);


    }

    // PIXELATION
  
    void PixelatedMethod()
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

                pixelateColor[bx, by] = new Color(r, g, b, a);
                coloraFade[bx, by] = 0;
                colorFadeValue[bx, by] = 0;
             
                FadePixel(bx + 1, by + 1, 0);
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
        copyTexture.Apply();
        UpdateSprite();

    }

    void UpdateSprite(Texture2D texture = null)
    {
        if (texture == null)
            texture = copyTexture;

        spriteRenderer.sprite = Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));
    }
    //ทำการ lerp px ตามตำแหน่งที่กำหนด
    private void FadePixel(int _px, int _py, float _fadeValue)
    {
        Vector2Int pxIndex = new Vector2Int(_px - 1, _py - 1);

        var startX = pxIndex.x * dividePixels;
        var startY = pxIndex.y * dividePixels;
        //  Debug.Log($"{gameObject.name} = {_fadeValue}");
        int blockwidth = startX + dividePixels >= originalTexture.width ? originalTexture.width - startX : dividePixels;
        int blockHigth = startY + dividePixels >= originalTexture.height ? originalTexture.height - startY : dividePixels;

        //   Debug.Log($"{startX}=>{blockwidth} .. {startX}=>{blockHigth}");
        var originalColors = originalTexture.GetPixels(startX, startY, blockwidth, blockHigth);
        colortest = new Color[originalColors.Length];
        colortest = originalColors;
        var currentColor = pixelateColor[pxIndex.x, pxIndex.y];
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
        if (rayCastHit.collider != collider) return;
        //  if (!canFade) return;
        var fps = FindPixelSeclcet();
        var currentFadeValue = (coloraFade[fps.x - 1, fps.y - 1] += fadeSpeed);
        colorFadeValue[fps.x - 1, fps.y - 1] += fadeSpeed;
        FadePixel(fps.x, fps.y, currentFadeValue);

        var changeIndex = new Vector2Int(fps.x, fps.y);
        if (!changedPixelsData.Contains(changeIndex))
            changedPixelsData.Add(changeIndex);
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
        Vector3 localPos = transform.InverseTransformPoint(rayCastHit.point);
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

        var vc2Int = new Vector2Int(Mathf.CeilToInt(pixelCoords.x / dividePixels), Mathf.CeilToInt(pixelCoords.y / dividePixels));

        return vc2Int;
    }

    // MOUSE INTERACTION

    void OnMouseDrag()
    {
        FadePixel();
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
    //รับค่าที่คนอื่นปัดมาบวกเข้ากับตัวเอง
    public void ReciveData(float[,] _colorFadeValue, List<Vector2Int> _changedPixelsData)
    {
        canFade = false;
        Add2DArrayToValue(coloraFade, _colorFadeValue);
        foreach (var T in _changedPixelsData)
        {
            FadePixel(T.x, T.y, coloraFade[T.x - 1, T.y - 1]);
        }
        canFade = true;
    }
    //ส่งข้อมูลการปัดตำแหน่งต่างๆ ไป
    public void SendData()
    {
        otherPicture.ReciveData(colorFadeValue, changedPixelsData);
        Set2DArrayToValue(colorFadeValue, 0);
        changedPixelsData.Clear();
    }

    // ส่งค่าจากตัว setup ไปให้อีกรูปนึง
    private void SendSetUP()
    {
        otherPicture.ReciveSetUp(dividePixels, mouseDragRadius, fadeSpeed, pixelateColor, copyTexture.GetPixels());
    }

     // รับค่า setup จากอันอื่น
    public void ReciveSetUp(int _dividePixels, float _mouseDragRadius, float _fadeSpeed, Color[,] _pixelateColorPatturn, Color[] _allColors)
    {
        SetUp();
        dividePixels = _dividePixels;
        mouseDragRadius = _mouseDragRadius;
        fadeSpeed = _fadeSpeed;
        pixelateColor = _pixelateColorPatturn;

        for (int by = 0; by < sizeY; by++)
        {
            for (int bx = 0; bx < sizeX; bx++)
            {
                FadePixel(bx + 1, by + 1, 0);
            }
        }
        copyTexture.Apply();
        UpdateSprite();
    }

}