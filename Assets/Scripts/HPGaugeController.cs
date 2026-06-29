using UnityEngine;
using UnityEngine.UI;

public class HPGaugeController : MonoBehaviour
{
    Billboard canvas;

    public int maxHp;
    int hp;
    Image hpGauge;
    GameObject mainCamera;
    void Start()
    {
        mainCamera = Camera.main.gameObject;
        canvas = transform.GetComponent<Billboard>();
    }

    void Update()
    {
        
    }

    void UpdateHpGauge()
    {
        float amount = Mathf.InverseLerp(0f, maxHp, hp);
        hpGauge.fillAmount = amount;
    }

    public void SetHp(int h)
    {
        hp = h;
        UpdateHpGauge();    
    }
}
