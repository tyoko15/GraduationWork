using Unity.VisualScripting;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    [SerializeField] CastleController[] castles;
    [SerializeField] FortController[] forts;

    Camera mainCamera;
    [SerializeField] Transform[] cameraPoints;

    [SerializeField] GameObject[] uis;

    [SerializeField] int a;
    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        UpdateCameraPoint(a);
    }

    void UpdateCameraPoint(int p)
    {      
        bool basicUIFlag;
        // カメラの設定を変更
        switch (p)
        {
            case 0:
            default:
                p = 0;
                mainCamera.rect = new Rect(0f, 0.3f, 1f, 0.7f);
                basicUIFlag = true;
                break;
            case 1:
                mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
                basicUIFlag = false;
                break;
        }
        // カメラの位置と回転を変更
        mainCamera.transform.position = cameraPoints[p].position;
        mainCamera.transform.eulerAngles = cameraPoints[p].eulerAngles;
        // UIを更新
        uis[0].transform.parent.gameObject.SetActive(basicUIFlag);
    }
}
