using TMPro;
using UnityEditor.Experimental.GraphView;
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


public class GroupData : MonoBehaviour, IDamage
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
                    guidLineFlag = false;
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

    private void OnTriggerEnter(Collider other)
    {
        int layer = (info.team == Team.Ally) ?  8 : 7;
        if (other.gameObject.layer == layer)
        {
            GroupData groupData = other.GetComponent<GroupData>();
            Type self = info.type;
            Type opponent = groupData.info.type;
            int damageAmount =  (int)(10*TypeMatchup(self, opponent));
            other.GetComponent<GroupData>().TakeDamage(damageAmount);
            if (guidLine != null) guidLine.gameObject.SetActive(false);
            guidLineFlag = false;
        }
    }

    public void TakeDamage(int damage)
    {

        info.soldierAmount -= damage;
        if (info.soldierAmount <= 0)
        {
            Destroy(gameObject);
        }
    }

    float TypeMatchup(Type self, Type opponent)
    {
        return (self, opponent) switch
        {
            (Type.RaccoonDog,   Type.Mole) => 1.5f,
            (Type.Mole,         Type.Fox) => 1.5f,
            (Type.Fox,          Type.RaccoonDog) => 1.5f,

            (Type.RaccoonDog, Type.Fox) => 0.5f,
            (Type.Fox, Type.Mole) => 0.5f,
            (Type.Mole, Type.RaccoonDog) => 0.5f,

            _=> 1f
        };
    }
}
