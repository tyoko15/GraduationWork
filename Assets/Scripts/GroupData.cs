using TMPro;
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
    public int number;
    public Type type;
    public Indication indication;
    public int soldierAmount;
}


public class GroupData : MonoBehaviour
{
    public Group info = new Group();

    TextMeshProUGUI soldierAmountText;
    Renderer leaderRenderer;
    [SerializeField] Texture[] leaderTexs;
    GameObject filterObject;


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
    }

    public void ActiveFilterObject(bool flag)
    {
        filterObject.SetActive(flag);
    }
}
