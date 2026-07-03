using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Type
{
    RaccoonDog,
    Fox,
    Mole
}


public enum Indication
{
    Waiting,
    Moving,
    Sortieing,
    Intercepting,

}

[System.Serializable]
public class Group
{
    public Team team;
    public int number;
    public Type type;
    public Indication indication;
    public int soldierAmount;
}


public class GroupData : MonoBehaviour
{
    public Group info = new Group();

    TextMeshProUGUI soldierAmountText;
    Renderer leaderRenderer;
    [SerializeField] Texture[] leaderTexs;
    GameObject filterObject;
    public float speed = 3f;
    public LineRenderer guidLine;
    public bool guidLineFlag;

    private void Start()
    {
        soldierAmountText = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        leaderRenderer = transform.GetChild(1).GetComponent<Renderer>();
        filterObject = transform.GetChild(2).gameObject;
    }

    private void Update()
    {
        int i = (info.type == Type.RaccoonDog) ? 0 : (info.type == Type.Fox) ? 1 : 2;
        soldierAmountText.text = $"{info.soldierAmount}";
        leaderRenderer.material.SetTexture("_BaseMap", leaderTexs[i]);

        if (guidLine != null && guidLineFlag)
        {
            Vector3 target = guidLine.GetPosition(0);

            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                if (guidLine.positionCount > 1) RemovePoint(0);
                else
                {
                    Destroy(guidLine.gameObject);
                    guidLine = null;
                }
            }
        }
    }
    public void RemovePoint(int index)
    {
        if (index < 0 || index >= guidLine.positionCount)
            return;

        Vector3[] points = new Vector3[guidLine.positionCount - 1];

        int j = 0;
        for (int i = 0; i < guidLine.positionCount; i++)
        {
            if (i == index)
                continue;

            points[j++] = guidLine.GetPosition(i);
        }

        guidLine.positionCount = points.Length;
        guidLine.SetPositions(points);
    }
    public void ActiveFilterObject(bool flag)
    {
        filterObject.SetActive(flag);
    }

    public void SetGuidLine(LineRenderer line)
    {
        guidLine = line;
    }
}
