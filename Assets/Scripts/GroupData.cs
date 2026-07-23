using TMPro;
using UnityEngine;

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
    GameObject arrowObject;
    public float speed = 3f;
    public LineRenderer guidLine;
    public bool guidLineFlag;

    FortController attackFort;
    bool fortAttackFlag;
    float repeatTimer;
    float repeatTime = 2f;

    GameObject[] sizes = new GameObject[3];

    private void Start()
    {
        soldierAmountText = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        leaderRenderer = transform.GetChild(1).GetChild(0).GetComponent<Renderer>();
        filterObject = transform.GetChild(1).GetChild(1).gameObject;
        for (int i = 0; i < sizes.Length; i++)
        {
            sizes[i] = transform.GetChild(2 + i).gameObject;
            sizes[i].SetActive(false);
        }
        switch (info.soldierAmount)
        {
            case <= 10:
                sizes[0].SetActive(true);
                break;
            case <= 20:
                sizes[1].SetActive(true);
                break;
            case <= 30:
                sizes[2].SetActive(true);
                break;
        }
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

        if (fortAttackFlag)
        {
            if (attackFort.hp <= 0 || attackFort.state == info.team)
            {
                attackFort = null;
                fortAttackFlag = false;
            }
            if (repeatTimer > repeatTime)
            {
                repeatTimer = 0f;
                if (attackFort.hp > 0) attackFort.TakeDamage(info.soldierAmount);
                else
                {
                    attackFort = null;
                    fortAttackFlag = false;
                }
            }
            else repeatTimer += Time.deltaTime;
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
        int groupLayer = (info.team == Team.Ally) ?  8 : 7;
        int fortLayer = (info.team == Team.Ally) ? 11 : 10;
        if (other.gameObject.layer == groupLayer)
        {
            GroupData groupData = other.GetComponent<GroupData>();
            Type self = info.type;
            Type opponent = groupData.info.type;
            int damageAmount =  (int)(10*TypeMatchup(self, opponent));
            other.GetComponent<GroupData>().TakeDamage(damageAmount);
            if (guidLine != null) guidLine.gameObject.SetActive(false);
            guidLineFlag = false;
        }
        else if (other.gameObject.layer == 9 || other.gameObject.layer == fortLayer)
        {
            attackFort = other.GetComponent<FortController>();
            if (attackFort.state != info.team)
            {
                attackFort.TakeDamage(info.soldierAmount);
                attackFort.attackedTeam = info.team;
                fortAttackFlag = true;
                if (guidLine != null) guidLine.gameObject.SetActive(false);
                guidLineFlag = false;
            }
            else attackFort = null;
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
