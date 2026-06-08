using UnityEngine;

public class S_CameraCountrol : MonoBehaviour
{
    [SerializeField] GameObject player;
    void Start()
    {
        
    }

    void Update()
    {
        Vector3 playerPos = player.transform.position;
        transform.position = new Vector3(playerPos.x, playerPos.y, -10f);
    }
}
