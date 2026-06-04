using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PendulumTest : MonoBehaviour
{
    public Transform bob;

    [Header("Pendulum Settings")]
    public float length = 5f;
    public float initialAngle = 30f; // ìx

    private float angle;
    private float angularFrequency;

    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();

        angle = initialAngle * Mathf.Deg2Rad;

        // É÷ = Å„(g/L)
        angularFrequency = Mathf.Sqrt(9.81f / length);
    }

    void Update()
    {
        float currentAngle =
            angle * Mathf.Cos(angularFrequency * Time.time);

        Vector3 bobPos = transform.position +
                         new Vector3(
                             Mathf.Sin(currentAngle) * length,
                            -Mathf.Cos(currentAngle) * length,
                             0);

        bob.position = bobPos;

        line.positionCount = 2;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, bobPos);
    }
}