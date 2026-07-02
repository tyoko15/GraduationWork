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
    LineRenderer leaderLine;
    public float speed = 3f;
    public bool letgo = false;
    private int currentPoint = 0;

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

        if (leaderLine != null && letgo)
        {
            if (currentPoint >= leaderLine.positionCount)
                return;

            Vector3 target = leaderLine.GetPosition(0);

            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                if (leaderLine.positionCount > 1) RemovePoint(0);
                else
                {
                    Destroy(leaderLine.gameObject);
                    leaderLine = null;
                }
            }
        }
    }
    public void RemovePoint(int index)
    {
        if (index < 0 || index >= leaderLine.positionCount)
            return;

        Vector3[] points = new Vector3[leaderLine.positionCount - 1];

        int j = 0;
        for (int i = 0; i < leaderLine.positionCount; i++)
        {
            if (i == index)
                continue;

            points[j++] = leaderLine.GetPosition(i);
        }

        leaderLine.positionCount = points.Length;
        leaderLine.SetPositions(points);
    }
    public void ActiveFilterObject(bool flag)
    {
        filterObject.SetActive(flag);
    }


    public void GetLine(LineRenderer line)
    {
        leaderLine = line;
    }
}
