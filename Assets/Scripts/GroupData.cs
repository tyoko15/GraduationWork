using UnityEngine;

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
    public Indication indication;
    public int infantryAmount;
    public int archerAmount;
    public int ashigaruAmount;
}


public class GroupData : MonoBehaviour
{
    public Group info { get; private set; } = new Group();

}
