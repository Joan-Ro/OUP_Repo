using UnityEngine;
using TMPro;
public class PickUP_Counter : MonoBehaviour
{
    public TextMeshProUGUI counterText;
    public TextMeshProUGUI coinText;
    public int pickUpCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Fix: Assign the correct TextMeshProUGUI component instead of 'this'
        counterText = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        coinText.text = "COINS: " + pickUpCount.ToString();
    }

    public void IncreaseCoins()
    {
        pickUpCount += 1;
        coinText.text = "TextCoins" + pickUpCount.ToString();
    }
}
