using UnityEngine;
using TMPro;
public class PickUP_Counter : MonoBehaviour
{
    public static PickUP_Counter instance;
    public TextMeshProUGUI coinText;
    public int pickUpCount;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }  
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateCoinText();
    }

    public void IncreaseCoins(int value)
    {
        pickUpCount += value;
        UpdateCoinText();
    }

    private void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = pickUpCount.ToString();
        }
    }
}
