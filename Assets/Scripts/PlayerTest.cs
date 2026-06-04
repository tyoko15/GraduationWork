using UnityEngine;

public class PlayerTest : MonoBehaviour
{
    [SerializeField] GameObject targetObject;
    void Start()
    {
        
    }

    void Update()
    {
         if (targetObject != null) transform.position = targetObject.transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("aa");
        if (other.gameObject.tag == "TargetObject")
        {
            if (targetObject != other.gameObject)
            {
                targetObject = other.gameObject;
            }
        }
    }
}
