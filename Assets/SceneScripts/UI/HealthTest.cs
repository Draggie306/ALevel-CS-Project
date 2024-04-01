using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
/// <summary>
/// This would have been a script to handle the health of the palyer. At the moment, they only take damage every 5-10 seconds.
/// </summary>


public class HealthTest : MonoBehaviour
{

    private int health = 100;
    public TextMeshProUGUI healthText;

    public void TakeDamage(int damage)
    {
        health -= damage;
        // healthText.text = health.ToString();

        if (health <= 0)
        {
            Debug.Log("Player is dead, haven't implemented this yet.");
            SceneManager.LoadScene("MainMenu");
        }

        StartCoroutine(Wait());
    }


    void Start()
    {
        Debug.Log("Initialised HealthTest.cs");
        StartCoroutine(Wait());
    }

    // wait for 5-10 seconds, then take 1 damage
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(Random.Range(5, 10));
        TakeDamage(1);
    }



    // Update is called once per frame
    void Update()
    {
        string completehealthStr = $"HEALTH: {health.ToString()}%";
        healthText.text = completehealthStr;

        /*
        // Test taking damage with spacebar
        if(Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(1);
        }
        */
    }
}
