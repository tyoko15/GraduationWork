using System.Collections;
using UnityEngine;

public enum State
{
    Independent,
    Ally,
    Opponent,
}

public class FortController : MonoBehaviour
{
    [SerializeField] State state = State.Independent;
    [HideInInspector] public HPGaugeController hpGaugeController;
    [SerializeField] int maxHp;
    int hp;

    GameObject filterObject;

    void Start()
    {
        InitializeHPGaugeController();
        filterObject = transform.GetChild(1).gameObject;
    }

    void Update()
    {

    }

    void Damage(int damage)
    {
        hp -= damage;
        hpGaugeController.SetHp(hp);
    }

    void InitializeHPGaugeController()
    {
        hpGaugeController = transform.GetChild(0).GetComponent<HPGaugeController>();
        hpGaugeController.maxHp = maxHp;
    }

    public void ActiveFilterObject(bool flag)
    {
        switch (state)
        {
            case State.Independent:
            case State.Opponent:
                filterObject.SetActive(flag);
                break;
            case State.Ally:
                filterObject.SetActive(flag);
                break;
        }
    }

    public void ChangeFortLayer(State change)
    {
        switch (change)
        {
            case State.Independent:
                gameObject.layer = 9;
                break;
            case State.Ally:
                gameObject.layer = 10;
                break;
            case State.Opponent:
                gameObject.layer = 11;
                break;
        }
    }
}
