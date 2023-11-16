using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTriggers : MonoBehaviour
{

    [SerializeField] Dialog dialog;
    public bool hasEntered;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && !hasEntered)
            {
            DialogManager.Instance.ShowDialog(dialog);
            hasEntered = true;
            Destroy(gameObject);
        }
    }
}
