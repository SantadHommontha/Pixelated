using UnityEngine;

public class SpritePixelHover : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Texture2D texture;

    [SerializeField][Range(0f, 1f)] private float r;
    [SerializeField][Range(0f, 1f)] private float g;
    [SerializeField][Range(0f, 1f)] private float b;
    [SerializeField][Range(0f, 1f)] private float a;

    void Start()
    {
        // 1. รับ SpriteRenderer และ Texture2D
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            texture = spriteRenderer.sprite.texture;
        }
        else
        {
            Debug.LogError("SpriteRenderer not found on this GameObject.");
            enabled = false; // ปิด Script ถ้าไม่มี SpriteRenderer
        }
    }

    void Update()
    {
        // // ใช้ Raycast เพื่อตรวจจับว่าเม้าส์ชนกับ Collider ของ Sprite หรือไม่
        // Vector3 mousePos = Input.mousePosition;
        // Ray ray = Camera.main.ScreenPointToRay(mousePos);
        // RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        // if (hit.collider != null && hit.collider.gameObject == gameObject)
        // {
        //     // เม้าส์ชี้อยู่บน Sprite นี้
        //     Vector2 pixelCoords = GetPixelCoordinates(hit.point);
        //     Debug.Log($"Mouse is hovering at Pixel: ({pixelCoords.x}, {pixelCoords.y})");

        //     // *Optional: เพื่อแสดงสีของพิกเซลที่ชี้*
        //     // Color pixelColor = texture.GetPixel((int)pixelCoords.x, (int)pixelCoords.y);
        //     // Debug.Log($"Pixel Color: {pixelColor}");
        // }
        CreateIMage();
    }
    private void CreateIMage()
    {
        spriteRenderer.color = new Color(r, g, b, a);
    }
    // 3. ฟังก์ชันคำนวณตำแหน่ง Pixel
    Vector2 GetPixelCoordinates(Vector3 worldPos)
    {
        // แปลง World Position เป็น Local Position ของ Sprite
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        // คำนวณตำแหน่งพิกเซลใน Local Space
        Sprite sprite = spriteRenderer.sprite;
        float ppu = sprite.pixelsPerUnit;

        // ตำแหน่งพิกเซลในท้องถิ่น (Local Pixel Position)
        Vector2 localPixelPos = new Vector2(localPos.x * ppu, localPos.y * ppu);

        // หาจุด Pivot ของ Sprite (ในหน่วยพิกเซล)
        Vector2 pivotPixel = sprite.pivot;

        // หาขอบเขตของ Sprite บน Texture (Rect)
        Rect spriteRect = sprite.rect;

        // คำนวณตำแหน่งพิกเซลบน Texture (Texture Space)
        // เริ่มจากจุด (0, 0) ของ Rect ใน Texture แล้วบวกด้วยจุด Pivot และ Local Pixel Position
        Vector2 pixelCoords = new Vector2(
            localPixelPos.x + pivotPixel.x,
            localPixelPos.y + pivotPixel.y
        );

        // ถ้า Sprite เป็นส่วนหนึ่งของ Atlas (Sprite Sheet) ให้ปรับตำแหน่งตาม Rect
        if (sprite.packed)
        {
            pixelCoords.x += spriteRect.x;
            pixelCoords.y += spriteRect.y;
        }

        // จำกัดค่าให้อยู่ในช่วงของ Sprite Rect เพื่อความปลอดภัย
        pixelCoords.x = Mathf.Clamp(pixelCoords.x, spriteRect.x, spriteRect.xMax - 1);
        pixelCoords.y = Mathf.Clamp(pixelCoords.y, spriteRect.y, spriteRect.yMax - 1);


        // แปลงเป็นค่า Int สำหรับตำแหน่งพิกเซล
        return new Vector2(Mathf.FloorToInt(pixelCoords.x), Mathf.FloorToInt(pixelCoords.y));
    }
}
