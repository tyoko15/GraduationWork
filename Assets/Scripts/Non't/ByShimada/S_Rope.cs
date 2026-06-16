using UnityEngine;
namespace no
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(CapsuleCollider))] // 自動でカプセルコライダーを必須にする
    public class S_Rope : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private CapsuleCollider capsuleCollider;

        [Header("ロープの設定")]
        [SerializeField] private float maxLength = 5f;       // ロープの最大値（本来の長さ）
        [SerializeField] private float gravity = 9.81f;    // 重力加速度
        [SerializeField] private float airResistance = 0.5f; // 空気抵抗
        [SerializeField] private float ropeRadius = 0.3f;    // ロープの当たり判定の太さ

        private float angle = 0f;
        private float angularVelocity = 0f;

        public float CurrentLength { get; set; }
        public float MaxLength => maxLength;

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            capsuleCollider = GetComponent<CapsuleCollider>();

            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;

            CurrentLength = maxLength;

            // ★コライダーの自動初期化
            SetupCapsuleCollider();

            UpdateRopePositions();
        }

        // ★ロープの長さに合わせてコライダーのサイズと位置を自動調整する関数
        private void SetupCapsuleCollider()
        {
            capsuleCollider.isTrigger = true;
            capsuleCollider.direction = 1; // 1 = Y軸方向（縦長）に設定
            capsuleCollider.radius = ropeRadius;

            // 高さはロープの最大長（maxLength）に合わせる
            capsuleCollider.height = maxLength;

            // コライダーの中心点をロープの中心（真下へ半分進んだ位置）にずらす
            // これにより、ロープの根元(0)から先端(-maxLength)まで綺麗にコライダーが覆う
            capsuleCollider.center = new Vector3(0f, -maxLength / 2f, 0f);
        }

        void Update()
        {
            float dt = Time.deltaTime;

            float angularAcceleration = -(gravity / CurrentLength) * Mathf.Sin(angle);

            angularVelocity += angularAcceleration * dt;
            angularVelocity *= (1f - airResistance * dt);
            angle += angularVelocity * dt;

            UpdateRopePositions();
        }

        private void UpdateRopePositions()
        {
            Vector3 startPos = transform.position;
            Vector3 maxOffset = new Vector3(Mathf.Sin(angle) * maxLength, -Mathf.Cos(angle) * maxLength, 0f);
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, startPos + maxOffset);
        }

        public Vector3 GetPositionAtLength(float length)
        {
            return transform.position + new Vector3(
                Mathf.Sin(angle) * length,
                -Mathf.Cos(angle) * length,
                0f
            );
        }

        // ★テコの原理を取り入れた勢いの継承関数
        public void InheritVelocity(float incomingVelocityX, float grabLength)
        {
            // レバー比（比率）を計算：掴んだ位置 ÷ 最大の長さ
            // 根元（0m）なら 0、 先端（maxLength）なら 1 になる
            float leverRatio = grabLength / maxLength;

            // 【テコの原理】先端に近い（1に近い）ほど力がそのまま伝わり、上（0に近い）ほど弱くなる
            float effectiveVelocity = incomingVelocityX * leverRatio;

            // 速度から角速度への変換
            float addedAngularVelocity = effectiveVelocity / grabLength;

            angularVelocity += addedAngularVelocity * 0.8f;
        }

        // ★ADでの加速時にもテコの原理（掴んでいる高さ）を適応させる
        public void ApplySwingInput(float inputHorizontal, float swingForce, float grabLength)
        {
            if (Mathf.Abs(inputHorizontal) < 0.01f) return;

            // 入力時も同様に、上の方を掴んでいるときは操作の力を弱くする
            float leverRatio = grabLength / maxLength;
            float effectiveForce = swingForce * leverRatio;

            if (Mathf.Sign(angularVelocity) == Mathf.Sign(inputHorizontal) && Mathf.Abs(angularVelocity) > 0.05f)
            {
                angularVelocity += (inputHorizontal * effectiveForce / grabLength) * Time.deltaTime;
            }
            else if (Mathf.Abs(angularVelocity) <= 0.05f)
            {
                angularVelocity += (inputHorizontal * effectiveForce * 0.5f / grabLength) * Time.deltaTime;
            }
        }
    }
}