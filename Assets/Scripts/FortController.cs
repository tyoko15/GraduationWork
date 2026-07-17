using UnityEngine;

public class FortController : MonoBehaviour, IDamage
{
    [SerializeField] public Team state = Team.Independent;
    [HideInInspector] public HPGaugeController hpGaugeController;
    [SerializeField] int maxHp;
    public int hp;

    GameObject filterObject;
    [SerializeField] Material[] filterMaterials;

    public bool attackedFlag;
    public Team attackedTeam;

    void Start()
    {
        hp = maxHp;
        InitializeHPGaugeController();
        filterObject = transform.GetChild(1).gameObject;
        ChangeFilterColor(state);

    }

    void Update()
    {
        if (hp <= 0)
        {
            state = attackedTeam;
            hp = maxHp;
            ChangeFilterColor(state);
            hpGaugeController.SetHp(hp);
        }
    }

    void InitializeHPGaugeController()
    {
        hpGaugeController = transform.GetChild(0).GetComponent<HPGaugeController>();
        hpGaugeController.maxHp = maxHp;
    }

    void ChangeFilterColor(Team state)
    {
        switch (state)
        {
            case Team.Independent:
                filterObject.GetComponent<Renderer>().material = filterMaterials[0];
                break;
            case Team.Ally:
                filterObject.GetComponent<Renderer>().material = filterMaterials[1];
                break;
            case Team.Opponent:
                filterObject.GetComponent<Renderer>().material = filterMaterials[2];
                break;
        }
    }

    public void ActiveFilterObject(bool flag)
    {
        switch (state)
        {
            case Team.Independent:
            case Team.Opponent:
                filterObject.SetActive(flag);
                break;
            case Team.Ally:
                filterObject.SetActive(flag);
                break;
        }
    }

    public void ChangeFortLayer(Team change)
    {
        switch (change)
        {
            case Team.Independent:
                gameObject.layer = 9;
                break;
            case Team.Ally:
                gameObject.layer = 10;
                break;
            case Team.Opponent:
                gameObject.layer = 11;
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        hpGaugeController.SetHp(hp);
    }
}
