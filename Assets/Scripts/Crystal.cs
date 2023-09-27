using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
    //Visual UI for Crystals Collected
    public TMPro.TextMeshProUGUI crystalsUI;

    //Getting Player Script
    public PlayerController PlayerController;

    // Start is called before the first frame update
    void Start()
    {
        crystalsUI.text = ("Crystals Collected: " + PlayerController.crystalsCollected + "/" + PlayerController.crystalsTotal);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
         

            PlayerController.crystalsCollected++;
            crystalsUI.text = ("Crystals Collected: " + PlayerController.crystalsCollected + "/" + PlayerController.crystalsTotal);
            Destroy(gameObject);

        }
    }
}
