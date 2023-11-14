using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTriggers : MonoBehaviour
{

    [SerializeField] Dialog dialog;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
            {
            DialogManager.Instance.ShowDialog(dialog);
            Debug.Log("WE HAVE COLLISIONNN BOy");
        }
    }
}
