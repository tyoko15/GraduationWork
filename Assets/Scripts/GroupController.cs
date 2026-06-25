using System.Collections.Generic;
using UnityEngine;

public class GroupController : MonoBehaviour
{
    List<Group> groupList = new ();
    
    void Start()
    {
        InitializeGroupList();
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
            groupList.Add (groupInfo.info);
            groupList[i].number = i;
            groupList[i].indication = Indication.Waiting;
            groupList[i].infantryAmount = 20;
            groupList[i].archerAmount = 20;
            groupList[i].ashigaruAmount = 20;
        }
    }
}
