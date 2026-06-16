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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        inputManager = InputManager.Instance;
    }

    void Update()
    {
        UpdateInputVariable();
        PlayerControl();
        DebugLog();
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
        velocity = new Vector3(move.x, 0f, move.y) * moveSpeed * Time.deltaTime;

        // 位置を更新
        rb.linearVelocity += velocity;

        IsGround();
        if (space && isGround) jump = true;
        if (jump)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jump = false;
        }
    }

    bool IsGround()
    {
        Ray ray = new Ray(transform.position, -Vector2.up);
        RaycastHit hit;
        if (Physics.Raycast(ray.origin, ray.direction, out hit, 1f))
        {
            isGround = true;
        }
        else isGround = false;
        return isGround;
    }

    void DebugLog()
    {

    }
}
