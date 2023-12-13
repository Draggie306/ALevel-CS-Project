using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthTest : MonoBehaviour
{

    private int health = 100;
    public TextMeshProUGUI healthText;

    public void TakeDamage(int damage)
    {
        health -= damage;
        healthText.text = health.ToString();
    }


    void Start()
    {
        Debug.Log("Initialised HealthTest.cs");
        StartCoroutine(Wait());
    }

    // Wait for random amount of time (10 seconds max) before taking damage
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(Random.Range(5, 10));
        TakeDamage(1);
    }



    // Update is called once per frame
    void Update()
    {
        healthText.text = "HEALTH: " + health.ToString() + "%";

        if(Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(1);
        }
    }
}
