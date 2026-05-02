using UnityEngine;

public class FloatEffect : MonoBehaviour
{
    [Header("Levitation Settings")]
    [SerializeField] private float floatSpeed = 0.5f;
    [SerializeField] private float floatHeight = 5f;

    private float startY;

    void Start()
    {
        startY = transform.localPosition.y;
    }

    void Update()
    {
        float newY = startY + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
    }
}
