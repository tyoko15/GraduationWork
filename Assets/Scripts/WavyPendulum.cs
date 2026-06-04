using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WiperLine : MonoBehaviour
{
    public int segments = 20;
    public float length = 5f;
    public float amplitude = 0.5f;
    public float speed = 2f;

    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = segments;
    }

    void Update()
    {
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);

            float x =
                Mathf.Sin(Time.time * speed + t * 2f)
                * amplitude * t;

            float y = -length * t;

            line.SetPosition(i, transform.position + new Vector3(x, y, 0));
        }
    }
}