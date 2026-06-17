
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /* Input変数 */
    InputManager inputManager;
    Vector2 move;
    bool space;

    /* player行動制御変数 */
    Rigidbody rb;
    [SerializeField] float moveSpeed;
    bool jump;
    bool isGround;
    [SerializeField] float jumpForce;
    float radius = 0.25f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        inputManager = InputManager.Instance;
        radius = 0.25f;
    }

    void Update()
    {
        UpdateInputVariable();
        PlayerControl();
    }

    void UpdateInputVariable()
    {
        // InputManagerの変数を取得
        move = inputManager.move;
        space = inputManager.jump;
    }

    void PlayerControl()
    {
        Vector3 velocity = Vector3.zero;
        // 移動位置を計算
        velocity = new Vector3(move.x * moveSpeed, rb.linearVelocity.y, move.y * moveSpeed);



        IsGround();
        if (space && isGround) jump = true;
        if (jump)
        {
            float jumpVelocity = Mathf.Sqrt(8f * Mathf.Abs(Physics.gravity.y) * jumpForce);
            rb.AddForce(Vector3.up * jumpForce * Time.deltaTime, ForceMode.Impulse);
            //velocity.y = jumpVelocity;
            jump = false;
        }
        else velocity.y = -4f;

        // 位置を更新
        rb.linearVelocity = velocity;
    }

    bool IsGround()
    {
        Ray ray = new Ray(transform.position, Vector2.down);
        RaycastHit hit;
        if (Physics.SphereCast(ray.origin, 0.25f, ray.direction, out hit, 0.1f))
        {
            isGround = true;
        }
        else isGround = false;
        return isGround;
    }

    private void OnDrawGizmos()
    {
        // ギズモの色を決定（接地していたら緑、浮いていたら赤）
        Gizmos.color = isGround ? Color.green : Color.red;

        // 開始地点（球体の初期位置）
        Vector3 origin = transform.position + Vector3.up * radius;
        // 終了地点（球体が最下点に到達した位置）
        Vector3 targetPosition = origin + Vector3.down * 1;

        // 1. 開始地点にワイヤー球体を描画
        Gizmos.DrawWireSphere(origin, radius);

        // 2. 終了地点にワイヤー球体を描画
        Gizmos.DrawWireSphere(targetPosition, radius);

        // 3. 開始地点と終了地点をつなぐ線を描画
        Gizmos.DrawLine(origin, targetPosition);
    }
}
