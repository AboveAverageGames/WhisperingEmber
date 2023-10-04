using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaBottle : MonoBehaviour
{
    public PlayerController PlayerController;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

        private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.currentMana +=50;
            PlayerController.currentMana = Mathf.Clamp(PlayerController.currentMana, 0, PlayerController.maxMana);
            Destroy(gameObject);
        }
    }
}
