using UnityEngine;
namespace no
{
    [RequireComponent(typeof(LineRenderer))]
    public class SwingLine : MonoBehaviour
    {
        public int segments = 20;

        public float length = 5f;
        public float angle = 90f;
        public float speed = 1f;

        // êÊí[Ç™Ç«ÇÍÇ≠ÇÁÇ¢íxÇÍÇÈÇ©
        public float delay = 0.2f;

        private LineRenderer line;

        [SerializeField] GameObject ob;

        void Start()
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = segments;
        }

        void Update()
        {
            Vector3 startPos = transform.position;

            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)(segments - 1);

                // ç™å≥ÇŸÇ«åªç›éûçè
                // êÊí[ÇŸÇ«âﬂãéÇÃéûçè
                float delayedTime = Time.time - t * delay;

                float currentAngle =
                    Mathf.Sin(delayedTime * speed) * angle;

                float rad = currentAngle * Mathf.Deg2Rad;

                float segmentLength = length * t;

                Vector3 pos = startPos + new Vector3(
                    Mathf.Sin(rad) * segmentLength,
                    -Mathf.Cos(rad) * segmentLength,
                    0f
                );

                line.SetPosition(i, pos);

            }

            ob.transform.localPosition = line.GetPosition(line.positionCount - 1);
        }
    }
}