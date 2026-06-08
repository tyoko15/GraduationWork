using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class S_PlayerControl : MonoBehaviour
{
    [Header("ロープの参照")]
    [SerializeField] private S_Rope currentRope;

    [Header("パラメーター調整")]
    [SerializeField] private float swingForce = 5f;
    [SerializeField] private float jumpForceX = 8f;
    [SerializeField] private float jumpForceY = 12f;

    [Header("スライド（滑り落ち）設定")]
    [SerializeField] private float slideSpeed = 5f;

    [Header("お助け機能（急降下）設定")]
    [SerializeField] private float dropGravityMultiplier = 3f;

    [Header("空中での振り向き設定")]
    [SerializeField] private float turnSpeed = 720f;

    private Rigidbody rb;
    private bool isAttached = false;
    private float moveInputX = 0f;
    private float moveInputY = 0f;
    private float playerLengthOnRope = 0f;
    private bool isDropping = false;

    private Quaternion targetRotation = Quaternion.identity;

    [SerializeField] Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (currentRope != null)
        {
            AttachToRope(currentRope, transform.position);
        }
    }

    public void OnMove(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();
        moveInputX = inputVector.x;
        moveInputY = inputVector.y;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isAttached)
        {
            DetachAndJump();
        }
    }

    void Update()
    {
        if (isAttached && currentRope != null)
        {
            animator.SetBool("Air", false);
            isDropping = false;

            if (playerLengthOnRope < currentRope.MaxLength)
            {
                playerLengthOnRope += slideSpeed * Time.deltaTime;
                playerLengthOnRope = Mathf.Min(playerLengthOnRope, currentRope.MaxLength);
            }

            currentRope.CurrentLength = playerLengthOnRope;

            transform.position = currentRope.GetPositionAtLength(playerLengthOnRope);

            // ★ロープの傾きに合わせる（Y軸180度問題に対応）
            AlignRotationToRope();

            currentRope.ApplySwingInput(moveInputX, swingForce, playerLengthOnRope);
        }
        else
        {
            HandleAirborneRotation();

            if (moveInputY < -0.5f && !isDropping)
            {
                TriggerEmergencyDrop();
            }
        }
    }

    void FixedUpdate()
    {
        if (!isAttached && isDropping)
        {
            rb.AddForce(Physics.gravity * (dropGravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    // ★修正: ロープの角度に合わせてプレイヤーの傾きを計算する関数
    private void AlignRotationToRope()
    {
        Vector3 ropeDirection = transform.position - currentRope.transform.position;

        if (ropeDirection.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(ropeDirection.y, ropeDirection.x) * Mathf.Rad2Deg;
            float targetZAngle = angle + 90f;

            // 現在のプレイヤーの向き（Y軸）を取得
            float currentYAngle = transform.rotation.eulerAngles.y;

            // Unityのオイラー角は内部的に「180」が「-180」や「360」付近になることがあるため、大まかに判定
            bool isFacingLeft = Mathf.Abs(Mathf.DeltaAngle(currentYAngle, 180f)) < 45f;

            if (isFacingLeft)
            {
                // 左を向いている（Y=180）時は、Z軸の回転方向（符号）を逆にしないと
                // モデルがひっくり返ったり、逆方向に傾いてしまいます。
                transform.rotation = Quaternion.Euler(0f, 180f, -targetZAngle);
            }
            else
            {
                // 右を向いている（Y=0）時
                transform.rotation = Quaternion.Euler(0f, 0f, targetZAngle);
            }
        }
    }

    private void HandleAirborneRotation()
    {
        if (moveInputX > 0.1f)
        {
            targetRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (moveInputX < -0.1f)
        {
            targetRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    private void ResetRotation(float targetYAngle)
    {
        targetRotation = Quaternion.Euler(0f, targetYAngle, 0f);
        transform.rotation = targetRotation;
    }

    private void TriggerEmergencyDrop()
    {
        isDropping = true;
        ResetRotation(transform.rotation.eulerAngles.y);

        float currentVy = rb.linearVelocity.y;
        if (currentVy > 0f) currentVy = 0f;

        rb.linearVelocity = new Vector3(0f, currentVy, 0f);
    }

    public void AttachToRope(S_Rope targetRope, Vector3 contactPoint)
    {
        currentRope = targetRope;
        isAttached = true;
        isDropping = false;

        float incomingVx = rb.linearVelocity.x;
        rb.isKinematic = true;

        float distanceToAnchor = Vector3.Distance(targetRope.transform.position, contactPoint);
        playerLengthOnRope = Mathf.Clamp(distanceToAnchor, 0.5f, targetRope.MaxLength);
        currentRope.CurrentLength = playerLengthOnRope;

        currentRope.InheritVelocity(incomingVx, playerLengthOnRope);

        // ★追加: 掴んだ瞬間の速度（方向）を見て、ロープ上での初期向きを決定する
        if (incomingVx < -0.1f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else if (incomingVx > 0.1f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        AlignRotationToRope();
    }

    private void DetachAndJump()
    {
        animator.SetBool("Air", true);
        isAttached = false;
        rb.isKinematic = false;

        Vector3 launchVelocity = new Vector3(
            (moveInputX * jumpForceX),
            jumpForceY,
            0f
        );

        rb.linearVelocity = launchVelocity;
        currentRope = null;

        float initialYAngle = moveInputX < 0f ? 180f : 0f;
        ResetRotation(initialYAngle);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isAttached)
        {
            S_Rope target = other.GetComponentInParent<S_Rope>();
            if (target != null)
            {
                AttachToRope(target, transform.position);
            }
        }
    }
}