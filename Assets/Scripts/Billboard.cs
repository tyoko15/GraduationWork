using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;
    [SerializeField] bool castleFlag;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    // カメラの移動（Update）に追従させるため、LateUpdateへの変更をおすすめします
    void LateUpdate()
    {
        if (mainCamera == null) return;


        // 1. カメラの現在の回転（オイラー角）を取得
        Vector3 cameraRotation = mainCamera.transform.rotation.eulerAngles;

        // 2. X軸だけカメラの傾きを合わせ、YとZは0（または現在のキャンバスの回転値）にする
        // もしキャンバスの初期のY軸回転などを維持したい場合は、0fの代わりに transform.localEulerAngles.y を使ってください
        Vector3 targetRotation = new Vector3(cameraRotation.x, 0f, 0f);

        if (castleFlag) targetRotation.y = 90f;

        // 3. クォータニオンに変換して適用
        transform.rotation = Quaternion.Euler(targetRotation);
    }
}