using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class LineManager : MonoBehaviour
{
    InputManager inputManager;

    GroupData groupData;
    private LineRenderer lineRenderer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask pointLayer;
    [SerializeField] private LayerMask oppenentFortLayer;
    [SerializeField] private LayerMask fortLayer;

    [Header("線の制限設定")]
    [SerializeField] private float minDistance = 1.5f;     // 【カクカク化】値を大きくすると中継点が減ります (例: 1.5〜2.0)
    [SerializeField] private float maxTurnAngle = 60f;     // 【急カーブ禁止】これ以上の急角度（度数法）は曲がれない
    [SerializeField] private float heightOffset = 0.1f;

    private List<Vector3> points = new List<Vector3>();    // 座標の管理用リスト
    private bool isLineInvalid = false;                    // 現在の線が無効かどうか

    bool clickFlag;
    bool startFlag;
    bool farstLayerFlag;

    GameObject lineObject;
    [SerializeField] Material lineMaterial;
    void Start()
    {
        inputManager = InputManager.Instance;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;

        lineObject = new GameObject("Line");
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 4f;
        lineRenderer.endWidth = 4f;
        lineRenderer.material = lineMaterial;
        lineRenderer.textureMode = LineTextureMode.RepeatPerSegment;

    }

    void Update()
    {
        clickFlag = inputManager.click;

        if (clickFlag && !startFlag)
        {
            ResetLine();
            FarstLayer();
            startFlag = true;
        }
        else if (clickFlag && !isLineInvalid && farstLayerFlag)
        {
            AddPointFromMouse();
        }
        else if (!clickFlag) 
        {
            startFlag = false;
            isLineInvalid = false;
            farstLayerFlag = false;
            if (groupData != null)
            {
                groupData.guidLineFlag = true;
                groupData = null;

                // 新しいLineObjectを作成しておく
                lineObject = new GameObject("Line");
                lineRenderer = lineObject.AddComponent<LineRenderer>();
                lineRenderer.startWidth = 4f;
                lineRenderer.endWidth = 4f;
                lineRenderer.material = lineMaterial;
                lineRenderer.textureMode = LineTextureMode.RepeatPerSegment;
            }
        }
    }

    private void ResetLine()
    {
        points.Clear();
    }

    void FarstLayer()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, pointLayer))
        {
            farstLayerFlag = true;
            groupData = hit.collider.GetComponent<GroupData>();

            if (groupData.guidLine != null)
            {
                Destroy(groupData.guidLine);
                groupData.guidLineFlag = false;
            }


            hit.collider.GetComponent<GroupData>().SetGuidLine(lineRenderer);
        }
    }

    IEnumerator SpawnLineAfterDelay(RaycastHit hit)
    {
        // 例: 2秒間待機する（ここに別の条件待ちを入れてもOK）
        yield return new WaitForSeconds(2.0f);

        lineObject = new GameObject("Line");
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 4f;
        lineRenderer.endWidth = 4f;
        lineRenderer.material = lineMaterial;
        lineRenderer.textureMode = LineTextureMode.RepeatPerSegment;

        groupData = hit.collider.GetComponent<GroupData>();
        if (groupData.guidLine != null) Destroy(groupData.guidLine);
        hit.collider.GetComponent<GroupData>().SetGuidLine(lineRenderer);
        Debug.Log("aaa");

    }

    private void AddPointFromMouse()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (lineObject == null) return;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, fortLayer))
        {
            AddNewPoint(hit.collider.gameObject.transform.position);
            isLineInvalid = true;
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 currentPosition = hit.point;
            currentPosition.y += heightOffset;

            // 最初の点
            if (points.Count == 0)
            {
                AddNewPoint(groupData.gameObject.transform.position);
                return;
            }

            // 【制限1】一定の距離（minDistance）以上離れたら、次の点を打つ判定を行う
            if (Vector3.Distance(currentPosition, points[points.Count - 1]) > minDistance)
            {
                // 【制限2】急カーブ（角度）のチェック
                if (points.Count >= 2)
                {
                    Vector3 lastDir = (points[points.Count - 1] - points[points.Count - 2]).normalized;
                    Vector3 newDir = (currentPosition - points[points.Count - 1]).normalized;
                    float angle = Vector3.Angle(lastDir, newDir);

                    if (angle > maxTurnAngle) return; // 急カーブならこの点は無視する
                }

                // 【制限3】線が自分自身と重なって（交差して）いないかチェック
                if (CheckSelfIntersection(points[points.Count - 1], currentPosition))
                {
                    InvalidateLine(); // 重なったら線を無効化して終了
                    return;
                }

                AddNewPoint(currentPosition);
            }
        }

    }

    private void AddNewPoint(Vector3 pos)
    {
        points.Add(pos);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(points.Count - 1, pos);
    }

    // 線が自分自身と交差しているかを2D平面（XZ平面）で数学的に判定
    private bool CheckSelfIntersection(Vector3 p1, Vector3 p2)
    {
        if (points.Count < 3) return false;

        // 今回引こうとしている線分(p1 - p2)が、過去の線分(points[i] - points[i+1])と交差するか調べる
        // ※隣り合う線分同士は必ず繋がっているので、points.Count - 2 まで調べる
        for (int i = 0; i < points.Count - 2; i++)
        {
            if (AreLinesIntersecting(p1, p2, points[i], points[i + 1]))
            {
                return true;
            }
        }
        return false;
    }

    // 線分の交差判定（外積を利用）
    private bool AreLinesIntersecting(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        // XZ平面（地面）での判定にするため、Yを無視
        float cross1 = (b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x);
        float cross2 = (b.x - a.x) * (d.z - a.z) - (b.z - a.z) * (d.x - a.x);
        float cross3 = (d.x - c.x) * (a.z - c.z) - (d.z - c.z) * (a.x - c.x);
        float cross4 = (d.x - c.x) * (b.z - c.z) - (d.z - c.z) * (b.x - c.x);

        return (cross1 * cross2 < 0) && (cross3 * cross4 < 0);
    }

    // 線を赤くして「無効」であることを分かりやすくし、入力をストップする
    private void InvalidateLine()
    {
        isLineInvalid = true;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        // ※実際の運用では、ここでルートを全消去（ResetLine）しても良いです
        ResetLine();
    }
}