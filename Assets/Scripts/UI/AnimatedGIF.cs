using UnityEngine;
using UnityEngine.UI;

public class AnimatedGIF : MonoBehaviour
{
    public Sprite[] frames;
    public float FPS = 10f;

    private Image image;
    private int currentFrame = 0;
    private float timer = 0f;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        float interval = 1f / FPS;

        if (timer >= interval)
        {
            timer -= interval;
            currentFrame = (currentFrame + 1) % frames.Length;

            if (image != null && frames[currentFrame] != null)
                image.sprite = frames[currentFrame];
        }
    }
}