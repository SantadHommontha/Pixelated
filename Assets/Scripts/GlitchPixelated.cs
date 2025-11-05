using System.Collections.Generic;

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
    public HashSet<Vector2Int> changedPixels = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> restoredPixels = new HashSet<Vector2Int>();
    // Shared random pattern for consistent pixelation between images
    private static Color[,] sharedPixelPattern;

    private Color[,] pixelateColor; //สีในแต่ะ grid
    public List<Vector2Int> changedPixelsData = new List<Vector2Int>(); //เก็บว่ามี grid ใหนเปี่ยนบ้าง

    private float[,] coloraFade; //ค่า t ในการใช้ lerp
    public Color[] colortest;
    public List<Color> colortest2;
    private int width => originalTexture.width;
    private int height => originalTexture.height;
    private Collider2D collider;
    private int sizeX, sizeY;
    public bool isMainSprite;
    void Start()
    {
        cameraMain = Camera.main;
        SetUp();
    }

    private void SetUp()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        originalTexture = spriteRenderer.sprite.texture;

        copyTexture = new Texture2D(originalTexture.width, originalTexture.height);
        copyTexture.SetPixels(originalTexture.GetPixels());
        copyTexture.Apply();

        // int width = originalTexture.width;
        // int height = originalTexture.height;

        sizeX = Mathf.CeilToInt(width / (float)dividePixels);
        sizeY = Mathf.CeilToInt(height / (float)dividePixels);

    }

    void Update()
    {
        rayCastHit = Physics2D.Raycast(cameraMain.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (Input.GetKeyDown(KeyCode.Q) && isMainSprite)
            PixelatedMethod();

        // if (Input.GetKeyDown(KeyCode.E))
        //     UpdateSprite(originalTexture);
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (otherPicture != null)
                SendSetUP();
        }


        // FindPixelSeclcet();
        if (Input.GetKeyDown(KeyCode.R) && otherPicture != null)
            CombineFromOther(otherPicture);

        if (Input.GetKeyDown(KeyCode.U))
            Debug.Log($"Seclet {FindPixelSeclcet()}");
    }

    // PIXELATION

    void PixelatedMethod()
    {

        if (dividePixels <= 0)
            dividePixels = 1;

        Debug.Log("--------------");

        if (sharedPixelPattern == null ||
            sharedPixelPattern.GetLength(0) != sizeX ||
            sharedPixelPattern.GetLength(1) != sizeY)
        {
            sharedPixelPattern = new Color[sizeX, sizeY];
            coloraFade = new float[sizeX, sizeY];
            pixelateColor = new Color[sizeX, sizeY];
            for (int by = 0; by < sizeY; by++)
            {
                for (int bx = 0; bx < sizeX; bx++)
                {
                    float r = Random.Range(0.2f, 0.6f);
                    float g = Random.Range(0f, 0.3f);
                    float b = Random.Range(0.4f, 1f);
                    float a = 1f;
                    sharedPixelPattern[bx, by] = new Color(r, g, b, a);
                    pixelateColor[bx, by] = new Color(r, g, b, a);
                    coloraFade[bx, by] = 0;
                    colortest2.Add(new Color(r, g, b, a));
                }
            }



            for (int y = 0; y < height; y += dividePixels)
            {
                for (int x = 0; x < width; x += dividePixels)
                {
                    int bx = x / dividePixels;
                    int by = y / dividePixels;
                    Color pixelColor = sharedPixelPattern[bx, by];

                    for (int dy = 0; dy < dividePixels; dy++)
                    {
                        for (int dx = 0; dx < dividePixels; dx++)
                        {
                            int px = x + dx;
                            int py = y + dy;

                            if (px < width && py < height)
                            {
                                //      Debug.Log("__");
                                copyTexture.SetPixel(px, py, pixelColor);
                                changedPixels.Add(new Vector2Int(px, py));

                            }
                        }
                    }
                }
            }




            copyTexture.Apply();
            UpdateSprite();
        }
    }
    private void SetPixelated()
    {





    }
    void UpdateSprite(Texture2D texture = null)
    {
        if (texture == null)
            texture = copyTexture;

        spriteRenderer.sprite = Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));
    }
    private void FadePixel(int _px, int _py)
    {
        Vector2Int pxIndex = new Vector2Int(_px - 1, _py - 1);
        var startX = pxIndex.x * dividePixels;
        var startY = pxIndex.y * dividePixels;


        int blockwidth = startX + dividePixels >= originalTexture.width ? blockwidth = originalTexture.width - startX : blockwidth = dividePixels;
        int blockHigth = startY + dividePixels >= originalTexture.height ? blockHigth = originalTexture.height - startY : blockHigth = dividePixels;

        var originalColors = originalTexture.GetPixels(startX, startY, blockwidth, blockHigth);
        colortest = new Color[originalColors.Length];
        colortest = originalColors;
        var currentColor = pixelateColor[pxIndex.x, pxIndex.y];
        var newColors = new Color[originalColors.Length];
        var currentFadeValue = (coloraFade[pxIndex.x, pxIndex.y] += fadeSpeed);
        for (int i = 0; i < originalColors.Length; i++)
        {
            newColors[i] = Color.Lerp(
                currentColor,
                originalColors[i],
                currentFadeValue
            );
        }
        SetColor(startX, startY, blockwidth, blockHigth, newColors);
    }

    private void FadePixel()
    {
        if (rayCastHit.collider != collider) return;
        var fps = FindPixelSeclcet();
        FadePixel(fps.x, fps.y);

        var changeIndex = new Vector2Int(fps.x, fps.y);
        if (!changedPixelsData.Contains(changeIndex))
            changedPixelsData.Add(changeIndex);

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
    }

    private void SetColor(int _startPx, int _StartPy, int _blockWidth, int blockHight, Color[] _color)
    {
        copyTexture.SetPixels(_startPx, _StartPy, _blockWidth, blockHight, _color);
        copyTexture.Apply();
        UpdateSprite();
    }
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

    //ตอนปัด
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

    public void CombineFromOther(GlitchPixelated other)
    {
        if (other == null || other.copyTexture == null) return;

        int w = copyTexture.width;
        int h = copyTexture.height;

        HashSet<Vector2Int> allChanged = new HashSet<Vector2Int>(changedPixels);
        foreach (var pos in other.changedPixels)
            allChanged.Add(pos);

        foreach (Vector2Int pos in allChanged)
        {
            if (pos.x < 0 || pos.y < 0 || pos.x >= w || pos.y >= h)
                continue;

            bool mineChanged = changedPixels.Contains(pos);
            bool otherChanged = other.changedPixels.Contains(pos);

            //Skip combining if both are already clean (restored)
            if (!mineChanged && !otherChanged)
            {
                Color originalColor = originalTexture.GetPixel(pos.x, pos.y);
                copyTexture.SetPixel(pos.x, pos.y, originalColor);
                continue;
            }

            // If one is restored but not the other, keep original
            if (restoredPixels.Contains(pos) || other.restoredPixels.Contains(pos))
            {
                Color originalColor = originalTexture.GetPixel(pos.x, pos.y);
                copyTexture.SetPixel(pos.x, pos.y, originalColor);
                restoredPixels.Add(pos);
                changedPixels.Remove(pos);
                continue;
            }

            Color myColor = copyTexture.GetPixel(pos.x, pos.y);
            Color otherColor = other.copyTexture.GetPixel(pos.x, pos.y);
            Color finalColor;

            if (mineChanged && otherChanged)
                finalColor = Color.Lerp(myColor, otherColor, 0.5f);
            else if (otherChanged)
                finalColor = otherColor;
            else
                finalColor = myColor;

            copyTexture.SetPixel(pos.x, pos.y, finalColor);
        }

        copyTexture.Apply();
        UpdateSprite();

        changedPixels = allChanged;

        foreach (var pos in other.restoredPixels)
            restoredPixels.Add(pos);

        other.changedPixels.Clear();
    }

    public void ReciveData(List<Vector2Int> _changedPixelsData, float[,] _colorFade)
    {

    }
    public void SendData()
    {

    }
    private void SendSetUP()
    {
        otherPicture.ReciveSetUp(dividePixels, mouseDragRadius, fadeSpeed, pixelateColor, copyTexture.GetPixels());
    }
    public void ReciveSetUp(int _dividePixels, float _mouseDragRadius, float _fadeSpeed, Color[,] _pixelateColor, Color[] _colors)
    {
        dividePixels = _dividePixels;
        mouseDragRadius = _mouseDragRadius;
        fadeSpeed = _fadeSpeed;
        pixelateColor = _pixelateColor;
        SetColor(0, 0, originalTexture.width, originalTexture.height, _colors);
    }
    public int px;
    public int py;
    [ContextMenu("Test")]
    public void TestSetcolod()
    {
        FadePixel(px, py);
    }
}