using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTest : MonoBehaviour
{
    [SerializeField] GameObject targetObject;
    List<GameObject> obedList = new ();

    Vector2 move;
    void Start()
    {
        
    }

    void Update()
    {
         if (targetObject != null) transform.position = targetObject.transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (TargetList(other.gameObject))
        {
            if (other.gameObject.tag == "TargetObject")
            {
                if (targetObject != other.gameObject)
                {
                    obedList.Add(targetObject);
                    targetObject = other.gameObject;
                }
            }
        }
    }

    bool TargetList(GameObject target)
    {
        bool flag = true;

        for (int i = 0; i < obedList.Count; i++)
        {
            if (target == obedList[i])
            {
                flag = false;
            }
        }
        return flag;
    }
}
