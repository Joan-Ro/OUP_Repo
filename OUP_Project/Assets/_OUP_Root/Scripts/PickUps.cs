using UnityEngine;

public class PickUps : MonoBehaviour
{
    public int value = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Here you can add code to increase the player's score or health
            Debug.Log("Picked up an item worth: " + value);
            Destroy(gameObject); // Remove the pickup from the scene
        }
    }
}
