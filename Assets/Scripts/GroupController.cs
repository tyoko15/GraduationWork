using System.Collections.Generic;
using UnityEngine;
public enum Team
{
    Independent,
    Ally, 
    Opponent,
}
public class GroupController : MonoBehaviour
{
    [SerializeField] Team team;
    List<Group> groupList = new ();

    GameObject filterObject;
    
    void Start()
    {
        InitializeGroupList();
        filterObject = transform.GetChild(2).gameObject;
    }

    
    void Update()
    {
        
    }

    void InitializeGroupList()
    {
        groupList = new List<Group>();
        GroupData groupInfo;

        for(int i = 0; i < 5; i++)
        {
            groupInfo = transform.GetChild(i).GetComponent<GroupData>();
            groupInfo.gameObject.layer = (team == Team.Ally) ? 7 : 8;
            groupInfo.info.team = team;
            groupInfo.info.number = i;
            //groupInfo.info.type = Type.RaccoonDog;
            //groupInfo.info.indication = Indication.Waiting;
            groupInfo.info.soldierAmount = 20;
            groupList.Add(groupInfo.info);
        }
    }

    public void ActiveFilterObject(bool flag)
    {
        filterObject.SetActive(flag);
    }
}
