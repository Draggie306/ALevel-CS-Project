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

        if(Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(1);
        }
    }
}
