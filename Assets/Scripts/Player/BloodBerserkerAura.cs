using UnityEngine;

public class BloodBerserkerAura : MonoBehaviour
{
    public static BloodBerserkerAura Instance { get; private set; }

    [Header("Aura Settings")]
    [SerializeField] private GameObject auraPrefab;
    [SerializeField] private float auraSize = 1.5f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private Color auraColor = new Color(1f, 0f, 0f, 0.8f);

    private GameObject auraObject;
    private SpriteRenderer auraSprite;
    private Transform playerTransform;
    private float baseSize;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerTransform = transform;
        CreateAura();
    }

    private void CreateAura()
    {
        auraObject = new GameObject("BloodBerserkerAura");
        auraObject.transform.SetParent(playerTransform);
        auraObject.transform.localPosition = new Vector3(0, 0, 0.1f);

        auraSprite = auraObject.AddComponent<SpriteRenderer>();
        auraSprite.sprite = CreateCircleSprite();
        auraSprite.color = auraColor;
        auraSprite.sortingOrder = 4;
        auraSprite.material = new Material(Shader.Find("Sprites/Default"));

        auraObject.transform.localScale = Vector3.one * auraSize;
        auraObject.SetActive(false);
    }

    private Sprite CreateCircleSprite()
    {
        return Sprite.Create(
            CreateCircleTexture(256),
            new Rect(0, 0, 256, 256),
            new Vector2(0.5f, 0.5f)
        );
    }

    private Texture2D CreateCircleTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < radius)
                {
                    float alpha = 1f - (dist / radius);
                    alpha = Mathf.Pow(alpha, 0.5f);
                    colors[y * size + x] = new Color(1f, 0.1f, 0.1f, alpha);
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    private void Update()
    {
        if (auraObject == null || !auraObject.activeSelf) return;

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.15f;
        auraObject.transform.localScale = Vector3.one * auraSize * pulse;
    }

    public void SetActive(bool active)
    {
        if (auraObject != null)
        {
            auraObject.SetActive(active);
            if (active && auraSprite != null)
                auraSprite.color = new Color(1f, 0f, 0f, 0.7f);
        }
    }
}